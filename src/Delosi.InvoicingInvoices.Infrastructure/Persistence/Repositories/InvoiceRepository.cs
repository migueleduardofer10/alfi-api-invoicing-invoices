using Delosi.InvoicingInvoices.Application.Common;
using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Application.Interfaces.Repositories;
using Delosi.InvoicingInvoices.Application.Mappings;
using Delosi.InvoicingInvoices.Domain.Entities;
using Delosi.InvoicingInvoices.Infrastructure.Persistence.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Delosi.InvoicingInvoices.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository(AppDbContext context, ILogger<InvoiceRepository> logger) : IInvoiceRepository
{
    public void Add(Invoice invoice) => context.Invoices.Add(invoice);

    public Task<Invoice?> GetForUpdateAsync(long id, CancellationToken cancellationToken)
        => DatabaseOperation.RunAsync(async () =>
        {
            if (context.Database.CurrentTransaction is null)
                throw new InvalidOperationException("La actualización requiere una transacción activa.");
            // SQL fijo, ID parametrizado por EF. El bloqueo serializa las actualizaciones de esta API.
            var invoice = await context.Invoices
                .FromSqlInterpolated($"SELECT * FROM invoice WHERE invoice_id = {id} FOR UPDATE")
                .AsTracking().SingleOrDefaultAsync(cancellationToken);
            if (invoice is not null)
                await context.Entry(invoice).Collection(x => x.Details).LoadAsync(cancellationToken);
            return invoice;
        }, logger);

    public void Touch(Invoice invoice) => context.Entry(invoice).Property(x => x.ModifiedBy).IsModified = true;

    public Task<InvoiceResponse?> GetByIdAsync(long id, CancellationToken cancellationToken)
        => DatabaseOperation.RunAsync(() => context.Invoices.AsNoTracking()
            .Where(x => x.InvoiceId == id).Select(InvoiceMappings.ToResponse)
            .SingleOrDefaultAsync(cancellationToken), logger);

    public Task<PagedResponse<InvoiceListItemResponse>> ListAsync(InvoiceFilterRequest filter, CancellationToken cancellationToken)
        => DatabaseOperation.RunAsync(async () =>
        {
            IQueryable<Invoice> query = context.Invoices.AsNoTracking();
            if (filter.CompanyId.HasValue) query = query.Where(x => x.CompanyId == filter.CompanyId.Value);
            if (filter.CustomerId.HasValue) query = query.Where(x => x.CustomerId == filter.CustomerId.Value);
            if (filter.BrandId.HasValue) query = query.Where(x => x.BrandId == filter.BrandId.Value);
            if (filter.StoreId.HasValue) query = query.Where(x => x.StoreId == filter.StoreId.Value);
            if (filter.DocumentTypeId.HasValue) query = query.Where(x => x.DocumentTypeId == filter.DocumentTypeId.Value);
            if (filter.Series is not null) query = query.Where(x => x.Series == filter.Series);
            if (filter.InvoiceNumber is not null) query = query.Where(x => x.InvoiceNumber == filter.InvoiceNumber);
            if (filter.Status is not null) query = query.Where(x => x.Status == filter.Status);
            if (filter.IssueDateFrom.HasValue) query = query.Where(x => x.IssueDate >= filter.IssueDateFrom.Value);
            if (filter.IssueDateTo.HasValue) query = query.Where(x => x.IssueDate <= filter.IssueDateTo.Value);

            // Dos consultas acotadas; no se materializa la tabla para filtrar o contar en memoria.
            var totalCount = await query.LongCountAsync(cancellationToken);
            var descending = filter.SortDirection == "desc";
            IOrderedQueryable<Invoice> ordered = (filter.SortBy, descending) switch
            {
                ("invoiceId", true) => query.OrderByDescending(x => x.InvoiceId),
                ("invoiceId", false) => query.OrderBy(x => x.InvoiceId),
                ("total", true) => query.OrderByDescending(x => x.Total),
                ("total", false) => query.OrderBy(x => x.Total),
                ("invoiceNumber", true) => query.OrderByDescending(x => x.InvoiceNumber),
                ("invoiceNumber", false) => query.OrderBy(x => x.InvoiceNumber),
                ("issueDate", true) => query.OrderByDescending(x => x.IssueDate),
                ("issueDate", false) => query.OrderBy(x => x.IssueDate),
                _ => throw new ArgumentOutOfRangeException(nameof(filter), "Ordenamiento no validado.")
            };
            ordered = descending ? ordered.ThenByDescending(x => x.InvoiceId) : ordered.ThenBy(x => x.InvoiceId);
            var items = await ordered.Skip(checked((filter.Page - 1) * filter.PageSize)).Take(filter.PageSize)
                .Select(InvoiceMappings.ToListItem).ToListAsync(cancellationToken);
            logger.LogInformation("Listado de facturas: página {Page}, {Count} resultados", filter.Page, items.Count);
            return new PagedResponse<InvoiceListItemResponse>(items, filter.Page, filter.PageSize, totalCount);
        }, logger);

	public Task<PagedResponse<InvoiceListItemWithDetailsResponse>> ListWithDetailsAsync(
	InvoiceFilterRequest filter,
	CancellationToken cancellationToken)
	{
		return DatabaseOperation.RunAsync(async () =>
		{
			IQueryable<Invoice> query = context.Invoices
				.AsNoTracking();

			if (filter.CompanyId.HasValue)
			{
				query = query.Where(
					x => x.CompanyId == filter.CompanyId.Value);
			}

			if (filter.CustomerId.HasValue)
			{
				query = query.Where(
					x => x.CustomerId == filter.CustomerId.Value);
			}

			if (filter.BrandId.HasValue)
			{
				query = query.Where(
					x => x.BrandId == filter.BrandId.Value);
			}

			if (filter.StoreId.HasValue)
			{
				query = query.Where(
					x => x.StoreId == filter.StoreId.Value);
			}

			if (filter.DocumentTypeId.HasValue)
			{
				query = query.Where(
					x => x.DocumentTypeId == filter.DocumentTypeId.Value);
			}

			if (filter.Series is not null)
			{
				query = query.Where(
					x => x.Series == filter.Series);
			}

			if (filter.InvoiceNumber is not null)
			{
				query = query.Where(
					x => x.InvoiceNumber == filter.InvoiceNumber);
			}

			if (filter.Status is not null)
			{
				query = query.Where(
					x => x.Status == filter.Status);
			}

			if (filter.IssueDateFrom.HasValue)
			{
				query = query.Where(
					x => x.IssueDate >= filter.IssueDateFrom.Value);
			}

			if (filter.IssueDateTo.HasValue)
			{
				query = query.Where(
					x => x.IssueDate <= filter.IssueDateTo.Value);
			}

			var totalCount = await query.LongCountAsync(
				cancellationToken);

			var descending = filter.SortDirection == "desc";

			IOrderedQueryable<Invoice> ordered =
				(filter.SortBy, descending) switch
				{
					("invoiceId", true) =>
						query.OrderByDescending(x => x.InvoiceId),

					("invoiceId", false) =>
						query.OrderBy(x => x.InvoiceId),

					("total", true) =>
						query.OrderByDescending(x => x.Total),

					("total", false) =>
						query.OrderBy(x => x.Total),

					("invoiceNumber", true) =>
						query.OrderByDescending(x => x.InvoiceNumber),

					("invoiceNumber", false) =>
						query.OrderBy(x => x.InvoiceNumber),

					("issueDate", true) =>
						query.OrderByDescending(x => x.IssueDate),

					("issueDate", false) =>
						query.OrderBy(x => x.IssueDate),

					_ => throw new ArgumentOutOfRangeException(
						nameof(filter),
						"Ordenamiento no validado.")
				};


			ordered = descending
				? ordered.ThenByDescending(x => x.InvoiceId)
				: ordered.ThenBy(x => x.InvoiceId);

			var skip = checked(
				(filter.Page - 1) * filter.PageSize);


			var items = await ordered
				.Skip(skip)
				.Take(filter.PageSize)
				.AsSingleQuery()
				.Select(InvoiceMappings.ToListItemWithDetails)
				.ToListAsync(cancellationToken);

			logger.LogInformation(
				"Listado de facturas con detalles: página {Page}, " +
				"{Count} facturas, total {TotalCount}",
				filter.Page,
				items.Count,
				totalCount);

			return new PagedResponse<InvoiceListItemWithDetailsResponse>(
				items,
				filter.Page,
				filter.PageSize,
				totalCount);
		}, logger);
	}
}
