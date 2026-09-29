namespace Delosi.InvoicingInvoices.Application.DTOs.Invoice;

public sealed record InvoiceResponse
{
    public long InvoiceId { get; init; }
    public long CompanyId { get; init; }
    public long CustomerId { get; init; }
    public long DocumentTypeId { get; init; }
    public long SalesOrganizationId { get; init; }
    public long DistributionChannelId { get; init; }
    public long PaymentTermId { get; init; }
    public long PaymentTypeId { get; init; }
    public long BrandId { get; init; }
    public string BrandCode { get; init; } = string.Empty;
    public long? StoreId { get; init; }
    public string? StoreCode { get; init; }
    public string CustomerDocumentNumber { get; init; } = string.Empty;
    public string CustomerLegalName { get; init; } = string.Empty;
    public string Series { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateOnly IssueDate { get; init; }
    public DateOnly? DueDate { get; init; }
    public string Currency { get; init; } = "PEN";
    public decimal ExchangeRate { get; init; } = 1m;
    public decimal Subtotal { get; init; } = 0m;
    public decimal Discount { get; init; } = 0m;
    public decimal ServiceChargeTotal { get; init; } = 0m;
    public decimal TaxTotal { get; init; } = 0m;
    public decimal Total { get; init; } = 0m;
    public string Status { get; init; } = "REGISTERED";
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public IReadOnlyList<InvoiceDetailResponse> Details { get; init; } = [];
}
