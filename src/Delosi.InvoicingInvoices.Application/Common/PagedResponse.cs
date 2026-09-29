namespace Delosi.InvoicingInvoices.Application.Common;

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount)
{
    public long TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
