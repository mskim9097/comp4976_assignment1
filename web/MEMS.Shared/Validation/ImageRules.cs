namespace MEMS.Shared.Validation;

public sealed record ImageCheck(bool Ok, string? ContentType, string? Error);

// Image upload restrictions shared by the Admin app and the API: allowed types, size cap and a file-signature
// check (the extension alone is never trusted).
public static class ImageRules
{
    public const int MaxBytes = 2 * 1024 * 1024;
    public const string AllowedDescription = "PNG, JPEG, GIF or WebP, 2 MB or smaller";

    public static ImageCheck Check(string? fileName, ReadOnlySpan<byte> data)
    {
        if (data.Length == 0) return new(false, null, "The image file is empty.");
        if (data.Length > MaxBytes) return new(false, null, "Images must be 2 MB or smaller.");

        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        switch (extension)
        {
            case ".png" when data.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }):
                return new(true, "image/png", null);
            case ".jpg" or ".jpeg" when data.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }):
                return new(true, "image/jpeg", null);
            case ".gif" when data.StartsWith("GIF87a"u8) || data.StartsWith("GIF89a"u8):
                return new(true, "image/gif", null);
            case ".webp" when data.Length >= 12 && data[..4].SequenceEqual("RIFF"u8) && data.Slice(8, 4).SequenceEqual("WEBP"u8):
                return new(true, "image/webp", null);
            case ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp":
                return new(false, null, "The file content does not match its extension, so it is not a valid image.");
            default:
                return new(false, null, "Upload a PNG, JPEG, GIF or WebP image.");
        }
    }
}
