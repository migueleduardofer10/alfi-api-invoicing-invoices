namespace Delosi.InvoicingInvoices.Domain.Entities;

/// <summary>Mapeo de invoice_tax; sin atributos de persistencia en Domain.</summary>
public sealed class InvoiceTax
{
    public long InvoiceTaxId { get; set; }
    public long InvoiceId { get; set; }
    public long? InvoiceDetailId { get; set; }
    public string TaxType { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal TaxableBase { get; set; }
    public decimal Amount { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public InvoiceDetail? InvoiceDetail { get; set; }
}
