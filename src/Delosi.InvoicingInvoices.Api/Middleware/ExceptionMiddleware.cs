using System.Text.Json;

using Delosi.InvoicingInvoices.Application.Common;
using Delosi.InvoicingInvoices.Application.Errors;
using Delosi.InvoicingInvoices.Domain.Exceptions;

using FluentValidation;

namespace Delosi.InvoicingInvoices.Api.Middleware;

/// <summary>
/// Manejo de errores estándar: cualquier excepción no controlada se traduce a un
/// <see cref="ErrorResponse"/> con código de catálogo, mensaje seguro y traceId.
/// Ningún endpoint debe formatear errores por su cuenta.
/// </summary>
public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException ex)
        {
            logger.LogWarning("Solicitud HTTP inválida: {StatusCode}", ex.StatusCode);
            await WriteAsync(context, AppError.Validation("Revise el JSON, la ruta y los parámetros de consulta.")
                with { HttpStatus = ex.StatusCode });
        }
        catch (JsonException)
        {
            await WriteAsync(context, AppError.Validation("El cuerpo no es un JSON válido para esta operación."));
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Error de validación: {Message}", ex.Message);

            var details = ex.Errors
                .Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage))
                .ToList();

            await WriteAsync(context, AppError.For(ErrorCodes.ValidationError), details);
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning("Recurso no encontrado: {Message}", ex.Message);
            await WriteAsync(context, AppError.For(ex.Code, ex.Message));
        }
        catch (BusinessRuleException ex)
        {
            logger.LogWarning("Regla de negocio incumplida: {Message}", ex.Message);
            await WriteAsync(context, AppError.For(ex.Code, ex.Message));
        }
        catch (DataAccessException ex)
        {
            // La causa de PostgreSQL se conserva solo en los logs.
            logger.LogError(ex, "Error de acceso a datos");
            await WriteAsync(context, AppError.For(ex.Code));
        }
        catch (UnauthorizedAccessException)
        {
            await WriteAsync(context, AppError.For(ErrorCodes.Unauthorized));
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // El cliente cortó la conexión: no hay a quién responder.
            logger.LogInformation("Request cancelado por el cliente: {Path}", context.Request.Path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Excepción no controlada en {Path}", context.Request.Path);
            await WriteAsync(context, AppError.For(ErrorCodes.InternalError));
        }
    }

    internal static Task WriteStatusAsync(HttpContext context)
    {
        var error = context.Response.StatusCode switch
        {
            401 => AppError.For(ErrorCodes.Unauthorized),
            403 => AppError.For(ErrorCodes.Forbidden),
            404 => AppError.For(ErrorCodes.ResourceNotFound),
            405 => AppError.Validation("El método HTTP no está disponible en esta ruta.") with { HttpStatus = 405 },
            415 => AppError.Validation("Content-Type debe ser application/json.") with { HttpStatus = 415 },
            _ => AppError.Validation("La solicitud no pudo ser procesada.") with { HttpStatus = context.Response.StatusCode }
        };
        return WriteAsync(context, error);
    }

    private static async Task WriteAsync(
        HttpContext context,
        AppError error,
        IReadOnlyCollection<ValidationErrorDetail>? details = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = error.HttpStatus;
        context.Response.ContentType = "application/json; charset=utf-8";

        var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var value)
            ? value?.ToString() ?? context.TraceIdentifier
            : context.TraceIdentifier;

        var body = new ErrorResponse(error.Code, error.Message, correlationId, details);

        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionMiddleware>();
}
