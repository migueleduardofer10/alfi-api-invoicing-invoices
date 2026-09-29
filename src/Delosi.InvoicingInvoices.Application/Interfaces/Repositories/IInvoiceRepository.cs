using Delosi.InvoicingInvoices.Application.Common;
using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Domain.Entities;

namespace Delosi.InvoicingInvoices.Application.Interfaces.Repositories;

public interface IInvoiceRepository
{
    void Add(Invoice invoice);
    // Solo dentro de IUnitOfWork.ExecuteInTransactionAsync; bloquea la cabecera.
    Task<Invoice?> GetForUpdateAsync(long id, CancellationToken cancellationToken);
    void Touch(Invoice invoice);
    Task<InvoiceResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<PagedResponse<InvoiceListItemResponse>> ListAsync(InvoiceFilterRequest filter, CancellationToken cancellationToken);

    Task<PagedResponse<InvoiceListItemWithDetailsResponse>> ListWithDetailsAsync(
    InvoiceFilterRequest filter,
    CancellationToken cancellationToken);
}
