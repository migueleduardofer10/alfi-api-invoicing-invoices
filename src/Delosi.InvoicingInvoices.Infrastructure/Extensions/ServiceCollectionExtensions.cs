using Delosi.InvoicingInvoices.Application.Interfaces.Repositories;
using Delosi.InvoicingInvoices.Infrastructure.HealthChecks;
using Delosi.InvoicingInvoices.Infrastructure.Options;
using Delosi.InvoicingInvoices.Infrastructure.Persistence.Postgres;
using Delosi.InvoicingInvoices.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Delosi.InvoicingInvoices.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new();
        if (options.CommandTimeoutSeconds is < 1 or > 120 || options.MaxBatchSize is < 1 or > 1000)
            throw new InvalidOperationException("Database:CommandTimeoutSeconds o Database:MaxBatchSize no válidos.");
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Debe configurar ConnectionStrings:Postgres.");

        services.AddDbContext<AppDbContext>(db => db.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.CommandTimeout(options.CommandTimeoutSeconds);
            npgsql.MaxBatchSize(options.MaxBatchSize);
            // No se reintenta una escritura cuyo COMMIT podría haberse completado.
        }).UseSnakeCaseNamingConvention());
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHealthChecks().AddCheck<PostgresHealthCheck>("postgres", tags: ["ready", "db"]);
        return services;
    }
}
