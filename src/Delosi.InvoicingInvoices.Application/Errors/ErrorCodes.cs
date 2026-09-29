namespace Delosi.InvoicingInvoices.Application.Errors;

/// <summary>Códigos del catálogo de errores. Deben existir como clave en errors.json.</summary>
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
    public const string DatabaseError = "DATABASE_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InternalError = "INTERNAL_ERROR";
}
