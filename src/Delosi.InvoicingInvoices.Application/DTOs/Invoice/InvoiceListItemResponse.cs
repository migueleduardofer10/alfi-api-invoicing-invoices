namespace Delosi.InvoicingInvoices.Application.DTOs.Invoice;

public sealed record InvoiceListItemResponse(
    long InvoiceId, long CompanyId, long CustomerId, string CustomerLegalName,
    string Series, string InvoiceNumber, DateOnly IssueDate, DateOnly? DueDate,
    string Currency, decimal Total, string Status, string BrandCode, string? StoreCode);




public sealed record InvoiceListItemWithDetailsResponse(
	long InvoiceId,
	long CompanyId,
	long CustomerId,
	string CustomerLegalName,
	string Series,
	string InvoiceNumber,
	DateOnly IssueDate,
	DateOnly? DueDate,
	string Currency,
	decimal Total,
	string Status,
	string BrandCode,
	string? StoreCode)
{
	public IReadOnlyList<InvoiceDetailResponse> Details { get; init; } = [];
}