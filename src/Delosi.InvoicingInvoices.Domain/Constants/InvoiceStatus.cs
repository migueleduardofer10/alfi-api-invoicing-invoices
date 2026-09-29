namespace Delosi.InvoicingInvoices.Domain.Constants;

public static class InvoiceStatus
{
    public const string Registered = "REGISTERED";
    public const string Issued = "ISSUED";
    public const string Sent = "SENT";
    public const string Accepted = "ACCEPTED";
    public const string Rejected = "REJECTED";
    public const string Cancelled = "CANCELLED";
    public static readonly IReadOnlyCollection<string> All = [Registered, Issued, Sent, Accepted, Rejected, Cancelled];
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
