using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace AiCallCenter.Api.OpenApi;

public sealed class BearerSecuritySchemeTransformer
    : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider
        _authenticationSchemeProvider;

    public BearerSecuritySchemeTransformer(
        IAuthenticationSchemeProvider authenticationSchemeProvider)
    {
        _authenticationSchemeProvider =
            authenticationSchemeProvider;
    }

    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes =
            await _authenticationSchemeProvider
                .GetAllSchemesAsync();

        var bearerSchemeExists =
            authenticationSchemes.Any(scheme =>
                scheme.Name ==
                JwtBearerDefaults.AuthenticationScheme);

        if (!bearerSchemeExists)
        {
            return;
        }

        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??=
            new Dictionary<string, OpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "Login işleminden aldığınız JWT tokenını girin."
            };

        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                operation.Security ??=
                    new List<OpenApiSecurityRequirement>();

                operation.Security.Add(
                    new OpenApiSecurityRequirement
                    {
                        [
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type =
                                        ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            }
                        ] = Array.Empty<string>()
                    });
            }
        }
    }
}