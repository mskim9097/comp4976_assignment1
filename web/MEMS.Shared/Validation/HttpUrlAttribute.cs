using System.ComponentModel.DataAnnotations;

namespace MEMS.Shared.Validation;

// [Url] also accepts ftp:// and similar; candidate websites must be plain http(s) links.
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class HttpUrlAttribute : ValidationAttribute
{
    public HttpUrlAttribute() : base("Enter a valid web address starting with http:// or https://.") { }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string text || text.Length == 0) return true; // optional field
        return Uri.TryCreate(text, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
