using System.ComponentModel.DataAnnotations;

namespace MEMS.Shared;

// [Url] also accepts ftp:// and similar; a candidate website must be a plain http(s) link. An empty value is allowed.
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class HttpUrlAttribute : ValidationAttribute
{
    public HttpUrlAttribute() : base("Enter a valid website URL starting with http:// or https://.") { }

    public override bool IsValid(object? value)
    {
        if (value is null || (value is string empty && empty.Length == 0))
        {
            return true;
        }

        if (value is not string text)
        {
            return false;
        }

        return Uri.TryCreate(text, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
