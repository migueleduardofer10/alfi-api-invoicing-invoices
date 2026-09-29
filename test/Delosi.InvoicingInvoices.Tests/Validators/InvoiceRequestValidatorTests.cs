using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Application.Validators;
using Delosi.InvoicingInvoices.Tests.Common;

namespace Delosi.InvoicingInvoices.Tests.Validators;

public sealed class InvoiceRequestValidatorTests
{
    private readonly CreateInvoiceRequestValidator _validator = new();

    [Fact] public void Acepta_factura_valida() => Assert.True(_validator.Validate(InvoiceFactory.Create()).IsValid);

    [Theory]
    [InlineData("118.01", true)]
    [InlineData("118.02", false)]
    public void Respeta_tolerancia_exacta_del_sql(string total, bool valid)
        => Assert.Equal(valid, _validator.Validate(InvoiceFactory.Create() with
        { Total = decimal.Parse(total, System.Globalization.CultureInfo.InvariantCulture) }).IsValid);

    [Fact] public void No_redondea_silenciosamente_precisiones_excesivas()
    {
        var request = InvoiceFactory.Create() with { Details = [InvoiceFactory.Line() with { UnitPrice = 1.1234567m }] };
        Assert.Contains(_validator.Validate(request).Errors, e => e.PropertyName.EndsWith("UnitPrice"));
    }

    [Fact] public void Null_en_coleccion_produce_validacion_sin_excepcion()
    {
        Assert.False(_validator.Validate(InvoiceFactory.Create() with { Details = null! }).IsValid);
        Assert.False(_validator.Validate(InvoiceFactory.Create() with { Details = [null!] }).IsValid);
    }

    [Fact] public void Rechaza_numeros_de_linea_duplicados()
        => Assert.False(_validator.Validate(InvoiceFactory.Create() with { Details = [InvoiceFactory.Line(), InvoiceFactory.Line()] }).IsValid);

    [Fact] public void Rechaza_ids_inyectados_en_crear()
        => Assert.False(_validator.Validate(InvoiceFactory.Create() with { Details = [InvoiceFactory.Line() with { InvoiceDetailId = 99 }] }).IsValid);

    [Fact] public void Rechaza_fechas_invertidas()
        => Assert.False(_validator.Validate(InvoiceFactory.Create() with { DueDate = new DateOnly(2020, 1, 1) }).IsValid);

    [Fact] public void Overflow_en_importes_es_validacion_y_no_500()
        => Assert.False(_validator.Validate(InvoiceFactory.Create() with { Subtotal = decimal.MaxValue, TaxTotal = decimal.MaxValue }).IsValid);

    [Fact] public void Limita_paginacion_y_ordenamiento()
    {
        var validator = new InvoiceFilterRequestValidator();
        Assert.False(validator.Validate(new InvoiceFilterRequest(Page: int.MaxValue, PageSize: 100)).IsValid);
        Assert.False(validator.Validate(new InvoiceFilterRequest(PageSize: 101)).IsValid);
        Assert.False(validator.Validate(new InvoiceFilterRequest(SortBy: "total; DROP TABLE invoice")).IsValid);
        Assert.True(validator.Validate(new InvoiceFilterRequest()).IsValid);
    }
}
