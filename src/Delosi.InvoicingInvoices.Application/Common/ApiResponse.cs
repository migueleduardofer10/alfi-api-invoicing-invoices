namespace Delosi.InvoicingInvoices.Application.Common;

/// <summary>Envoltura estándar de respuesta exitosa con datos.</summary>
public sealed record ApiResponse<T>(T Data, bool Success = true, string? Message = null)
{
    public static ApiResponse<T> Ok(T data, string? message = null) => new(data, true, message);
}

/// <summary>Envoltura estándar de respuesta exitosa sin datos (comandos).</summary>
public sealed record ApiResponse(bool Success = true, string? Message = null)
{
    public static ApiResponse Ok(string? message = null) => new(true, message);
}
