# Base de datos y migraciones

La capa `EduApoyos.Infrastructure` aloja el `ApplicationDbContext`, las configuraciones fluidas, los interceptores y las migraciones EF Core (Code First).

## Reglas de persistencia

- **RN-001**: dominio + `IEntityTypeConfiguration` = fuente de verdad del esquema.
- **RN-002**: cambios estructurales solo vía migraciones EF Core.
- **RN-003**: PKs `Guid` generadas en dominio.
- **RN-004**: `UtcDateTimeSaveChangesInterceptor` normaliza `DateTime` a UTC.
- **RN-005**: lecturas con `AsNoTracking()`.

## Migraciones actuales

| Orden | Migración | Contenido |
|-------|-----------|-----------|
| 1 | `InitialIdentity` | Identity + columnas de negocio |
| 2 | `AddStudents` | Tabla `Students` |
| 3 | `AddSupportRequests` | Tabla `SupportRequests` |
| 4 | `AddStatusHistories` | Tabla `StatusHistories` |
| 5 | `ReseedIdentityRoles` | Roles `Advisor` / `Student` (seed determinístico) |
| 6 | `AddSupportRequestsStatusUpdatedAtIndex` | Índice `(Status, UpdatedAt)` en `SupportRequests` |

## Scripts SQL (`scripts/`)

Consultas e índice entregables para análisis / operación sobre `SupportRequests`. Ejecutar contra la base `EduApoyos` (p. ej. Azure Data Studio, SSMS o `sqlcmd`).

| Script | Propósito |
|--------|-----------|
| [`01_pending_stale_over_5_days.sql`](../scripts/01_pending_stale_over_5_days.sql) | Pendientes con más de 5 días sin actualización, por antigüedad |
| [`02_count_by_status_and_type_last_month.sql`](../scripts/02_count_by_status_and_type_last_month.sql) | Conteos del último mes por estado y tipo de apoyo |
| [`03_create_index_status_updatedat.sql`](../scripts/03_create_index_status_updatedat.sql) | Justificación y creación del índice no agrupado `(Status, UpdatedAt)` |

> El índice del script 03 también está modelado en EF Core (`SupportRequestConfiguration`) y se aplica con la migración `AddSupportRequestsStatusUpdatedAtIndex` al arrancar la API.

> Las migraciones intermedias `SeedIdentityRoles` / `RemoveIdentityRolesSeed` se eliminaron porque se anulaban mutuamente; el seed vigente de roles queda solo en `ReseedIdentityRoles`.

En desarrollo / Docker, la API ejecuta `Database.MigrateAsync()` al arrancar y luego el [seed de datos de prueba](DemoData.md).

## Comandos EF Core

```bash
dotnet tool restore

# Crear una nueva migración
dotnet ef migrations add <Nombre> \
  --project src/EduApoyos.Infrastructure \
  --startup-project src/EduApoyos.Api \
  --output-dir Persistence/Migrations \
  --context ApplicationDbContext

# Aplicar migraciones pendientes
dotnet ef database update \
  --project src/EduApoyos.Infrastructure \
  --startup-project src/EduApoyos.Api \
  --context ApplicationDbContext

# Deshacer la última migración (solo antes de commitear)
dotnet ef migrations remove \
  --project src/EduApoyos.Infrastructure \
  --startup-project src/EduApoyos.Api \
  --context ApplicationDbContext
```

Para el diseño de capas y el rol de Infrastructure en Clean Architecture, ver [Architecture.md](Architecture.md).
