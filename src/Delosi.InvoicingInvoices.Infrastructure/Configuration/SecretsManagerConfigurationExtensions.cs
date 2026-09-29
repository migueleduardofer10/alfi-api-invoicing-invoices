using System.Text.Json;

using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

using Microsoft.Extensions.Configuration;

namespace Delosi.InvoicingInvoices.Infrastructure.Configuration;

/// <summary>
/// Lee secretos de AWS Secrets Manager e inyecta sus claves en IConfiguration.
///
/// Convención de nombres dentro del secreto (JSON plano con __ para el anidamiento):
///   "ConnectionStrings__Postgres" → config["ConnectionStrings:Oracle"]
///   "JwtAuth__ValidAudience"    → config["JwtAuth:ValidAudience"]
///
/// Solo actúa si existen las variables de entorno DB_SECRET_NAME y/o APP_SECRET_NAME
/// (las define la Lambda). En local es un no-op y manda appsettings.Development.json.
/// </summary>
public static class SecretsManagerConfigurationExtensions
{
    private const string DbSecretEnvVar = "DB_SECRET_NAME";
    private const string AppSecretEnvVar = "APP_SECRET_NAME";

    public static IConfigurationBuilder AddSecretsManagerSecrets(
        this IConfigurationBuilder builder,
        string? region = null)
    {
        var dbSecretName = Environment.GetEnvironmentVariable(DbSecretEnvVar);
        var appSecretName = Environment.GetEnvironmentVariable(AppSecretEnvVar);

        // En local (sin env vars) no se hace nada: appsettings.Development.json tiene los valores.
        if (string.IsNullOrEmpty(dbSecretName) && string.IsNullOrEmpty(appSecretName))
        {
            return builder;
        }

        var awsRegion = region
            ?? Environment.GetEnvironmentVariable("AWS_REGION")
            ?? Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION")
            ?? "us-east-1";

        using var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(awsRegion));

        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(dbSecretName))
        {
            MergeSecret(client, dbSecretName, values);
        }

        if (!string.IsNullOrEmpty(appSecretName))
        {
            MergeSecret(client, appSecretName, values);
        }

        if (values.Count > 0)
        {
            builder.AddInMemoryCollection(values);
        }

        return builder;
    }

    private static void MergeSecret(
        IAmazonSecretsManager client,
        string secretName,
        Dictionary<string, string?> target)
    {
        try
        {
            var response = client.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretName })
                .GetAwaiter()
                .GetResult();

            var json = response.SecretString
                ?? throw new InvalidOperationException(
                    $"El secreto '{secretName}' es binario — solo se admiten secretos JSON.");

            // Formatos admitidos:
            //   (A) Plano:   { "ConnectionStrings__Postgres": "..." }
            //   (B) Anidado: { "ConnectionStrings": { "Oracle": "..." } }
            using var document = JsonDocument.Parse(json);
            FlattenJson(document.RootElement, prefix: string.Empty, target);
        }
        catch (ResourceNotFoundException ex)
        {
            throw new InvalidOperationException(
                $"El secreto '{secretName}' no existe en AWS Secrets Manager. " +
                $"Créelo antes de desplegar. Detalle: {ex.Message}",
                ex);
        }
    }

    /// <summary>Aplana un JsonElement a pares clave:valor de IConfiguration.</summary>
    private static void FlattenJson(
        JsonElement element,
        string prefix,
        Dictionary<string, string?> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var segment = property.Name.Replace("__", ":", StringComparison.Ordinal);
                    var key = string.IsNullOrEmpty(prefix) ? segment : $"{prefix}:{segment}";
                    FlattenJson(property.Value, key, result);
                }

                break;

            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    FlattenJson(item, $"{prefix}:{index}", result);
                    index++;
                }

                break;

            case JsonValueKind.Null:
                result[prefix] = null;
                break;

            default: // String, Number, True, False
                result[prefix] = element.ToString();
                break;
        }
    }
}
