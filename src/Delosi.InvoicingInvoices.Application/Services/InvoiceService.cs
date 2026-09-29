using Delosi.InvoicingInvoices.Application.Common;
using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Application.Interfaces.Repositories;
using Delosi.InvoicingInvoices.Application.Interfaces.Services;
using Delosi.InvoicingInvoices.Application.Mappings;
using Delosi.InvoicingInvoices.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Delosi.InvoicingInvoices.Application.Services;

public sealed class InvoiceService(IInvoiceRepository repository, IUnitOfWork unitOfWork,
	ILogger<InvoiceService> logger) : IInvoiceService
{
	public async Task<InvoiceSavedResponse> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken)
	{
		var invoice = request.ToEntity();
		repository.Add(invoice);
		// Un único SaveChanges guarda el grafo completo en una transacción, con batching de EF.
		await unitOfWork.SaveChangesAsync(cancellationToken);
		logger.LogInformation("Factura {InvoiceId} creada con {LineCount} líneas", invoice.InvoiceId, invoice.Details.Count);
		return invoice.ToSavedResponse();
	}

	public async Task<InvoiceSavedResponse> UpdateAsync(long id, UpdateInvoiceRequest request, CancellationToken cancellationToken)
	{
		var result = await unitOfWork.ExecuteInTransactionAsync(async ct =>
		{
			var invoice = await repository.GetForUpdateAsync(id, ct)
				?? throw new NotFoundException("la factura", id);
			var existing = invoice.Details.ToDictionary(x => x.InvoiceDetailId);
			var incomingIds = request.Details.Where(x => x.InvoiceDetailId.HasValue)
				.Select(x => x.InvoiceDetailId!.Value).ToHashSet();

			if (incomingIds.Any(key => !existing.ContainsKey(key)))
				throw new BusinessRuleException("Uno de los detalles no pertenece a esta factura.");
			// Sin operaciones sobre invoice_tax no se pueden borrar líneas con seguridad.
			// Esta versión permite editar y agregar; requiere conservar todas las líneas existentes.
			if (existing.Keys.Any(key => !incomingIds.Contains(key)))
				throw new BusinessRuleException("Incluya todas las líneas existentes con su invoiceDetailId. Esta versión no elimina líneas.");

			// El UNIQUE (invoice_id, line_number) no es DEFERRABLE. Para intercambiar números
			// movemos primero las líneas afectadas a números positivos libres, en la misma transacción.
			var occupied = invoice.Details.Select(x => x.LineNumber)
				.Concat(request.Details.Select(x => x.LineNumber)).ToHashSet();
			var candidate = int.MaxValue;
			var renumbered = false;
			foreach (var detail in request.Details.Where(x => x.InvoiceDetailId.HasValue))
			{
				var entity = existing[detail.InvoiceDetailId!.Value];
				if (entity.LineNumber == detail.LineNumber) continue;
				while (occupied.Contains(candidate)) candidate--;
				entity.LineNumber = candidate;
				occupied.Add(candidate--);
				renumbered = true;
			}
			if (renumbered) await unitOfWork.SaveChangesAsync(ct);

			request.ApplyTo(invoice);
			invoice.ModifiedBy = request.ModifiedBy;
			foreach (var detail in request.Details)
			{
				if (detail.InvoiceDetailId is { } detailId)
					detail.ApplyTo(existing[detailId]);
				else
					invoice.Details.Add(detail.ToEntity());
			}
			// Fuerza el UPDATE de cabecera incluso si solo cambian detalles: el trigger sella modified_at.
			repository.Touch(invoice);
			await unitOfWork.SaveChangesAsync(ct);
			return invoice.ToSavedResponse();
		}, cancellationToken);
		logger.LogInformation("Factura {InvoiceId} actualizada", id);
		return result;
	}

	public async Task<InvoiceResponse> GetByIdAsync(long id, CancellationToken cancellationToken)
		=> await repository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("la factura", id);

	public Task<PagedResponse<InvoiceListItemResponse>> ListAsync(InvoiceFilterRequest filter, CancellationToken cancellationToken)
		=> repository.ListAsync(filter with
		{
			Series = Blank(filter.Series),
			InvoiceNumber = Blank(filter.InvoiceNumber),
			Status = Blank(filter.Status)?.ToUpperInvariant()
		}, cancellationToken);

	private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

	public Task<PagedResponse<InvoiceListItemWithDetailsResponse>> ListWithDetailsAsync(
	InvoiceFilterRequest filter,
	CancellationToken cancellationToken)
	{
		var normalizedFilter = filter with
		{
			Series = string.IsNullOrWhiteSpace(filter.Series)
				? null
				: filter.Series.Trim(),

			InvoiceNumber = string.IsNullOrWhiteSpace(filter.InvoiceNumber)
				? null
				: filter.InvoiceNumber.Trim(),

			Status = string.IsNullOrWhiteSpace(filter.Status)
				? null
				: filter.Status.Trim().ToUpperInvariant()
		};

		return repository.ListWithDetailsAsync(normalizedFilter, cancellationToken);
	}
}
