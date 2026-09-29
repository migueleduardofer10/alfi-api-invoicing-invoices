namespace Delosi.InvoicingInvoices.Application.Common;

/// <summary>
/// Cuerpo estándar de error de la API. Lo emite únicamente el middleware global
/// de excepciones para que todos los endpoints fallen con la misma forma.
/// </summary>
public sealed record ErrorResponse(
    string Code,
    string Message,
    string TraceId,
    IReadOnlyCollection<ValidationErrorDetail>? Errors = null)
{
    public bool Success => false;
}

/// <summary>Detalle de un error de validación por campo.</summary>
public sealed record ValidationErrorDetail(string Field, string Message);
