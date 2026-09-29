using System.Text.Json.Serialization;
using Asp.Versioning;
using Delosi.InvoicingInvoices.Api.Endpoints;
using Delosi.InvoicingInvoices.Api.Extensions;
using Delosi.InvoicingInvoices.Api.Middleware;
using Delosi.InvoicingInvoices.Api.Options;
using Delosi.InvoicingInvoices.Application;
using Delosi.InvoicingInvoices.Infrastructure.Configuration;
using Delosi.InvoicingInvoices.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSecretsManagerSecrets();
var isLambda = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME"));

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Delosi.InvoicingInvoices.Api")
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
});
if (isLambda) builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(cors => cors.AddDefaultPolicy(policy =>
{
    if (allowedOrigins.Length == 0) policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    else policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));
builder.Services.AddApiVersioning(versioning =>
{
    versioning.DefaultApiVersion = new ApiVersion(1);
    versioning.AssumeDefaultVersionWhenUnspecified = true;
    versioning.ReportApiVersions = true;
});
var swaggerEnabled = builder.Configuration.GetValue("Swagger:Enabled", !isLambda);
if (swaggerEnabled) builder.Services.AddSwaggerDocumentation();

var app = builder.Build();
app.UseCorrelationId();
app.UseSerilogRequestLogging();
app.UseGlobalExceptionHandler();
app.UseStatusCodePages(async statusContext => await ExceptionMiddleware.WriteStatusAsync(statusContext.HttpContext));
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
if (swaggerEnabled) app.UseSwaggerDocumentation();

// Se conserva versionado v1, pero las rutas son exactamente las solicitadas: /facturas/...
var operation = InvoiceLambdaOptions.Resolve(app.Configuration, isLambda);
var v1 = app.NewVersionedApi().MapGroup("").HasApiVersion(1);
var invoices = v1.MapInvoiceEndpoints(operation);
if (builder.Configuration.GetValue("JwtAuth:Enabled", true)) invoices.RequireAuthorization();
app.MapHealthEndpoints();
app.Run();

public partial class Program { }
