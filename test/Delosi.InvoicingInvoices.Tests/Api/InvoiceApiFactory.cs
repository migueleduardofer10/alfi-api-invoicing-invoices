using Delosi.InvoicingInvoices.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Delosi.InvoicingInvoices.Tests.Api;

public sealed class InvoiceApiFactory : WebApplicationFactory<Program>
{
    public IInvoiceService Service { get; } = Substitute.For<IInvoiceService>();
    public string Operation { get; init; } = "All";
    public bool RequireAuthentication { get; init; }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtAuth:Enabled"] = RequireAuthentication.ToString(), ["Swagger:Enabled"] = "false", ["INVOICE_OPERATION"] = Operation
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IInvoiceService>();
            services.AddScoped(_ => Service);
        });
    }
}
