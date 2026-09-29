using Delosi.InvoicingInvoices.Application.Common;
using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Application.Interfaces.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Delosi.InvoicingInvoices.Api.Endpoints;

public static class InvoiceEndpoints
{
	public static RouteGroupBuilder MapInvoiceEndpoints(
		this IEndpointRouteBuilder app,
		string operation = "All")
	{
		var group = app
			.MapGroup("/facturas")
			.WithTags("Facturas");

		if (operation is "All" or "Create")
		{
			group.MapPost("/crear", CreateInvoice)
				.WithName("CreateInvoice")
				.Produces<ApiResponse<InvoiceSavedResponse>>(
					StatusCodes.Status201Created)
				.Produces<ErrorResponse>(400)
				.Produces<ErrorResponse>(409);
		}

		if (operation is "All" or "Update")
		{
			group.MapPut("/actualizar/{id}", UpdateInvoice)
				.WithName("UpdateInvoice")
				.Produces<ApiResponse<InvoiceSavedResponse>>()
				.Produces<ErrorResponse>(400)
				.Produces<ErrorResponse>(404)
				.Produces<ErrorResponse>(409);
		}

		if (operation is "All" or "Get")
		{
			group.MapGet("/consultar/{id}", GetInvoice)
				.WithName("GetInvoice")
				.Produces<ApiResponse<InvoiceResponse>>()
				.Produces<ErrorResponse>(400)
				.Produces<ErrorResponse>(404);
		}

		if (operation is "All" or "List")
		{
			group.MapGet("/listar", ListInvoices)
				.WithName("ListInvoices")
				.Produces<ApiResponse<PagedResponse<InvoiceListItemResponse>>>()
				.Produces<ErrorResponse>(400);

			group.MapGet("/listar-con-detalle", ListInvoicesWithDetails)
				.WithName("ListInvoicesWithDetails")
				.Produces<ApiResponse<PagedResponse<InvoiceListItemWithDetailsResponse>>>()
				.Produces<ErrorResponse>(400);
		}

		return group;
	}

	private static async Task<IResult> CreateInvoice([FromBody] CreateInvoiceRequest request,
        IInvoiceService service, IValidator<CreateInvoiceRequest> validator, HttpContext http, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var result = await service.CreateAsync(request, ct);
        return Results.Created($"{http.Request.PathBase}/facturas/consultar/{result.InvoiceId}",
            ApiResponse<InvoiceSavedResponse>.Ok(result, "Factura creada."));
    }

    private static async Task<IResult> UpdateInvoice([FromRoute] long id, [FromBody] UpdateInvoiceRequest request,
        IInvoiceService service, IValidator<UpdateInvoiceRequest> validator, CancellationToken ct)
    {
        ValidateId(id);
        await validator.ValidateAndThrowAsync(request, ct);
        return Results.Ok(ApiResponse<InvoiceSavedResponse>.Ok(await service.UpdateAsync(id, request, ct), "Factura actualizada."));
    }

    private static async Task<IResult> GetInvoice([FromRoute] long id, IInvoiceService service, CancellationToken ct)
    {
        ValidateId(id);
        return Results.Ok(ApiResponse<InvoiceResponse>.Ok(await service.GetByIdAsync(id, ct)));
    }

    private static async Task<IResult> ListInvoices(
        [FromQuery] int? page, [FromQuery] int? pageSize,
        [FromQuery] long? companyId, [FromQuery] long? customerId,
        [FromQuery] long? brandId, [FromQuery] long? storeId,
        [FromQuery] long? documentTypeId, [FromQuery] string? series,
        [FromQuery] string? invoiceNumber, [FromQuery] string? status,
        [FromQuery] DateOnly? issueDateFrom, [FromQuery] DateOnly? issueDateTo,
        [FromQuery] string? sortBy, [FromQuery] string? sortDirection,
        IInvoiceService service, IValidator<InvoiceFilterRequest> validator, CancellationToken ct)
    {
        var filter = new InvoiceFilterRequest(page ?? 1, pageSize ?? 20, companyId, customerId,
            brandId, storeId, documentTypeId, series, invoiceNumber, status, issueDateFrom, issueDateTo,
            sortBy ?? "issueDate", sortDirection ?? "desc");
        await validator.ValidateAndThrowAsync(filter, ct);
        return Results.Ok(ApiResponse<PagedResponse<InvoiceListItemResponse>>.Ok(await service.ListAsync(filter, ct)));
    }

	private static async Task<IResult> ListInvoicesWithDetails(
	[FromQuery] int? page,
	[FromQuery] int? pageSize,
	[FromQuery] long? companyId,
	[FromQuery] long? customerId,
	[FromQuery] long? brandId,
	[FromQuery] long? storeId,
	[FromQuery] long? documentTypeId,
	[FromQuery] string? series,
	[FromQuery] string? invoiceNumber,
	[FromQuery] string? status,
	[FromQuery] DateOnly? issueDateFrom,
	[FromQuery] DateOnly? issueDateTo,
	[FromQuery] string? sortBy,
	[FromQuery] string? sortDirection,
	IInvoiceService service,
	IValidator<InvoiceFilterRequest> validator,
	CancellationToken ct)
	{
		var filter = new InvoiceFilterRequest(
			page ?? 1,
			pageSize ?? 20,
			companyId,
			customerId,
			brandId,
			storeId,
			documentTypeId,
			series,
			invoiceNumber,
			status,
			issueDateFrom,
			issueDateTo,
			sortBy ?? "issueDate",
			sortDirection ?? "desc");

		await validator.ValidateAndThrowAsync(filter, ct);

		var result = await service.ListWithDetailsAsync(filter, ct);

		return Results.Ok(
			ApiResponse<PagedResponse<InvoiceListItemWithDetailsResponse>>
				.Ok(result));
	}

	private static void ValidateId(long id)
    {
        if (id <= 0) throw new ValidationException([new ValidationFailure("id", "El identificador debe ser mayor que cero.")]);
    }
}
