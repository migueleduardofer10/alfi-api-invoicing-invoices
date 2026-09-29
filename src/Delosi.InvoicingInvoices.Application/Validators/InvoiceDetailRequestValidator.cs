using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Domain.Constants;
using FluentValidation;

namespace Delosi.InvoicingInvoices.Application.Validators;

public sealed class InvoiceDetailRequestValidator : AbstractValidator<InvoiceDetailRequest>
{
    public InvoiceDetailRequestValidator()
    {
        RuleFor(x => x.InvoiceDetailId).GreaterThan(0).When(x => x.InvoiceDetailId.HasValue);
        RuleFor(x => x.LineNumber).GreaterThan(0);
        RuleFor(x => x.ConceptId).GreaterThan(0);
        RuleFor(x => x.MaterialId).GreaterThan(0);
        RuleFor(x => x.AccountingAccountId).GreaterThan(0).When(x => x.AccountingAccountId.HasValue);
        RuleFor(x => x.ConceptCode).NotEmpty().MaximumLength(InvoiceFieldLengths.ConceptCode);
        RuleFor(x => x.MaterialCode).NotEmpty().MaximumLength(InvoiceFieldLengths.MaterialCode);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(InvoiceFieldLengths.Description);
        RuleFor(x => x.Quantity).GreaterThan(0).PrecisionScale(14, 4, true);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).PrecisionScale(18, 6, true);
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.Subtotal).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.ServiceCharge).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.TaxAmount).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.Total).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
    }
}
