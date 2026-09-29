namespace Delosi.InvoicingInvoices.Domain.Exceptions;

/// <summary>Conflicto con una regla de negocio o una restricción existente. HTTP 409.</summary>
public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message) { }

    public override string Code => "BUSINESS_RULE_VIOLATION";
}
