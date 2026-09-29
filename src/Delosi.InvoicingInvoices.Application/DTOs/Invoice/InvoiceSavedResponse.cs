namespace Delosi.InvoicingInvoices.Application.DTOs.Invoice;

public sealed record InvoiceSavedResponse(long InvoiceId, DateTime CreatedAt, DateTime? ModifiedAt,
    IReadOnlyList<InvoiceDetailIdentityResponse> Details);

public sealed record InvoiceDetailIdentityResponse(long InvoiceDetailId, int LineNumber);
