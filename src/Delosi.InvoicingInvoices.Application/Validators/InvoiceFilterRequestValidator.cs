using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Domain.Constants;
using FluentValidation;

namespace Delosi.InvoicingInvoices.Application.Validators;

public sealed class InvoiceFilterRequestValidator : AbstractValidator<InvoiceFilterRequest>
{
    public InvoiceFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Page).Must((filter, page) => ((long)page - 1) * filter.PageSize <= int.MaxValue)
            .WithMessage("La página solicitada excede el rango admitido.");
        RuleFor(x => x.CompanyId).GreaterThan(0).When(x => x.CompanyId.HasValue);
        RuleFor(x => x.CustomerId).GreaterThan(0).When(x => x.CustomerId.HasValue);
        RuleFor(x => x.BrandId).GreaterThan(0).When(x => x.BrandId.HasValue);
        RuleFor(x => x.StoreId).GreaterThan(0).When(x => x.StoreId.HasValue);
        RuleFor(x => x.DocumentTypeId).GreaterThan(0).When(x => x.DocumentTypeId.HasValue);
        RuleFor(x => x.Series).MaximumLength(InvoiceFieldLengths.Series);
        RuleFor(x => x.InvoiceNumber).MaximumLength(InvoiceFieldLengths.InvoiceNumber);
        RuleFor(x => x.Status).Must(x => InvoiceStatus.IsValid(x?.Trim().ToUpperInvariant()))
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
        RuleFor(x => x.IssueDateTo).Must((filter, to) => !to.HasValue || !filter.IssueDateFrom.HasValue || to >= filter.IssueDateFrom)
            .WithMessage("El rango de fechas no es válido.");
        RuleFor(x => x.SortBy).Must(x => x is "issueDate" or "invoiceId" or "total" or "invoiceNumber")
            .WithMessage("SortBy admite issueDate, invoiceId, total o invoiceNumber.");
        RuleFor(x => x.SortDirection).Must(x => x is "asc" or "desc")
            .WithMessage("SortDirection admite asc o desc.");
    }
}
