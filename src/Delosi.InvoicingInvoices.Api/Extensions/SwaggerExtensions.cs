using System.Reflection;

using Microsoft.OpenApi.Models;

namespace Delosi.InvoicingInvoices.Api.Extensions;

/// <summary>Swagger/OpenAPI base con soporte para el esquema Bearer.</summary>
public static class SwaggerExtensions
{
    private const string SecuritySchemeId = "Bearer";

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(swagger =>
        {
            swagger.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Delosi — Facturas",
                Version = "v1",
                Description =
                    "Crear, actualizar, consultar y listar facturas.",
                Contact = new OpenApiContact { Name = "DELOSI S.A." },
            });

            swagger.AddSecurityDefinition(SecuritySchemeId, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Token JWT. Formato: Bearer {token}",
            });

            swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = SecuritySchemeId,
                        },
                    },
                    Array.Empty<string>()
                },
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                swagger.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(ui =>
        {
            ui.SwaggerEndpoint("/swagger/v1/swagger.json", "Invoices API v1");
            ui.DocumentTitle = "Delosi — Facturas";
        });

        return app;
    }
}
