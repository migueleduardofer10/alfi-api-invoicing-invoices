namespace Delosi.InvoicingInvoices.Application.DTOs.Invoice;

public sealed record InvoiceDetailRequest
{
    /// <summary>Null para una línea nueva; en PUT, ID devuelto por Crear o Consultar.</summary>
    public long? InvoiceDetailId { get; init; }
    public int LineNumber { get; init; }
    public long ConceptId { get; init; }
    public string ConceptCode { get; init; } = string.Empty;
    public long MaterialId { get; init; }
    public string MaterialCode { get; init; } = string.Empty;
    public long? AccountingAccountId { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Discount { get; init; } = 0m;
    public decimal Subtotal { get; init; } = 0m;
    public decimal ServiceCharge { get; init; } = 0m;
    public decimal TaxAmount { get; init; } = 0m;
    public decimal Total { get; init; } = 0m;
}
