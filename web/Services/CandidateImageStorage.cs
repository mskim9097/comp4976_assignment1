using Microsoft.AspNetCore.Http;

namespace MunicipalElections.Web.Services;

public static class CandidateImageStorage
{
    public const long MaximumFileSize = 5 * 1024 * 1024;

    private static readonly Dictionary<string, (string ContentType, byte[] Signature)> ImageFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = ("image/jpeg", [0xFF, 0xD8, 0xFF]),
        [".jpeg"] = ("image/jpeg", [0xFF, 0xD8, 0xFF]),
        [".png"] = ("image/png", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
        [".gif"] = ("image/gif", "GIF8"u8.ToArray()),
        [".webp"] = ("image/webp", "RIFF"u8.ToArray())
    };

    public static async Task<string?> SaveAsync(IFormFile? upload, string webRootPath, CancellationToken cancellationToken = default)
    {
        if (upload is null || upload.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(upload.FileName);
        if (!ImageFormats.TryGetValue(extension, out var format))
        {
            throw new InvalidDataException("Upload a PNG, JPEG, GIF, or WebP image.");
        }

        if (upload.Length > MaximumFileSize)
        {
            throw new InvalidDataException("Candidate images must be 5 MB or smaller.");
        }

        if (!string.Equals(upload.ContentType, format.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("The image content type does not match its file extension.");
        }

        var signatureLength = extension.Equals(".webp", StringComparison.OrdinalIgnoreCase)
            ? 12
            : extension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
                ? 6
                : format.Signature.Length;
        if (upload.Length < signatureLength)
        {
            throw new InvalidDataException("The uploaded file is not a valid image.");
        }

        var header = new byte[signatureLength];
        await using (var input = upload.OpenReadStream())
        {
            var bytesRead = await input.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
            if (bytesRead != header.Length || !HasValidSignature(extension, format.Signature, header))
            {
                throw new InvalidDataException("The uploaded file is not a valid image.");
            }
        }

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var imageDirectory = Path.Combine(webRootPath, "images", "candidates");
        Directory.CreateDirectory(imageDirectory);

        var destination = Path.Combine(imageDirectory, fileName);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await upload.CopyToAsync(output, cancellationToken);
        return fileName;
    }

    public static void DeleteUploaded(string? fileName, string webRootPath)
    {
        if (fileName is null ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
            !Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _) ||
            !ImageFormats.ContainsKey(Path.GetExtension(fileName)))
        {
            return;
        }

        var imagePath = Path.Combine(webRootPath, "images", "candidates", fileName);
        if (File.Exists(imagePath))
        {
            File.Delete(imagePath);
        }
    }

    private static bool HasValidSignature(string extension, byte[] signature, byte[] header)
    {
        if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        {
            return header.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                   header.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        }

        if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
        {
            return header.AsSpan(0, 6).SequenceEqual("GIF87a"u8) ||
                   header.AsSpan(0, 6).SequenceEqual("GIF89a"u8);
        }

        return header.AsSpan(0, signature.Length).SequenceEqual(signature);
    }
}
