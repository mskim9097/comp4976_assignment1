using MEMS.Shared;

namespace MEMS.Data.Services;

public enum ImageFolder
{
    Candidates,
    Logos
}

// Saves uploaded candidate photos and municipality logos below one root folder (images/candidates, images/logos).
// The Api serves that folder, so the Admin app and the Api must use the same root (Uploads:Path).
public class ImageFileStore(string rootPath)
{
    private static readonly Dictionary<string, (string ContentType, byte[] Signature)> Formats = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = ("image/jpeg", [0xFF, 0xD8, 0xFF]),
        [".jpeg"] = ("image/jpeg", [0xFF, 0xD8, 0xFF]),
        [".png"] = ("image/png", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
        [".gif"] = ("image/gif", "GIF8"u8.ToArray()),
        [".webp"] = ("image/webp", "RIFF"u8.ToArray())
    };

    public string RootPath => rootPath;

    // Checks extension, size, content type and the first bytes of the file, then stores it under a new GUID name.
    public async Task<string> SaveAsync(
        ImageFolder folder, string originalFileName, string contentType, long length, Stream content,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName);
        if (!Formats.TryGetValue(extension, out var format))
        {
            throw new RuleViolationException("Upload a PNG, JPEG, GIF, or WebP image.");
        }
        if (length <= 0)
        {
            throw new RuleViolationException("The uploaded file is empty.");
        }
        if (length > Limits.ImageMaxBytes)
        {
            throw new RuleViolationException("Images must be 5 MB or smaller.");
        }
        if (!string.Equals(contentType, format.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new RuleViolationException("The image content type does not match its file extension.");
        }

        var header = new byte[12];
        var read = await ReadAtLeastAsync(content, header, cancellationToken);
        if (read < header.Length || !HasValidSignature(extension, format.Signature, header))
        {
            throw new RuleViolationException("The uploaded file is not a valid image.");
        }

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var directory = DirectoryFor(folder);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);

        try
        {
            await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await output.WriteAsync(header, cancellationToken);
            // The declared length is not trusted: stop when the real content is over the limit.
            var total = (long)header.Length;
            var buffer = new byte[81920];
            int count;
            while ((count = await content.ReadAsync(buffer, cancellationToken)) > 0)
            {
                total += count;
                if (total > Limits.ImageMaxBytes)
                {
                    throw new RuleViolationException("Images must be 5 MB or smaller.");
                }
                await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
            }
        }
        catch
        {
            File.Delete(path);
            throw;
        }

        return fileName;
    }

    // Only files that were uploaded (GUID names) are removed; the seed images that ship with the Api are never touched.
    public void DeleteUploaded(ImageFolder folder, string? fileName)
    {
        if (fileName is null ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
            !Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _) ||
            !Formats.ContainsKey(Path.GetExtension(fileName)))
        {
            return;
        }

        var path = Path.Combine(DirectoryFor(folder), fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string DirectoryFor(ImageFolder folder) =>
        Path.Combine(rootPath, "images", folder == ImageFolder.Logos ? "logos" : "candidates");

    private static async Task<int> ReadAtLeastAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var total = 0;
        while (total < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(total), cancellationToken);
            if (read == 0)
            {
                break;
            }
            total += read;
        }
        return total;
    }

    private static bool HasValidSignature(string extension, byte[] signature, byte[] header)
    {
        if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        {
            return header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        }
        if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
        {
            return header.AsSpan(0, 6).SequenceEqual("GIF87a"u8) || header.AsSpan(0, 6).SequenceEqual("GIF89a"u8);
        }
        return header.AsSpan(0, signature.Length).SequenceEqual(signature);
    }
}
