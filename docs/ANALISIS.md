# Análisis previo: esquema y repositorio

Revisión realizada el 15 de septiembre de 2026 antes de implementar el módulo. Repositorio consultado: [DelosiVoucher en a083400](https://github.com/gianfrancogc/DelosiVoucher/tree/a083400addb95eed8d5d919ccb82df7af0512323). El SQL adjunto está conservado sin cambios en `invoicing_db.sql` como evidencia; la aplicación no lo ejecuta.

## Arquitectura observada y correspondencia

| Referencia | Implementación |
|---|---|
| `src/Delosi.VoucherModel.Domain` | `src/Delosi.InvoicingInvoices.Domain` |
| `Domain/Entities`, `Constants`, `Exceptions` | Mismas carpetas y clases de excepción base |
| `Application/DTOs/VoucherModel` | `Application/DTOs/Invoice` |
| `Application/Interfaces/Repositories`, `Interfaces/Services` | Mismos contratos por capa; se incorpora `IUnitOfWork` |
| `Application/Services`, `Mappings`, `Validators` | Servicios, mapeos manuales y FluentValidation |
| `Application/Common`, `Errors/errors.json` | Misma forma de respuesta y catálogo embebido |
| `Application/DependencyInjection.cs` | `AddApplication`, registros scoped y descubrimiento de validadores |
| `Infrastructure/Extensions/ServiceCollectionExtensions.cs` | `AddInfrastructure`, contexto, repositorio y UnitOfWork scoped |
| `Infrastructure/Persistence/Postgres/AppDbContext.cs` | Se completa el contexto existente en la estructura base |
| `Infrastructure/Persistence/Repositories` | `InvoiceRepository` con EF Core |
| `Infrastructure/Options`, `Configuration`, `HealthChecks` | Opciones, Secrets Manager y comprobación PostgreSQL |
| `Api/Program.cs`, `Endpoints`, `Middleware`, `Extensions`, `Options` | Host Minimal API, Serilog, JWT, Swagger y errores globales |
| `test/Delosi.VoucherModel.Tests` | `test/Delosi.InvoicingInvoices.Tests` |

La referencia usa clases de entidades mutables, DTOs record, primary constructors, mapeos manuales sin AutoMapper, repositorios por módulo y servicios de aplicación. No contiene MediatR, CQRS formal, repositorio genérico ni una capa de eventos DDD; no se introducen esos patrones. Domain permanece sin dependencia de EF Core.

## Inconsistencias y decisiones

1. **El repositorio no implementa facturas ni persistencia PostgreSQL activa.** Su caso de negocio consulta Oracle mediante Dapper y stored procedures. El DbContext PostgreSQL está vacío y es opcional. No existe UnitOfWork. Es incompatible copiar esa persistencia literalmente y, a la vez, cumplir EF Core sobre tablas PostgreSQL existentes. Se conserva la arquitectura y se sustituye el acceso a Oracle por el exigido; se documentan las extensiones necesarias.
2. **No hay FOREIGN KEY en el SQL.** `invoice_detail.invoice_id`, `invoice_tax.invoice_id` e `invoice_tax.invoice_detail_id` expresan relaciones, pero no hay constraints referenciales físicas. Tampoco hay FKs a maestros. Se configuran relaciones de navegación en EF con `ClientNoAction`, sin cascadas ni DDL. La API verifica pertenencia de detalles al actualizar. La integridad de escrituras realizadas por otros sistemas no queda garantizada por este script.
3. **“impuesto” se interpreta como `invoice_tax`.** No existe una tabla llamada `impuesto` en el adjunto. `InvoiceTax` y su configuración quedan listas, sin DTOs, servicios, consultas ni escrituras tributarias. `invoice_integration` no se incluye en el modelo. Los importes agregados `tax_total` y `tax_amount` sí pertenecen a factura/detalle y se almacenan tal como se solicitan.
4. **El trigger es la autoridad para `modified_at`.** `created_at` usa `now()` y `modified_at` se genera al actualizar la cabecera. EF trata ambos como valores de servidor; la aplicación no convierte el tipo a `timestamptz`, no agrega columnas y no asigna UTC a un `timestamp` sin zona.
5. **Los UNIQUE no son diferibles.** Renumerar dos líneas directamente puede fallar por `uq_invoice_detail_line`. Se usan números positivos libres durante una primera escritura y los definitivos después, dentro de una transacción con rollback completo.
6. **Actualizar conserva las líneas existentes.** No hay operación de eliminación solicitada y `invoice_tax` debe permanecer sin lógica. PUT exige todos los IDs existentes, permite editar/agregar y rechaza omisiones. Esto evita borrar filas que podrían ser referenciadas por impuestos sin introducir acceso tributario.
7. **Las rutas de la referencia tienen `/api/v1`.** Las solicitadas no. Se conserva el versionado de API y se expone `/facturas/...` sin ese prefijo. Se conserva el host `AddAWSLambdaHosting(RestApi)` y el handler de ensamblado; el template define cuatro recursos con selección de operación.
8. **La optimización se limita a los índices existentes.** EF describe índices, pero nunca los crea. Filtros por sociedad, cliente, marca y tienda más fecha corresponden a índices compuestos; el índice de pendientes es parcial. Ordenar por importe/número, consultar estados fuera del índice parcial o pedir páginas muy profundas puede requerir trabajo adicional en PostgreSQL. No se promete el uso de índices para toda combinación.

## Mapeo del esquema

| Tabla | Columnas | Clave primaria | Unicidad | Índices adicionales | CHECK |
|---|---:|---|---|---:|---:|
| `invoice` | 30 | `pk_invoice` | `uq_invoice_document` | 5 | 5 |
| `invoice_detail` | 16 | `pk_invoice_detail` | `uq_invoice_detail_line` | 3 | 3 |
| `invoice_tax` | 8 | `pk_invoice_tax` | — | 2 | 3 |

Se mapean todas las columnas de esas tres tablas: nombre, tipo PostgreSQL, nulabilidad, longitudes, precisión/escala, identity by default y valores predeterminados. Los UNIQUE se representan con `HasIndex(...).IsUnique()` con el nombre del índice que PostgreSQL ya crea para la constraint, evitando convertir campos editables en alternate keys inmutables de EF. Se elimina la convención automática de índices de FKs para que el modelo no agregue índices inexistentes. Las relaciones en EF son metadatos de navegación: no se afirma que existan constraints FK en PostgreSQL.

La tabla no especifica un schema cualificado: el mapeo utiliza el `search_path` de la conexión. Si las tablas reales están en un schema específico, configure `Search Path=nombre_schema` en la cadena. No se fuerza ni crea `public`.

Se validan las restricciones de importes y fechas existentes. Como reglas del contrato HTTP se exigen IDs positivos, textos obligatorios no vacíos, moneda de tres letras mayúsculas y de 1 a 1000 líneas. No se implementan transiciones SAP, cálculo de impuestos, correlativos ni validación contra maestros ausentes.

## Transacciones y consultas

- Crear agrega un grafo y usa un único `SaveChangesAsync`; EF ejecuta inserciones por lotes, respetando la identidad de cabecera antes de los detalles. No se usa `SaveChanges` por línea ni se presenta esto como una operación COPY/bulk.
- Actualizar usa UnitOfWork, transacción `ReadCommitted` y bloqueo de la cabecera `FOR UPDATE`, con ID parametrizado a través de EF Core. El bloqueo serializa actualizaciones de esta API; no agrega un token de versión ni garantiza que otros escritores sigan el mismo protocolo. Tampoco impide que un cliente sobrescriba intencionadamente datos con una copia antigua.
- Consultar usa una proyección de cabecera y detalles en una consulta SQL, sin lazy loading.
- Listar mantiene `IQueryable` hasta `Count` y `Select/Skip/Take`, ejecuta dos consultas y no carga detalles/impuestos. El conteo y la página son lecturas separadas y pueden observar cambios concurrentes.
- No hay reintentos automáticos de escritura ante un resultado de COMMIT incierto. El documento único se protege con la constraint de PostgreSQL; un reintento de Crear puede devolver 409 y debe resolverse consultando por sociedad, tipo, serie y número.

Estas decisiones se apoyan en la documentación de [proyecciones y consultas eficientes de EF Core](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying), [identity y valores generados de Npgsql](https://www.npgsql.org/efcore/modeling/generated-properties.html) y [tipos de fecha/hora de Npgsql](https://www.npgsql.org/doc/types/datetime.html).
