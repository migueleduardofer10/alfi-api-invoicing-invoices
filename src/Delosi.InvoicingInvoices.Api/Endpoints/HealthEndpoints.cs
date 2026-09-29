using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Delosi.InvoicingInvoices.Api.Endpoints;

/// <summary>
/// Health checks sin versionado ni autenticación, para balanceadores y monitoreo.
///   /health       → la app responde (liveness).
///   /health/ready → además, PostgreSQL acepta conexiones (readiness).
/// </summary>
public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "Healthy",
            service = "Delosi.InvoicingInvoices.Api",
            timestamp = DateTimeOffset.UtcNow,
        }))
        .WithName("Health")
        .WithTags("Health")
        .AllowAnonymous()
        .ExcludeFromDescription();

        app.MapGet("/health/ready", async (HealthCheckService healthChecks, CancellationToken ct) =>
        {
            var report = await healthChecks.CheckHealthAsync(
                registration => registration.Tags.Contains("ready"),
                ct);

            var payload = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                }),
            };

            return report.Status == HealthStatus.Healthy
                ? Results.Ok(payload)
                : Results.Json(payload, statusCode: StatusCodes.Status503ServiceUnavailable);
        })
        .WithName("HealthReady")
        .WithTags("Health")
        .AllowAnonymous()
        .ExcludeFromDescription();

        return app;
    }
}
