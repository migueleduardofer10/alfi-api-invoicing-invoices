namespace Delosi.InvoicingInvoices.Api.Options;

public static class InvoiceLambdaOptions
{
    public static string Resolve(IConfiguration configuration, bool isLambda)
    {
        var operation = configuration["INVOICE_OPERATION"] ?? "All";
        if (operation is not ("All" or "Create" or "Update" or "Get" or "List") || (isLambda && operation == "All"))
            throw new InvalidOperationException("INVOICE_OPERATION debe identificar una operación en Lambda: Create, Update, Get o List.");
        return operation;
    }
}
