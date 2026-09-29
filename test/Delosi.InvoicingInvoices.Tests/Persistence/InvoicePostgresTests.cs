using System.Data.Common;
using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Application.Services;
using Delosi.InvoicingInvoices.Domain.Exceptions;
using Delosi.InvoicingInvoices.Infrastructure.Persistence.Postgres;
using Delosi.InvoicingInvoices.Infrastructure.Persistence.Repositories;
using Delosi.InvoicingInvoices.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace Delosi.InvoicingInvoices.Tests.Persistence;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("INVOICING_TEST_POSTGRES")))
            Skip = "Defina INVOICING_TEST_POSTGRES apuntando a la base desechable invoicing_test.";
    }
}

[CollectionDefinition("Postgres", DisableParallelization = true)]
public sealed class PostgresCollection { }

[Collection("Postgres")]
public sealed class InvoicePostgresTests
{
    private static AppDbContext Context(CommandCounter? counter = null)
    {
        var cs = Environment.GetEnvironmentVariable("INVOICING_TEST_POSTGRES")!;
        if (new NpgsqlConnectionStringBuilder(cs).Database != "invoicing_test")
            throw new InvalidOperationException("Los tests solo admiten una base llamada invoicing_test.");
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(cs).UseSnakeCaseNamingConvention();
        if (counter is not null) options.AddInterceptors(counter);
        return new(options.Options);
    }
    private static InvoiceService Service(AppDbContext db) => new(
        new InvoiceRepository(db, NullLogger<InvoiceRepository>.Instance),
        new UnitOfWork(db, NullLogger<UnitOfWork>.Instance), NullLogger<InvoiceService>.Instance);

    [PostgresFact] public async Task Ciclo_create_update_get_list_y_consultas_acotadas()
    {
        var create = InvoiceFactory.Create();
        InvoiceSavedResponse saved;
        await using (var db = Context()) saved = await Service(db).CreateAsync(create, default);
        Assert.True(saved.InvoiceId > 0);
        Assert.True(saved.Details[0].InvoiceDetailId > 0);
        Assert.Null(saved.ModifiedAt);
        await using (var db = Context())
        {
            var update = InvoiceFactory.Update(create, saved) with { CustomerLegalName = "Cliente actualizado" };
            var result = await Service(db).UpdateAsync(saved.InvoiceId, update, default);
            Assert.NotNull(result.ModifiedAt);
            Assert.Equal(saved.Details[0].InvoiceDetailId, result.Details[0].InvoiceDetailId);
        }
        var counter = new CommandCounter();
        await using var queryDb = Context(counter);
        var service = Service(queryDb);
        var invoice = await service.GetByIdAsync(saved.InvoiceId, default);
        Assert.Equal("Cliente actualizado", invoice.CustomerLegalName);
        Assert.Equal(DateTimeKind.Unspecified, invoice.CreatedAt.Kind);
        Assert.Equal(1, counter.Readers);
        Assert.Empty(queryDb.ChangeTracker.Entries());
        counter.Readers = 0;
        var page = await service.ListAsync(new(CompanyId: create.CompanyId, InvoiceNumber: create.InvoiceNumber), default);
        Assert.Single(page.Items);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(2, counter.Readers);
        Assert.Empty(queryDb.ChangeTracker.Entries());
    }

    [PostgresFact] public async Task Error_de_unicidad_revierte_incluso_la_renumeracion_previa()
    {
        var a = InvoiceFactory.Create() with { Details = [InvoiceFactory.Line(1), InvoiceFactory.Line(2)] };
        var b = InvoiceFactory.Create();
        InvoiceSavedResponse saved;
        await using (var db = Context())
        {
            saved = await Service(db).CreateAsync(a, default);
            await Service(db).CreateAsync(b, default);
        }
        var update = InvoiceFactory.Update(a, saved) with { InvoiceNumber = b.InvoiceNumber };
        update = update with { Details = update.Details.Select(d => d with { LineNumber = 3 - d.LineNumber }).ToArray() };
        await using (var db = Context())
            await Assert.ThrowsAsync<BusinessRuleException>(() => Service(db).UpdateAsync(saved.InvoiceId, update, default));
        await using var verify = Context();
        var result = await Service(verify).GetByIdAsync(saved.InvoiceId, default);
        Assert.Equal(a.InvoiceNumber, result.InvoiceNumber);
        Assert.Null(result.ModifiedAt);
        foreach (var d in result.Details)
            Assert.Equal(saved.Details.Single(x => x.InvoiceDetailId == d.InvoiceDetailId).LineNumber, d.LineNumber);
    }

    [PostgresFact] public async Task Intercambia_numeros_sin_cambiar_ids_y_actualiza_trigger_con_cambio_solo_en_detalle()
    {
        var create = InvoiceFactory.Create() with { Details = [InvoiceFactory.Line(1), InvoiceFactory.Line(2)] };
        InvoiceSavedResponse saved;
        await using (var db = Context()) saved = await Service(db).CreateAsync(create, default);
        var update = InvoiceFactory.Update(create, saved) with { ModifiedBy = null };
        update = update with { Details = update.Details.Select(x => x with { LineNumber = 3 - x.LineNumber }).ToArray() };
        await using (var db = Context()) await Service(db).UpdateAsync(saved.InvoiceId, update, default);
        await using var verify = Context();
        var result = await Service(verify).GetByIdAsync(saved.InvoiceId, default);
        Assert.NotNull(result.ModifiedAt);
        foreach (var detail in result.Details)
            Assert.Equal(3 - saved.Details.Single(x => x.InvoiceDetailId == detail.InvoiceDetailId).LineNumber, detail.LineNumber);
    }

    [PostgresFact] public async Task No_acepta_detalles_ajenos_ni_elimina_lineas()
    {
        var create = InvoiceFactory.Create();
        InvoiceSavedResponse saved;
        await using (var db = Context()) saved = await Service(db).CreateAsync(create, default);
        var update = InvoiceFactory.Update(create, saved);
        await using var edit = Context();
        await Assert.ThrowsAsync<BusinessRuleException>(() => Service(edit).UpdateAsync(saved.InvoiceId,
            update with { Details = [update.Details[0] with { InvoiceDetailId = long.MaxValue }] }, default));
        await Assert.ThrowsAsync<BusinessRuleException>(() => Service(edit).UpdateAsync(saved.InvoiceId,
            update with { Details = [InvoiceFactory.Line()] }, default));
    }

    private sealed class CommandCounter : DbCommandInterceptor
    {
        public int Readers { get; set; }
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Readers++;
            return ValueTask.FromResult(result);
        }
    }
}
