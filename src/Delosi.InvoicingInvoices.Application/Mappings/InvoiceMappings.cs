using System.Linq.Expressions;
using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Domain.Entities;

namespace Delosi.InvoicingInvoices.Application.Mappings;

/// <summary>Mapeos manuales como el repositorio de referencia; expresiones traducibles a SQL.</summary>
public static class InvoiceMappings
{
	public static Invoice ToEntity(this CreateInvoiceRequest request)
	{
		var entity = new Invoice { CreatedBy = request.CreatedBy };
		request.ApplyTo(entity);
		foreach (var detail in request.Details)
			entity.Details.Add(detail.ToEntity());
		return entity;
	}

	public static void ApplyTo(this InvoiceWriteRequest request, Invoice entity)
	{
		entity.CompanyId = request.CompanyId;
		entity.CustomerId = request.CustomerId;
		entity.DocumentTypeId = request.DocumentTypeId;
		entity.SalesOrganizationId = request.SalesOrganizationId;
		entity.DistributionChannelId = request.DistributionChannelId;
		entity.PaymentTermId = request.PaymentTermId;
		entity.PaymentTypeId = request.PaymentTypeId;
		entity.BrandId = request.BrandId;
		entity.BrandCode = request.BrandCode;
		entity.StoreId = request.StoreId;
		entity.StoreCode = request.StoreCode;
		entity.CustomerDocumentNumber = request.CustomerDocumentNumber;
		entity.CustomerLegalName = request.CustomerLegalName;
		entity.Series = request.Series;
		entity.InvoiceNumber = request.InvoiceNumber;
		entity.IssueDate = request.IssueDate;
		entity.DueDate = request.DueDate;
		entity.Currency = request.Currency;
		entity.ExchangeRate = request.ExchangeRate;
		entity.Subtotal = request.Subtotal;
		entity.Discount = request.Discount;
		entity.ServiceChargeTotal = request.ServiceChargeTotal;
		entity.TaxTotal = request.TaxTotal;
		entity.Total = request.Total;
		entity.Status = request.Status;
	}

	public static InvoiceDetail ToEntity(this InvoiceDetailRequest request)
	{
		var entity = new InvoiceDetail();
		request.ApplyTo(entity);
		return entity;
	}

	public static void ApplyTo(this InvoiceDetailRequest request, InvoiceDetail entity)
	{
		entity.LineNumber = request.LineNumber;
		entity.ConceptId = request.ConceptId;
		entity.ConceptCode = request.ConceptCode;
		entity.MaterialId = request.MaterialId;
		entity.MaterialCode = request.MaterialCode;
		entity.AccountingAccountId = request.AccountingAccountId;
		entity.Description = request.Description;
		entity.Quantity = request.Quantity;
		entity.UnitPrice = request.UnitPrice;
		entity.Discount = request.Discount;
		entity.Subtotal = request.Subtotal;
		entity.ServiceCharge = request.ServiceCharge;
		entity.TaxAmount = request.TaxAmount;
		entity.Total = request.Total;
	}

	public static InvoiceSavedResponse ToSavedResponse(this Invoice entity) => new(
		entity.InvoiceId, entity.CreatedAt, entity.ModifiedAt,
		entity.Details.OrderBy(x => x.LineNumber)
			.Select(x => new InvoiceDetailIdentityResponse(x.InvoiceDetailId, x.LineNumber)).ToList());

	public static readonly Expression<Func<Invoice, InvoiceResponse>> ToResponse = x => new InvoiceResponse
	{
		InvoiceId = x.InvoiceId,
		CompanyId = x.CompanyId,
		CustomerId = x.CustomerId,
		DocumentTypeId = x.DocumentTypeId,
		SalesOrganizationId = x.SalesOrganizationId,
		DistributionChannelId = x.DistributionChannelId,
		PaymentTermId = x.PaymentTermId,
		PaymentTypeId = x.PaymentTypeId,
		BrandId = x.BrandId,
		BrandCode = x.BrandCode,
		StoreId = x.StoreId,
		StoreCode = x.StoreCode,
		CustomerDocumentNumber = x.CustomerDocumentNumber,
		CustomerLegalName = x.CustomerLegalName,
		Series = x.Series,
		InvoiceNumber = x.InvoiceNumber,
		IssueDate = x.IssueDate,
		DueDate = x.DueDate,
		Currency = x.Currency,
		ExchangeRate = x.ExchangeRate,
		Subtotal = x.Subtotal,
		Discount = x.Discount,
		ServiceChargeTotal = x.ServiceChargeTotal,
		TaxTotal = x.TaxTotal,
		Total = x.Total,
		Status = x.Status,
		CreatedBy = x.CreatedBy,
		CreatedAt = x.CreatedAt,
		ModifiedBy = x.ModifiedBy,
		ModifiedAt = x.ModifiedAt,
		Details = x.Details.OrderBy(d => d.LineNumber).Select(d => new InvoiceDetailResponse
		{
			InvoiceDetailId = d.InvoiceDetailId,
			LineNumber = d.LineNumber,
			ConceptId = d.ConceptId,
			ConceptCode = d.ConceptCode,
			MaterialId = d.MaterialId,
			MaterialCode = d.MaterialCode,
			AccountingAccountId = d.AccountingAccountId,
			Description = d.Description,
			Quantity = d.Quantity,
			UnitPrice = d.UnitPrice,
			Discount = d.Discount,
			Subtotal = d.Subtotal,
			ServiceCharge = d.ServiceCharge,
			TaxAmount = d.TaxAmount,
			Total = d.Total
		}).ToList()
	};

	public static readonly Expression<Func<Invoice, InvoiceListItemResponse>> ToListItem = x => new(
		x.InvoiceId, x.CompanyId, x.CustomerId, x.CustomerLegalName,
		x.Series, x.InvoiceNumber, x.IssueDate, x.DueDate,
		x.Currency, x.Total, x.Status, x.BrandCode, x.StoreCode);



	public static readonly Expression<
	Func<Invoice, InvoiceListItemWithDetailsResponse>>
	ToListItemWithDetails = invoice =>
		new InvoiceListItemWithDetailsResponse(
			invoice.InvoiceId,
			invoice.CompanyId,
			invoice.CustomerId,
			invoice.CustomerLegalName,
			invoice.Series,
			invoice.InvoiceNumber,
			invoice.IssueDate,
			invoice.DueDate,
			invoice.Currency,
			invoice.Total,
			invoice.Status,
			invoice.BrandCode,
			invoice.StoreCode)
		{
			Details = invoice.Details
				.OrderBy(detail => detail.LineNumber)
				.Select(detail => new InvoiceDetailResponse
				{
					InvoiceDetailId = detail.InvoiceDetailId,
					LineNumber = detail.LineNumber,
					ConceptId = detail.ConceptId,
					ConceptCode = detail.ConceptCode,
					MaterialId = detail.MaterialId,
					MaterialCode = detail.MaterialCode,
					AccountingAccountId = detail.AccountingAccountId,
					Description = detail.Description,
					Quantity = detail.Quantity,
					UnitPrice = detail.UnitPrice,
					Discount = detail.Discount,
					Subtotal = detail.Subtotal,
					ServiceCharge = detail.ServiceCharge,
					TaxAmount = detail.TaxAmount,
					Total = detail.Total
				})
				.ToList()
		};
}
