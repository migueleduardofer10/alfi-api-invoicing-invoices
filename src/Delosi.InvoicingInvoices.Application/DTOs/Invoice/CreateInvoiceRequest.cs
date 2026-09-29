namespace Delosi.InvoicingInvoices.Application.DTOs.Invoice;

public sealed record CreateInvoiceRequest : InvoiceWriteRequest
{
    public string? CreatedBy { get; init; }
}
