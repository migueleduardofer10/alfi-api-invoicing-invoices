using Delosi.InvoicingInvoices.Application.Mappings;
using Delosi.InvoicingInvoices.Domain.Entities;
using Delosi.InvoicingInvoices.Infrastructure.Persistence.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Delosi.InvoicingInvoices.Tests.Persistence;

public sealed class ModelTests
{
    private static AppDbContext Context() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql("Host=localhost;Database=not_opened;Username=test").UseSnakeCaseNamingConvention().Options);

    [Fact] public void No_mapea_integracion_ni_columnas_sombra()
    {
        using var context = Context();
        var entities = context.Model.GetEntityTypes().ToArray();
        Assert.Equal(3, entities.Length);
        Assert.DoesNotContain(entities, x => x.GetTableName() == "invoice_integration");
        Assert.Equal(54, entities.Sum(x => x.GetProperties().Count()));
        Assert.DoesNotContain(entities.SelectMany(x => x.GetProperties()), x => x.IsShadowProperty());
    }

    [Fact] public void Conserva_indices_y_checks_sin_indices_implicitos_adicionales()
    {
        using var context = Context();
        var model = context.GetService<IDesignTimeModel>().Model;
        Assert.Equal(12, model.GetEntityTypes().Sum(x => x.GetIndexes().Count()));
        Assert.Equal(11, model.GetEntityTypes().Sum(x => x.GetCheckConstraints().Count()));
        Assert.Equal("timestamp without time zone", model.FindEntityType(typeof(Invoice))!.FindProperty("ModifiedAt")!.GetColumnType());
        Assert.Equal("numeric(18,6)", model.FindEntityType(typeof(InvoiceDetail))!.FindProperty("UnitPrice")!.GetColumnType());
    }

    [Fact] public void Proyecciones_se_traducen_a_SQL_sin_cargar_impuestos_o_integracion()
    {
        using var context = Context();
        var listing = context.Invoices.AsNoTracking().Select(InvoiceMappings.ToListItem).ToQueryString();
        Assert.DoesNotContain("customer_document_number", listing);
        Assert.DoesNotContain("invoice_detail", listing);
        var single = context.Invoices.AsNoTracking().Where(x => x.InvoiceId == 1).Select(InvoiceMappings.ToResponse).ToQueryString();
        Assert.Contains("invoice_detail", single);
        Assert.DoesNotContain("invoice_tax", single);
        Assert.DoesNotContain("invoice_integration", single);
        Assert.Empty(context.ChangeTracker.Entries());
    }
}
