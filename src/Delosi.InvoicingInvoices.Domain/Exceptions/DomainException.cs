namespace Delosi.InvoicingInvoices.Domain.Exceptions;

/// <summary>Excepción base de negocio. El middleware la traduce a una respuesta de error estándar.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>Código de error del catálogo (Application/Errors/errors.json).</summary>
    public abstract string Code { get; }
}
