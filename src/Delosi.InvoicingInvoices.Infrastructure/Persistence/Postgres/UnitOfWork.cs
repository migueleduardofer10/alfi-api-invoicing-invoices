using System.Data;
using Delosi.InvoicingInvoices.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Delosi.InvoicingInvoices.Infrastructure.Persistence.Postgres;

public sealed class UnitOfWork(AppDbContext context, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => DatabaseOperation.RunAsync(() => context.SaveChangesAsync(cancellationToken), logger);

    public Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
        => DatabaseOperation.RunAsync(async () =>
        {
            if (context.Database.CurrentTransaction is not null)
                throw new InvalidOperationException("No se permiten transacciones anidadas.");
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            try
            {
                var result = await operation(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                try { await transaction.RollbackAsync(CancellationToken.None); }
                catch (Exception rollbackError) { logger.LogWarning(rollbackError, "No se pudo confirmar el rollback de la conexión"); }
                context.ChangeTracker.Clear();
                throw;
            }
        }, logger);
}
