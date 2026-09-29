using System.Reflection;
using Delosi.InvoicingInvoices.Application.Interfaces.Services;
using Delosi.InvoicingInvoices.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Delosi.InvoicingInvoices.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
        services.AddScoped<IInvoiceService, InvoiceService>();
        return services;
    }
}
