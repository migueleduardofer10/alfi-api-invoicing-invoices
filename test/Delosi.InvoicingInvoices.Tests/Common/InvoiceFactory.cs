using System.Text.Json;
using Delosi.InvoicingInvoices.Application.DTOs.Invoice;

namespace Delosi.InvoicingInvoices.Tests.Common;

public static class InvoiceFactory
{
    private static int _number = Random.Shared.Next(1_000_000, 90_000_000);
    public static CreateInvoiceRequest Create() => new()
    {
        CompanyId = 900001, CustomerId = 1, DocumentTypeId = 1, SalesOrganizationId = 1,
        DistributionChannelId = 1, PaymentTermId = 1, PaymentTypeId = 1, BrandId = 1,
        BrandCode = "KFC", CustomerDocumentNumber = "20123456789", CustomerLegalName = "Cliente de pruebas",
        Series = "T001", InvoiceNumber = Interlocked.Increment(ref _number).ToString("D8"),
        IssueDate = new DateOnly(2026, 9, 15), DueDate = new DateOnly(2026, 10, 15),
        Currency = "PEN", ExchangeRate = 1m, Subtotal = 100m, TaxTotal = 18m, Total = 118m,
        CreatedBy = "test", Details = [Line()]
    };

    public static InvoiceDetailRequest Line(int number = 1) => new()
    {
        LineNumber = number, ConceptId = 1, ConceptCode = "CON01", MaterialId = 1, MaterialCode = "MAT01",
        Description = "Servicio de prueba", Quantity = 1, UnitPrice = 100, Subtotal = 100, TaxAmount = 18, Total = 118
    };

    public static UpdateInvoiceRequest Update(CreateInvoiceRequest source, InvoiceSavedResponse saved)
        => JsonSerializer.Deserialize<UpdateInvoiceRequest>(JsonSerializer.Serialize(source))! with
        {
            ModifiedBy = "test-update",
            Details = source.Details.Select(x => x with
            {
                InvoiceDetailId = saved.Details.Single(d => d.LineNumber == x.LineNumber).InvoiceDetailId
            }).ToArray()
        };
}
