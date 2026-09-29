namespace Delosi.InvoicingInvoices.Api.Options;

/// <summary>Configuración de la autenticación JWT (sección "JwtAuth" del appsettings).</summary>
public sealed class JwtAuthOptions
{
    public const string SectionName = "JwtAuth";

    /// <summary>
    /// Activa o desactiva la exigencia de token en los endpoints de negocio.
    /// En Development se deja en false para poder probar con Swagger sin IdP.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>URL del documento OpenID Connect del IdP (validación por clave pública).</summary>
    public string? MetadataAddress { get; set; }

    /// <summary>Emisor esperado. Si está vacío no se valida el issuer.</summary>
    public string? ValidIssuer { get; set; }

    /// <summary>Audiencia esperada del token.</summary>
    public string? ValidAudience { get; set; }

    /// <summary>
    /// Clave simétrica de firma. Alternativa a MetadataAddress para entornos sin IdP
    /// (pruebas locales). Nunca debe versionarse: va en Secrets Manager.
    /// </summary>
    public string? SigningKey { get; set; }

    /// <summary>Tolerancia de reloj en segundos al validar la expiración.</summary>
    public int ClockSkewSeconds { get; set; } = 60;
}
