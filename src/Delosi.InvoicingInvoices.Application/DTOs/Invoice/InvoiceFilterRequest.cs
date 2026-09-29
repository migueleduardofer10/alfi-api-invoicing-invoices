namespace Delosi.InvoicingInvoices.Application.DTOs.Invoice;

public sealed record InvoiceFilterRequest(
    int Page = 1, int PageSize = 20,
    long? CompanyId = null, long? CustomerId = null,
    long? BrandId = null, long? StoreId = null,
    long? DocumentTypeId = null, string? Series = null, string? InvoiceNumber = null,
    string? Status = null, DateOnly? IssueDateFrom = null, DateOnly? IssueDateTo = null,
    string SortBy = "issueDate", string SortDirection = "desc");
