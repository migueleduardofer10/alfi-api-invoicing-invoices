namespace Delosi.InvoicingInvoices.Domain.Exceptions;

/// <summary>Fallo de PostgreSQL. El detalle técnico nunca se devuelve al cliente. HTTP 503.</summary>
public sealed class DataAccessException : DomainException
{
    public DataAccessException(string message, Exception innerException)
        : base(message, innerException) { }

    public override string Code => "DATABASE_ERROR";
}
