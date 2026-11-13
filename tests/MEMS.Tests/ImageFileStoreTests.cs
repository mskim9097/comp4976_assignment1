using MEMS.Data.Services;
using MEMS.Shared;

namespace MEMS.Tests;

// Image upload restrictions (type, size, content type and the first bytes of the file).
public class ImageFileStoreTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0, 1, 2, 3];

    private static Task<string> Save(ImageFileStore store, string name, string contentType, byte[] bytes, long? length = null) =>
        store.SaveAsync(ImageFolder.Candidates, name, contentType, length ?? bytes.Length, new MemoryStream(bytes));

    [Fact]
    public async Task A_real_png_is_stored_under_a_guid_name()
    {
        using var host = new TestHost();
        var fileName = await Save(host.Images, "me.png", "image/png", Png);

        Assert.EndsWith(".png", fileName);
        Assert.True(Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _));
        Assert.True(File.Exists(Path.Combine(host.UploadFolder, "images", "candidates", fileName)));
    }

    [Fact]
    public async Task Wrong_extension_content_type_signature_or_size_are_refused()
    {
        using var host = new TestHost();
        var store = host.Images;

        await Assert.ThrowsAsync<RuleViolationException>(() => Save(store, "run.exe", "application/octet-stream", Png));
        await Assert.ThrowsAsync<RuleViolationException>(() => Save(store, "me.png", "image/jpeg", Png));
        await Assert.ThrowsAsync<RuleViolationException>(() => Save(store, "fake.png", "image/png", "this is not an image"u8.ToArray()));
        await Assert.ThrowsAsync<RuleViolationException>(() => Save(store, "empty.png", "image/png", [], 0));
        await Assert.ThrowsAsync<RuleViolationException>(() => Save(store, "big.png", "image/png", Png, Limits.ImageMaxBytes + 1));
    }

    [Fact]
    public async Task The_real_size_is_checked_not_only_the_declared_one()
    {
        using var host = new TestHost();
        var big = new byte[Limits.ImageMaxBytes + 1000];
        Png.CopyTo(big, 0);

        await Assert.ThrowsAsync<RuleViolationException>(() => Save(host.Images, "lies.png", "image/png", big, length: 100));
        // nothing is left behind
        var folder = Path.Combine(host.UploadFolder, "images", "candidates");
        Assert.True(!Directory.Exists(folder) || Directory.GetFiles(folder).Length == 0);
    }

    [Fact]
    public async Task Only_uploaded_files_are_deleted_never_the_seed_images()
    {
        using var host = new TestHost();
        var store = host.Images;
        var uploaded = await Save(store, "me.png", "image/png", Png);

        var seedPath = Path.Combine(host.UploadFolder, "images", "candidates", "c01.png");
        File.WriteAllBytes(seedPath, Png);

        store.DeleteUploaded(ImageFolder.Candidates, uploaded);
        store.DeleteUploaded(ImageFolder.Candidates, "c01.png");
        store.DeleteUploaded(ImageFolder.Candidates, "../../secret.png");

        Assert.False(File.Exists(Path.Combine(host.UploadFolder, "images", "candidates", uploaded)));
        Assert.True(File.Exists(seedPath));
    }
}
