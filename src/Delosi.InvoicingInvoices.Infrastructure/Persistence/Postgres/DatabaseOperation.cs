using Delosi.InvoicingInvoices.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Delosi.InvoicingInvoices.Infrastructure.Persistence.Postgres;

internal static class DatabaseOperation
{
    public static async Task<T> RunAsync<T>(Func<Task<T>> operation, ILogger logger)
    {
        try { return await operation(); }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessRuleException("El recurso cambió durante la operación; consulte de nuevo antes de reintentar.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            throw Translate(pg, logger);
        }
        catch (PostgresException ex) { throw Translate(ex, logger); }
        catch (InvalidOperationException ex) when (FindDatabaseCause(ex) is not null)
        {
            // La estrategia no reintentable de Npgsql puede envolver un fallo transitorio.
            if (FindDatabaseCause(ex) is PostgresException pg) throw Translate(pg, logger);
            logger.LogError(ex, "Fallo transitorio de PostgreSQL");
            throw new DataAccessException("PostgreSQL no está disponible para completar la operación.", ex);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error de persistencia PostgreSQL");
            throw new DataAccessException("No se pudo persistir la factura.", ex);
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Error de comunicación con PostgreSQL");
            throw new DataAccessException("No se pudo completar la operación PostgreSQL.", ex);
        }
        catch (TimeoutException ex)
        {
            logger.LogError(ex, "Timeout PostgreSQL");
            throw new DataAccessException("La operación PostgreSQL excedió el tiempo disponible.", ex);
        }
    }

    private static Exception? FindDatabaseCause(Exception exception)
    {
        for (Exception? cause = exception.InnerException; cause is not null; cause = cause.InnerException)
            if (cause is NpgsqlException or TimeoutException) return cause;
        return null;
    }

    private static Exception Translate(PostgresException ex, ILogger logger)
    {
        logger.LogWarning("PostgreSQL rechazó la operación. SqlState={SqlState} Constraint={Constraint}", ex.SqlState, ex.ConstraintName);
        return ex.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation when ex.ConstraintName == "uq_invoice_document" =>
                new BusinessRuleException("Ya existe una factura con la misma sociedad, tipo de documento, serie y número."),
            PostgresErrorCodes.UniqueViolation => new BusinessRuleException("La operación genera un valor duplicado en una clave única."),
            PostgresErrorCodes.CheckViolation => new BusinessRuleException("Los datos no cumplen una restricción existente de la factura."),
            PostgresErrorCodes.ForeignKeyViolation => new BusinessRuleException("La operación afecta una referencia de datos existente."),
            PostgresErrorCodes.NotNullViolation or PostgresErrorCodes.NumericValueOutOfRange or PostgresErrorCodes.StringDataRightTruncation =>
                new BusinessRuleException("Uno de los valores no cumple el tipo, tamaño o nulabilidad de la columna."),
            PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected =>
                new BusinessRuleException("La operación entró en conflicto con otra actualización; consulte de nuevo y reintente."),
            _ => new DataAccessException("La operación PostgreSQL no pudo completarse.", ex)
        };
    }
}
