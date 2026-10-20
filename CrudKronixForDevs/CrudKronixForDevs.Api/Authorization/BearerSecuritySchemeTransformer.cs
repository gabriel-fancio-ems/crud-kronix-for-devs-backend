using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CrudKronixForDevs.Api.Authorization
{
    /// <summary>
    /// Ensina o documento OpenAPI que a API usa token Bearer.
    /// </summary>
    public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        private const string SchemeName = "Bearer";

        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Faça POST /api/auth/login, copie o accessToken e cole aqui (sem a palavra Bearer)."
            };

            document.Security ??= [];
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SchemeName, document)] = []
            });

            return Task.CompletedTask;
        }
    }
}