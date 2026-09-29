using Delosi.InvoicingInvoices.Domain.Exceptions;
using Delosi.InvoicingInvoices.Infrastructure.Persistence.Postgres;
using Delosi.InvoicingInvoices.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Delosi.InvoicingInvoices.Tests.Persistence;

public sealed class DatabaseErrorTests
{
    [Fact]
    public async Task Fallo_de_conexion_se_traduce_a_DataAccess_y_no_error_500()
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=invoicing_test;Username=test;Timeout=1;Pooling=false")
            .Options);
        var repository = new InvoiceRepository(db, NullLogger<InvoiceRepository>.Instance);
        await Assert.ThrowsAsync<DataAccessException>(() => repository.GetByIdAsync(1, default));
    }
}
