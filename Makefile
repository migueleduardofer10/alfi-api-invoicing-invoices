# SAM invoca un target por cada recurso Lambda. CodeUri=. conserva referencias entre capas.
.PHONY: build-CreateInvoiceFunction build-UpdateInvoiceFunction build-GetInvoiceFunction build-ListInvoicesFunction
build-CreateInvoiceFunction build-UpdateInvoiceFunction build-GetInvoiceFunction build-ListInvoicesFunction:
	dotnet publish src/Delosi.InvoicingInvoices.Api/Delosi.InvoicingInvoices.Api.csproj --configuration Release --runtime linux-x64 --self-contained false -p:UseAppHost=false --output "$(ARTIFACTS_DIR)"
