using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MunicipalElections.Web.Extensions;

public static class OpenApiExtensions
{
    private const string BearerScheme = "Bearer";

    // OpenAPI document with a JWT "Authorize" button; only endpoints marked [Authorize] show the lock.
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            // Document only the JSON API; the MVC pages are not part of it.
            options.ShouldInclude = description =>
                description.RelativePath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true;

            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Municipal Elections API",
                    Version = "v1",
                    Description = "Municipalities, positions and candidates, plus a personal voting guide for logged-in users. "
                        + "Log in with POST /api/auth/login, then press Authorize and paste the token."
                };
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT returned by POST /api/auth/login"
                };
                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var requiresLogin = context.Description.ActionDescriptor.EndpointMetadata
                    .OfType<IAuthorizeData>().Any();
                if (requiresLogin)
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(BearerScheme, context.Document)] = []
                    });
                }
                return Task.CompletedTask;
            });
        });
        return services;
    }
}
