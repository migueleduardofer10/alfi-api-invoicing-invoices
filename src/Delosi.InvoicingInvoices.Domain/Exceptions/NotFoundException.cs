namespace Delosi.InvoicingInvoices.Domain.Exceptions;

/// <summary>El recurso solicitado no existe. Se traduce a HTTP 404.</summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string resource, object key)
        : base($"No se encontró {resource} con identificador '{key}'.") { }

    public override string Code => "RESOURCE_NOT_FOUND";
}
