using Serilog.Context;

namespace Delosi.InvoicingInvoices.Api.Middleware;

/// <summary>
/// Logging transversal: asigna un correlation id por request, lo devuelve en la
/// cabecera de respuesta y lo adjunta a todos los logs emitidos durante el request.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var incoming)
                            && !string.IsNullOrWhiteSpace(incoming)
            ? incoming.ToString()
            : context.TraceIdentifier;

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
