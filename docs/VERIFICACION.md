# Verificación de esta entrega

Fecha: 15 de septiembre de 2026.

| Comprobación | Resultado |
|---|---|
| Restore de dependencias con lock files y compilación Release | Correctos; solución completa y cinco proyectos |
| Publicación para `linux-x64`, .NET 8, framework dependent | Correcta; configuración y certificados incluidos |
| Casos de prueba de validación, API, modelo y persistencia | 30 aprobados, 0 fallidos, 0 omitidos |
| Colección Postman ejecutada con Newman contra el host HTTP | 4 solicitudes correctas, 8 assertions aprobadas |
| Estados del ciclo Crear → Actualizar → Consultar → Listar | 201 → 200 → 200 → 200 |
| Registros en `invoice_tax` / `invoice_integration` tras el ciclo | 0 / 0 |
| `serverless.template`, validación CloudFormation/SAM con cfn-lint | Sin errores ni advertencias |
| Tablas/columnas de EF | 3 tablas, 54 columnas, sin propiedades sombra |
| Índices y CHECK en el modelo | 12 índices, incluidos los dos respaldos de UNIQUE; 11 CHECK |
| Lecturas de consultar/listar | 1 / 2 comandos SQL; ChangeTracker vacío |
| Actualización fallida después de renumerar líneas | Rollback comprobado: cabecera y números originales preservados |
| Intercambio de números de línea y auditoría | IDs preservados y trigger de `modified_at` comprobado |

Los resultados detallados están en `verification/test-results.json` y `verification/postman-results.json`.

## Alcance de la ejecución

Se compiló con SDK .NET 8.0.413. Los lock files contienen las versiones NuGet efectivamente restauradas; los proyectos mantienen las familias de versiones de la referencia.

El ejecutor VSTest convencional no pudo iniciar su host por una limitación de información de procesos de este entorno. Los mismos métodos de prueba xUnit y sus assertions se ejecutaron secuencialmente mediante un runner en proceso; el código de producción y los tests no se modificaron para omitir comprobaciones. El workflow incluido permite ejecutarlos con `dotnet test` sobre un runner normal.

Las pruebas de persistencia y el ciclo HTTP usaron un PostgreSQL aislado, compilado a WebAssembly: PostgreSQL 18.3 / PGlite 0.5.8, mediante Npgsql. Se inicializó con una copia exacta del SQL del usuario. Esta ejecución comprueba SQL, tipos, proyecciones, constraints, trigger y rollback; no sustituye una prueba de concurrencia, rendimiento o despliegue sobre Aurora/PostgreSQL 15/16. El proyecto incluye configuración para repetir las pruebas sobre un contenedor PostgreSQL 16 desechable.

No se desplegó en AWS, no se accedió a la base existente y no se validaron sus credenciales, conectividad VPC o proveedor JWT. La plantilla y la publicación se verificaron localmente.

SHA-256 del SQL original y de la copia incluida:

```text
63cd5a758bc602270f715cbd74cea4157e12c5225c02f9fc41096f902e781a3e
```
