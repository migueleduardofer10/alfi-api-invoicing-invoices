using Delosi.InvoicingInvoices.Application.Common;
using Delosi.InvoicingInvoices.Application.DTOs.Invoice;

namespace Delosi.InvoicingInvoices.Application.Interfaces.Services;

public interface IInvoiceService
{
	Task<InvoiceSavedResponse> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken);
	Task<InvoiceSavedResponse> UpdateAsync(long id, UpdateInvoiceRequest request, CancellationToken cancellationToken);
	Task<InvoiceResponse> GetByIdAsync(long id, CancellationToken cancellationToken);
	Task<PagedResponse<InvoiceListItemResponse>> ListAsync(InvoiceFilterRequest filter, CancellationToken cancellationToken);

	Task<PagedResponse<InvoiceListItemWithDetailsResponse>> ListWithDetailsAsync(
	InvoiceFilterRequest filter,
	CancellationToken cancellationToken);
}
