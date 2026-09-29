namespace Delosi.InvoicingInvoices.Domain.Entities;

/// <summary>Mapeo de invoice_detail; sin atributos de persistencia en Domain.</summary>
public sealed class InvoiceDetail
{
    public long InvoiceDetailId { get; set; }
    public long InvoiceId { get; set; }
    public int LineNumber { get; set; }
    public long ConceptId { get; set; }
    public string ConceptCode { get; set; } = string.Empty;
    public long MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public long? AccountingAccountId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; } = 0m;
    public decimal Subtotal { get; set; } = 0m;
    public decimal ServiceCharge { get; set; } = 0m;
    public decimal TaxAmount { get; set; } = 0m;
    public decimal Total { get; set; } = 0m;
    public Invoice Invoice { get; set; } = null!;
    public ICollection<InvoiceTax> Taxes { get; set; } = new List<InvoiceTax>();
}
