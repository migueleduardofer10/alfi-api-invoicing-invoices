using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Delosi.InvoicingInvoices.Application.DTOs.Invoice;
using Delosi.InvoicingInvoices.Tests.Common;
using NSubstitute;

namespace Delosi.InvoicingInvoices.Tests.Api;

public sealed class InvoiceEndpointsTests
{
    [Fact] public async Task Crear_devuelve_201_y_envoltura_del_repositorio()
    {
        await using var factory = new InvoiceApiFactory();
        factory.Service.CreateAsync(Arg.Any<CreateInvoiceRequest>(), Arg.Any<CancellationToken>())
            .Returns(new InvoiceSavedResponse(42, new DateTime(2026, 9, 15), null, [new(17, 1)]));
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/facturas/crear", InvoiceFactory.Create());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/facturas/consultar/42", response.Headers.Location!.ToString());
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(42, json.RootElement.GetProperty("data").GetProperty("invoiceId").GetInt64());
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"taxes\":[]}")]
    public async Task JSON_invalido_usa_error_estandar(string body)
    {
        await using var factory = new InvoiceApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-Id", "test-correlation");
        using var response = await client.PostAsync("/facturas/crear", new StringContent(body, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("VALIDATION_ERROR", json.RootElement.GetProperty("code").GetString());
        Assert.Equal("test-correlation", json.RootElement.GetProperty("traceId").GetString());
        Assert.Equal("test-correlation", response.Headers.GetValues("X-Correlation-Id").Single());
    }

    [Theory]
    [InlineData("/facturas/consultar/abc")]
    [InlineData("/facturas/consultar/0")]
    [InlineData("/facturas/listar?pageSize=101")]
    [InlineData("/facturas/listar?pageSize=abc")]
    [InlineData("/facturas/listar?issueDateFrom=invalid")]
    public async Task Parametros_invalidos_devuelven_400(string path)
    {
        await using var factory = new InvoiceApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("VALIDATION_ERROR", json.RootElement.GetProperty("code").GetString());
    }

    [Fact] public async Task Lambda_de_crear_no_expone_consultar()
    {
        await using var factory = new InvoiceApiFactory { Operation = "Create" };
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/facturas/consultar/1")).StatusCode);
    }

    [Fact] public async Task Autenticacion_exigida_devuelve_401_estandar()
    {
        await using var factory = new InvoiceApiFactory { RequireAuthentication = true };
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/facturas/consultar/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEmpty(response.Headers.WwwAuthenticate);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("UNAUTHORIZED", json.RootElement.GetProperty("code").GetString());
    }
}
