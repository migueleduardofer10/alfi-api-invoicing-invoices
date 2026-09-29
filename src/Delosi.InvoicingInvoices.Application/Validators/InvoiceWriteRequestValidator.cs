using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Domain.Constants;
using FluentValidation;

namespace Delosi.InvoicingInvoices.Application.Validators;

public abstract class InvoiceWriteRequestValidator<T> : AbstractValidator<T> where T : InvoiceWriteRequest
{
    public InvoiceWriteRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.DocumentTypeId).GreaterThan(0);
        RuleFor(x => x.SalesOrganizationId).GreaterThan(0);
        RuleFor(x => x.DistributionChannelId).GreaterThan(0);
        RuleFor(x => x.PaymentTermId).GreaterThan(0);
        RuleFor(x => x.PaymentTypeId).GreaterThan(0);
        RuleFor(x => x.BrandId).GreaterThan(0);
        RuleFor(x => x.StoreId).GreaterThan(0).When(x => x.StoreId.HasValue);
        RuleFor(x => x.BrandCode).NotEmpty().MaximumLength(InvoiceFieldLengths.BrandCode);
        RuleFor(x => x.StoreCode).MaximumLength(InvoiceFieldLengths.StoreCode);
        RuleFor(x => x.CustomerDocumentNumber).NotEmpty().MaximumLength(InvoiceFieldLengths.CustomerDocumentNumber);
        RuleFor(x => x.CustomerLegalName).NotEmpty().MaximumLength(InvoiceFieldLengths.CustomerLegalName);
        RuleFor(x => x.Series).NotEmpty().MaximumLength(InvoiceFieldLengths.Series);
        RuleFor(x => x.InvoiceNumber).NotEmpty().MaximumLength(InvoiceFieldLengths.InvoiceNumber);
        RuleFor(x => x.IssueDate).NotEmpty();
        RuleFor(x => x.DueDate).Must((request, due) => !due.HasValue || due.Value >= request.IssueDate)
            .WithMessage("La fecha de vencimiento no puede ser anterior a la fecha de emisión.");
        RuleFor(x => x.Currency).NotEmpty().Matches("^[A-Z]{3}$");
        RuleFor(x => x.ExchangeRate).GreaterThan(0).PrecisionScale(12, 6, true);
        RuleFor(x => x.Subtotal).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.ServiceChargeTotal).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.TaxTotal).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.Total).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.Total).Must((request, _) => MatchesTotal(request))
            .WithMessage("Total debe coincidir con subtotal - descuento + recargo + impuesto, con tolerancia de 0.01.");
        RuleFor(x => x.Status).Must(InvoiceStatus.IsValid).WithMessage("El estado no está permitido por ck_invoice_status.");
        RuleFor(x => x.Details).NotNull().Must(x => x is { Count: > 0 and <= 1000 })
            .WithMessage("Debe enviar entre 1 y 1000 líneas.");
        RuleForEach(x => x.Details).NotNull().SetValidator(new InvoiceDetailRequestValidator());
        RuleFor(x => x.Details).Must(lines => lines is null ||
            lines.Where(x => x is not null).Select(x => x.LineNumber).Distinct().Count() == lines.Count)
            .WithMessage("Los números de línea deben ser únicos.");
        RuleFor(x => x.Details).Must(lines => lines is null ||
            lines.Where(x => x?.InvoiceDetailId is not null).Select(x => x.InvoiceDetailId).Distinct().Count() ==
            lines.Count(x => x?.InvoiceDetailId is not null))
            .WithMessage("No puede repetir el identificador de una línea.");
    }

    private static bool MatchesTotal(InvoiceWriteRequest request)
    {
        try
        {
            return Math.Abs(request.Total - (request.Subtotal - request.Discount
                + request.ServiceChargeTotal + request.TaxTotal)) <= 0.01m;
        }
        catch (OverflowException) { return false; }
    }
}
