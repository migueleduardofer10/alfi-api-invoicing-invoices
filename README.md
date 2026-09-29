# api-invoicing-invoices

Microservicio .NET 8 / C# 12 para crear, actualizar, consultar y listar facturas sobre las tablas PostgreSQL existentes. Incluye solución de Visual Studio, las cuatro capas, cuatro recursos AWS Lambda independientes, API Gateway REST, configuración, tests y Postman.

Referencia analizada: [DelosiVoucher, commit a083400addb95eed8d5d919ccb82df7af0512323](https://github.com/gianfrancogc/DelosiVoucher/tree/a083400addb95eed8d5d919ccb82df7af0512323). Consulte primero [el análisis y las diferencias necesarias](docs/ANALISIS.md).

## Abrir y ejecutar

Requisitos locales: SDK .NET 8 y acceso a una base PostgreSQL que ya tenga las tablas del SQL recibido.

1. Abra `Delosi.InvoicingInvoices.sln` o sitúese en esta carpeta.
2. Configure `ConnectionStrings:Postgres` en `src/Delosi.InvoicingInvoices.Api/appsettings.json`, o la variable `ConnectionStrings__Postgres`. El archivo contiene una cadena completa con contraseña de ejemplo `REPLACE_ME`.
3. Ejecute:

```bash
dotnet restore --locked-mode
dotnet build --configuration Release --no-restore
dotnet run --project src/Delosi.InvoicingInvoices.Api --launch-profile Invoices
```

La API local escucha en `http://localhost:5080`; Swagger está en `/swagger`. El perfil local usa `Development`, donde JWT está desactivado como en el repositorio de referencia. `appsettings.json` habilita JWT para el resto de entornos. No se ejecuta ningún script SQL al arrancar, ni `Migrate`, `EnsureCreated` o creación automática de índices.

## Endpoints

| Método | Ruta | Resultado correcto |
|---|---|---|
| POST | `/facturas/crear` | 201, ID de factura e IDs de detalles generados |
| PUT | `/facturas/actualizar/{id}` | 200, datos identificadores y auditoría actualizada |
| GET | `/facturas/consultar/{id}` | 200, cabecera y detalles proyectados a DTO |
| GET | `/facturas/listar` | 200, página de resúmenes y conteo |

Las rutas son exactamente las solicitadas, sin prefijo `/api/v1`. Se conserva el registro de versionado v1 de la referencia. `/health` y `/health/ready` están disponibles en el host local; el template solo publica las cuatro rutas de negocio.

Los cuerpos de escritura usan nombres `camelCase`, fechas `yyyy-MM-dd`, importes JSON numéricos y estados en mayúsculas. `invoiceTax`, `taxes` e integración SAP no forman parte de los requests. Los campos desconocidos se rechazan con 400 para evitar que el consumidor crea que fueron guardados.

**Crear:** envíe cabecera y entre 1 y 1000 detalles. PostgreSQL genera los IDs. No envíe `invoiceDetailId`, `invoiceId`, `createdAt` ni `modifiedAt`.

**Actualizar:** envíe la cabecera completa y todas las líneas existentes con sus respectivos `invoiceDetailId`. Puede editar líneas y agregar otras con ID omitido o `null`. Esta versión **no elimina líneas**: protege posibles referencias tributarias sin leer ni modificar `invoice_tax`. Los IDs existentes se conservan; un ID de otra factura produce 409. Se permiten cambios en los números de línea, incluidos intercambios, dentro de una única transacción. `createdBy` es exclusivo de Crear; Actualizar recibe `modifiedBy`.

**Importes:** se reciben del consumidor y se validan con la precisión del SQL. No se redondean ni recalculan impuestos. Se aplica la fórmula de cabecera `total = subtotal - discount + serviceChargeTotal + taxTotal`, con tolerancia de 0.01. El SQL no define que las sumas de detalles deban coincidir con la cabecera ni la fórmula de cada línea; no se inventa esa regla.

**Listado:** `page=1`, `pageSize=20` por defecto; tamaño máximo 100. Filtros opcionales: `companyId`, `customerId`, `brandId`, `storeId`, `documentTypeId`, `series`, `invoiceNumber`, `status`, `issueDateFrom`, `issueDateTo`. `sortBy`: `issueDate`, `invoiceId`, `total` o `invoiceNumber`; `sortDirection`: `asc` o `desc`. Siempre se desempata por `invoiceId`. Fechas inclusivas. Los filtros se combinan con AND. Valores vacíos de filtros textuales se ignoran.

```text
/facturas/listar?page=1&pageSize=20&companyId=1&status=REGISTERED&sortBy=issueDate&sortDirection=desc
```

Consultar y Listar usan parámetros de ruta/query; **los GET no llevan body JSON**.

## Postman

Importe `postman/api-invoicing-invoices.postman_collection.json`. Configure `baseUrl` y, para entornos con JWT, `token`. Ejecute las cuatro solicitudes en orden, desde Collection Runner o una por una.

Crear genera un correlativo de prueba de ocho caracteres y guarda `invoiceId`, `invoiceDetailId` e `invoiceNumber`; las otras solicitudes los reutilizan. Los ejemplos individuales están en `postman/examples`. En el JSON estático de Actualizar sustituya el ID de detalle de ejemplo por el real.

El ciclo cambia el estado de `REGISTERED` a `ISSUED` y verifica que la consulta y el listado reflejen la actualización. No escribe impuestos ni integración. Ejecutarlo escribe datos: use su entorno de pruebas.

## Respuestas y errores

Se mantiene la envoltura del repositorio:

```json
{"data":{"invoiceId":123,"createdAt":"2026-09-15T10:00:00","modifiedAt":null,"details":[{"invoiceDetailId":456,"lineNumber":1}]},"success":true,"message":"Factura creada."}
```

```json
{"code":"VALIDATION_ERROR","message":"Los datos enviados no son válidos.","traceId":"...","errors":[{"field":"Details[0].Quantity","message":"..."}],"success":false}
```

| HTTP | Código | Uso |
|---|---|---|
| 400 | `VALIDATION_ERROR` | JSON, parámetros o validaciones |
| 401 / 403 | `UNAUTHORIZED` / `FORBIDDEN` | Autenticación/autorización |
| 404 | `RESOURCE_NOT_FOUND` | Factura o ruta inexistente |
| 409 | `BUSINESS_RULE_VIOLATION` | Documento duplicado, línea ajena, omisión de líneas o conflicto de escritura |
| 503 | `DATABASE_ERROR` | Conectividad o fallo de base de datos |
| 500 | `INTERNAL_ERROR` | Error inesperado |

`X-Correlation-Id` relaciona respuesta y logs. El middleware no devuelve SQL, credenciales ni excepciones internas. La autorización JWT conserva el esquema base de la referencia; no se agrega una política de sociedades, roles o transiciones de estado no especificada por el requerimiento.

## AWS

Vea [DEPLOY.md](docs/DEPLOY.md). El mismo ensamblado Minimal API se publica en cuatro Lambdas; cada función registra únicamente su operación por `INVOICE_OPERATION`. Se comparte código y se mantienen funciones independientes para permisos, métricas y escalado.

## Verificación

```bash
dotnet test --configuration Release
```

Las pruebas unitarias y de API no requieren conexión a PostgreSQL. Las cuatro pruebas de persistencia se omiten si no existe `INVOICING_TEST_POSTGRES`. Para ejecutarlas en una **base desechable**:

```bash
docker compose -f docker-compose.tests.yml up -d --wait
export INVOICING_TEST_POSTGRES='Host=localhost;Port=55432;Database=invoicing_test;Username=postgres;Password=postgres'
dotnet test --configuration Release
docker compose -f docker-compose.tests.yml down
```

En PowerShell, use `$env:INVOICING_TEST_POSTGRES = 'Host=localhost;Port=55432;Database=invoicing_test;Username=postgres;Password=postgres'`.

El contenedor inicializa exclusivamente una base temporal con el SQL recibido, sin migraciones. Los tests exigen el nombre `invoicing_test` y no crean ni alteran tablas. El workflow de ejemplo usa PostgreSQL 16 desechable. Las credenciales `postgres/postgres` pertenecen únicamente a esas pruebas.

El resultado concreto de la verificación de esta entrega está en [VERIFICACION.md](docs/VERIFICACION.md).
