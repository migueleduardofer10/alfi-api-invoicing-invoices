namespace Delosi.InvoicingInvoices.Application.DTOs.Invoice;

public sealed record UpdateInvoiceRequest : InvoiceWriteRequest
{
    public string? ModifiedBy { get; init; }
}
