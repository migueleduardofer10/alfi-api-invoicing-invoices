namespace Delosi.InvoicingInvoices.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    public int CommandTimeoutSeconds { get; set; } = 20;
    public int MaxBatchSize { get; set; } = 100;
}
