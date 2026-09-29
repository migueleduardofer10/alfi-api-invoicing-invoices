# Despliegue en AWS

El entregable prepara el despliegue; no crea recursos en su cuenta automáticamente. `serverless.template` define cuatro `AWS::Serverless::Function` .NET 8 y un API Gateway REST. Cada recurso usa el mismo ensamblado y una operación distinta, siguiendo el [hosting Minimal API de AWS](https://docs.aws.amazon.com/lambda/latest/dg/csharp-package-asp.html).

## Configuración necesaria

1. Una base PostgreSQL/Aurora existente con las tablas proporcionadas. El template no crea RDS, tablas, índices ni migraciones.
2. Subredes y security groups existentes con conectividad a PostgreSQL, y acceso a Secrets Manager mediante NAT o endpoint VPC.
3. Dos secretos existentes con los formatos de `docs/secrets/database.example.json` y `application.example.json`. Se aceptan claves planas con `__` o JSON anidado, como en la referencia. En el secreto de aplicación configure emisor, audiencia y metadata OIDC reales; JWT está habilitado en producción.
4. AWS CLI/SAM CLI y credenciales con capacidad para desplegar CloudFormation, Lambda, API Gateway, IAM y CloudWatch. Para compilar localmente, SDK .NET 8 y Make; en Windows puede usar el build de SAM en contenedor.

Los secretos se leen durante el arranque de cada entorno Lambda usando su role; no se incorporan las credenciales a la plantilla ni al ZIP. Al rotar secretos, los entornos calientes retienen su configuración hasta reiniciarse. Si usa una clave KMS administrada por usted, agregue el permiso `kms:Decrypt` limitado a esa clave al role correspondiente.

Para Aurora/RDS se incluye `src/Delosi.InvoicingInvoices.Api/certificates/global-bundle.pem`, descargado del [truststore oficial de AWS RDS](https://truststore.pki.rds.amazonaws.com/global/global-bundle.pem). El proyecto lo copia a publicación. El ejemplo configura `SSL Mode=VerifyFull` y `/var/task/certificates/global-bundle.pem`. Si usa otro proveedor, configure su CA y conexión correspondientes.

## Construir y desplegar

Desde la raíz:

```bash
sam validate --lint --template-file serverless.template
sam build --template-file serverless.template
sam deploy --guided --stack-name api-invoicing-invoices-dev --capabilities CAPABILITY_IAM
```

Alternativa de compilación con Docker:

```bash
sam build --use-container --template-file serverless.template
```

El `Makefile` contiene un target por recurso Lambda. `CodeUri` es la raíz para que la compilación conserve `Directory.Build.props` y las referencias entre proyectos. El [builder Makefile de SAM](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/building-custom-runtimes.html) publica los artefactos en el directorio indicado por SAM. El runtime sigue siendo `dotnet8`.

En el asistente de despliegue ingrese:

| Parámetro | Contenido |
|---|---|
| `StageName` | `dev`, `cert` o `prod` |
| `DatabaseSecretArn` | ARN completo del secreto PostgreSQL |
| `ApplicationSecretArn` | ARN completo del secreto de aplicación/JWT |
| `SubnetIds` | IDs de subredes existentes |
| `SecurityGroupIds` | IDs de security groups existentes |

Las funciones solo reciben permiso de lectura de los dos secretos y los permisos de red/logs asociados a Lambda en VPC. No se declara un authorizer nuevo en API Gateway; la validación JWT se ejecuta en ASP.NET, igual que en el repositorio de referencia.

Al terminar, use el output `BaseUrl` en Postman. Incluye el stage de API Gateway. Mantenga `INVOICE_OPERATION` con `Create`, `Update`, `Get` y `List` respectivamente; si una Lambda no tiene una operación válida, el arranque falla para evitar exponer rutas por una configuración accidental.

La configuración inicial usa 512 MB, timeout Lambda de 28 segundos, timeout SQL de 20 segundos y pool máximo de 10 conexiones por entorno. Son parámetros iniciales, no resultados de una prueba de carga. Ajuste concurrencia y capacidad de conexiones según su PostgreSQL/Aurora; se puede configurar el endpoint de un RDS Proxy existente sin cambiar el esquema.
