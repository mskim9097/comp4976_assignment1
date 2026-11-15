using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MEMS.Api.Infrastructure;

public static class OpenApiExtensions
{
    private const string BearerScheme = "Bearer";

    // OpenAPI document with a JWT "Authorize" button; endpoints marked [Authorize] show the lock.
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.ShouldInclude = description =>
                description.RelativePath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true;

            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "MEMS API",
                    Version = "v1",
                    Description = "Municipal Elections Management System. Every municipality is a tenant: data routes look like " +
                        "/api/{tenant}/candidates (for example /api/burnaby/candidates). Log in with POST /api/auth/login, " +
                        "press Authorize and paste the token to call the administrator endpoints."
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
                var requiresLogin = context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any()
                    && !context.Description.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();
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
