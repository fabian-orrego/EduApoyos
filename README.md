# EduApoyos

Aplicación para registrar y gestionar solicitudes de apoyo económico.

## Arranque rápido (recomendado)

Con Docker Desktop puedes levantar **SQL Server + API + UI** con un solo comando.

### Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Compose v2)

### 1. Variables de entorno

```bash
cp .env.example .env
```

El archivo `.env` **no se commitea**. Contiene la contraseña de SQL Server, la clave JWT y los puertos publicados.

### 2. Levantar todo

```bash
docker compose up --build
```

La primera vez construye las imágenes (puede tardar varios minutos). Cuando API y UI estén healthy, el servicio `banner` imprime:

```text
========================================================
  EduApoyos listo
  UI:      http://localhost:4200
  Swagger: http://localhost:8080/swagger
  API:     http://localhost:8080/api
========================================================
```

| Servicio | Contenedor | URL por defecto |
|----------|------------|-----------------|
| UI (Angular) | `eduapoyos-web` | http://localhost:4200 |
| Swagger / API | `eduapoyos-api` | http://localhost:8080/swagger |
| SQL Server | `eduapoyos-sqlserver` | `localhost:1433` |

La UI sirve el build de producción y nginx reenvía `/api/*` al contenedor de la API. Al arrancar, la API aplica las migraciones EF Core automáticamente y carga los [datos de prueba](#datos-de-prueba).

### 3. Detener

```bash
docker compose down
```

Para borrar también el volumen de SQL Server:

```bash
docker compose down -v
```

### Puertos

Configurables en `.env`:

| Variable | Default | Uso |
|----------|---------|-----|
| `WEB_PORT` | `4200` | UI |
| `API_PORT` | `8080` | API + Swagger |
| `MSSQL_PORT` | `1433` | SQL Server |

---

## Desarrollo local (sin Docker para API/UI)

Útil si quieres hot-reload de Angular / depurar la API en Visual Studio. SQL Server sigue pudiendo ir en Docker.

### Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) + npm 10+
- Docker Desktop (solo para SQL Server, o todo el stack)

### Solo SQL Server

```bash
cp .env.example .env
docker compose up -d sqlserver
```

Verificar:

```bash
docker compose ps
```

`sqlserver` debe aparecer como `healthy`.

### Cadena de conexión (API local)

`src/EduApoyos.Api/appsettings.Development.json` apunta a `localhost,1433`. Si cambias `MSSQL_SA_PASSWORD` en `.env`, actualiza también esa cadena (o usa `ConnectionStrings__DefaultConnection`).

### API

```bash
dotnet run --project src/EduApoyos.Api
```

- Swagger: https://localhost:7260/swagger (perfil `https`)
- Migraciones: se aplican al arrancar en `Development`

### Frontend

```bash
cd client
npm install
npm start
```

- UI: http://localhost:4200  
- El proxy (`proxy.conf.json`) reenvía `/api` a `https://localhost:7260`

Build de producción:

```bash
npm run build:prod
```

---

## Datos de prueba

Al arrancar la API en desarrollo o con Docker, si la base aún no tiene el usuario asesor demo se inserta un catálogo de prueba **idempotente** (las contraseñas se guardan hasheadas con ASP.NET Identity).

### Credenciales de acceso

| Rol | Email | Contraseña |
|-----|-------|------------|
| Asesor | `asesor@eduapoyos.local` | `Advisor1234*` |
| Estudiante 1 | `estudiante1@eduapoyos.local` | `Student1*` |
| Estudiante 2 | `estudiante2@eduapoyos.local` | `Student2*` |
| … | … | … |
| Estudiante 20 | `estudiante20@eduapoyos.local` | `Student20*` |

Patrón de estudiantes: email `estudianteN@eduapoyos.local` y contraseña `StudentN*` (donde `N` va de 1 a 20).

### Qué datos se crean

- **1 asesor** (`Carolina Mejía Ríos`) con rol Advisor.
- **20 estudiantes** con nombre, documento, programa académico y semestre inventados.
- **46 solicitudes de apoyo** repartidas así:
  - Estudiantes 1–5 → 1 solicitud cada uno
  - Estudiantes 6–10 → 2 solicitudes cada uno
  - Estudiantes 11–15 → 3 solicitudes cada uno
  - Estudiantes 16–19 → 4 solicitudes cada uno
  - Estudiante 20 → sin solicitudes
- Estados intercalados: Pendiente, En revisión, Aprobada y Rechazada.
- Historial de estados con comentarios en cada transición (creación, revisión y decisión final).

### Borrado de estudiantes

Solo se pueden eliminar estudiantes **que no tengan solicitudes de apoyo** asociadas. En el seed, el único caso listo para probar el borrado es:

| Email | Contraseña | Motivo |
|-------|------------|--------|
| `estudiante20@eduapoyos.local` | `Student20*` | No tiene solicitudes |

Si intentas borrar cualquiera de los estudiantes 1–19, la API rechazará la operación porque ya tienen solicitudes registradas.

### Regenerar el seed

Si ya existía la base (o cambiaste el seed) y quieres volver a cargar los datos desde cero:

```bash
docker compose down -v
docker compose up --build
```

En local (sin Docker para la API): elimina la base `EduApoyos` en SQL Server y vuelve a ejecutar la API.

---

## Persistencia con EF Core (Code First)

La capa `EduApoyos.Infrastructure` aloja el `ApplicationDbContext`, configuraciones fluidas, interceptores y migraciones (RN-001 a RN-005):

- **RN-001**: dominio + `IEntityTypeConfiguration` = fuente de verdad del esquema.
- **RN-002**: cambios estructurales solo vía migraciones EF Core.
- **RN-003**: PKs `Guid` generadas en dominio.
- **RN-004**: `UtcDateTimeSaveChangesInterceptor` normaliza `DateTime` a UTC.
- **RN-005**: lecturas con `AsNoTracking()`.

### Migraciones

| Orden | Migración | Contenido |
|-------|-----------|-----------|
| 1 | `InitialIdentity` | Identity + columnas de negocio |
| 2 | `AddStudents` | Tabla `Students` |
| 3 | `AddSupportRequests` | Tabla `SupportRequests` |
| 4 | `AddStatusHistories` | Tabla `StatusHistories` |
| 5 | `ReseedIdentityRoles` | Roles `Advisor` / `Student` (seed determinístico) |

> Las migraciones intermedias `SeedIdentityRoles` / `RemoveIdentityRolesSeed` se eliminaron porque
> se anulaban mutuamente; el seed vigente de roles queda solo en `ReseedIdentityRoles`.

En desarrollo / Docker, la API ejecuta `Database.MigrateAsync()` al arrancar y luego el
[seed de datos de prueba](#datos-de-prueba).

```bash
dotnet tool restore

dotnet ef migrations add <Nombre> \
  --project src/EduApoyos.Infrastructure \
  --startup-project src/EduApoyos.Api \
  --output-dir Persistence/Migrations \
  --context ApplicationDbContext

dotnet ef database update \
  --project src/EduApoyos.Infrastructure \
  --startup-project src/EduApoyos.Api \
  --context ApplicationDbContext
```

---

## Estructura del frontend

```
client/src/app/
├── core/           # Auth, guards, interceptors, services, models
├── shared/         # Componentes y pipes reutilizables
├── features/       # Vertical slices (auth, students, support-requests, …)
└── layout/         # Layout, sidebar, 404
```

- Standalone components + Signals  
- Angular Material 20  
- Guards e interceptors funcionales  
- Lazy-loaded routes  
- Sin NgModules / sin NgRx  
