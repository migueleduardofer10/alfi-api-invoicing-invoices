namespace Delosi.InvoicingInvoices.Domain.Entities;

/// <summary>Mapeo de invoice; sin atributos de persistencia en Domain.</summary>
public sealed class Invoice
{
    public long InvoiceId { get; set; }
    public long CompanyId { get; set; }
    public long CustomerId { get; set; }
    public long DocumentTypeId { get; set; }
    public long SalesOrganizationId { get; set; }
    public long DistributionChannelId { get; set; }
    public long PaymentTermId { get; set; }
    public long PaymentTypeId { get; set; }
    public long BrandId { get; set; }
    public string BrandCode { get; set; } = string.Empty;
    public long? StoreId { get; set; }
    public string? StoreCode { get; set; }
    public string CustomerDocumentNumber { get; set; } = string.Empty;
    public string CustomerLegalName { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Currency { get; set; } = "PEN";
    public decimal ExchangeRate { get; set; } = 1m;
    public decimal Subtotal { get; set; } = 0m;
    public decimal Discount { get; set; } = 0m;
    public decimal ServiceChargeTotal { get; set; } = 0m;
    public decimal TaxTotal { get; set; } = 0m;
    public decimal Total { get; set; } = 0m;
    public string Status { get; set; } = "REGISTERED";
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public ICollection<InvoiceDetail> Details { get; set; } = new List<InvoiceDetail>();
    public ICollection<InvoiceTax> Taxes { get; set; } = new List<InvoiceTax>();
}
