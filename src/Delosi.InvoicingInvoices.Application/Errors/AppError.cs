using System.Reflection;
using System.Text.Json;

namespace Delosi.InvoicingInvoices.Application.Errors;

/// <summary>
/// Error de aplicación resuelto contra el catálogo embebido (errors.json).
/// Centraliza código, mensaje al usuario y status HTTP en un único lugar.
/// </summary>
public sealed record AppError(string Code, string Message, int HttpStatus)
{
    private static readonly IReadOnlyDictionary<string, AppError> Catalog = LoadCatalog();

    /// <summary>Obtiene el error del catálogo; si el código no existe cae a INTERNAL_ERROR.</summary>
    public static AppError For(string code)
        => Catalog.TryGetValue(code, out var error) ? error : Catalog[ErrorCodes.InternalError];

    /// <summary>Error del catálogo con mensaje específico (por ejemplo, el devuelto por un SP).</summary>
    public static AppError For(string code, string message)
        => For(code) with { Message = message };

    public static AppError Validation(string message)
        => For(ErrorCodes.ValidationError, message);

    private static IReadOnlyDictionary<string, AppError> LoadCatalog()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .First(name => name.EndsWith("errors.json", StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var document = JsonDocument.Parse(stream);

        return document.RootElement.EnumerateObject()
            .ToDictionary(
                property => property.Name,
                property => new AppError(
                    property.Name,
                    property.Value.GetProperty("message").GetString()!,
                    property.Value.GetProperty("httpStatus").GetInt32()),
                StringComparer.Ordinal);
    }
}
