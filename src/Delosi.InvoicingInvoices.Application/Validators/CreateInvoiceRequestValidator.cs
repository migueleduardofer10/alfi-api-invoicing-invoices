using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Domain.Constants;
using FluentValidation;

namespace Delosi.InvoicingInvoices.Application.Validators;

public sealed class CreateInvoiceRequestValidator : InvoiceWriteRequestValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.CreatedBy).MaximumLength(InvoiceFieldLengths.User);
        RuleForEach(x => x.Details).Must(x => x is null || !x.InvoiceDetailId.HasValue)
            .WithMessage("En Crear los identificadores de detalle los genera PostgreSQL.");
    }
}
