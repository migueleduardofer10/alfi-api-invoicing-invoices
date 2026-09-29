using Delosi.InvoicingInvoices.Infrastructure.Persistence.Postgres;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Delosi.InvoicingInvoices.Infrastructure.HealthChecks;

public sealed class PostgresHealthCheck(AppDbContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthCheckContext, CancellationToken cancellationToken = default)
        => await context.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("PostgreSQL no está disponible.");
}
