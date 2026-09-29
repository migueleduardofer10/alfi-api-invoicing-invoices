using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Domain.Constants;
using FluentValidation;

namespace Delosi.InvoicingInvoices.Application.Validators;

public sealed class UpdateInvoiceRequestValidator : InvoiceWriteRequestValidator<UpdateInvoiceRequest>
{
    public UpdateInvoiceRequestValidator()
    {
        RuleFor(x => x.ModifiedBy).MaximumLength(InvoiceFieldLengths.User);
    }
}
