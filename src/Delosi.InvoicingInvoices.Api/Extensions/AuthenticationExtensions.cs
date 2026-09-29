using System.Text;

using Delosi.InvoicingInvoices.Api.Options;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Delosi.InvoicingInvoices.Api.Extensions;

/// <summary>
/// Seguridad base de la API: Bearer JWT.
///
/// Admite dos modos de validación de la firma:
///   1) MetadataAddress → descubre las claves públicas del IdP (producción).
///   2) SigningKey      → clave simétrica, útil para pruebas locales sin IdP.
///
/// Con JwtAuth:Enabled = false los servicios se registran igualmente (para que
/// [Authorize] y el pipeline funcionen), pero los endpoints no exigen token.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var options = configuration.GetSection(JwtAuthOptions.SectionName).Get<JwtAuthOptions>()
            ?? new JwtAuthOptions();

        services.Configure<JwtAuthOptions>(configuration.GetSection(JwtAuthOptions.SectionName));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.RequireHttpsMetadata = !environment.IsDevelopment();

                if (!string.IsNullOrWhiteSpace(options.MetadataAddress))
                {
                    jwt.MetadataAddress = options.MetadataAddress;
                }

                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrWhiteSpace(options.ValidIssuer),
                    ValidIssuer = options.ValidIssuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(options.ValidAudience),
                    ValidAudience = options.ValidAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(options.ClockSkewSeconds),
                    ValidateIssuerSigningKey = true,
                };

                if (!string.IsNullOrWhiteSpace(options.SigningKey))
                {
                    jwt.TokenValidationParameters.IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
                }
            });

        services.AddAuthorization();

        return services;
    }
}
