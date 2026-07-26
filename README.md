# EduApoyos

Aplicación para registrar y gestionar solicitudes de apoyo económico.

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (con soporte para Docker Compose v2)

## Base de datos (SQL Server con Docker)

El proyecto incluye un `docker-compose.yml` con una instancia lista de SQL Server 2022 (edición Developer) para acelerar la configuración local.

### 1. Configurar variables de entorno

Copiar el archivo de ejemplo y ajustar la contraseña del usuario `SA` si se desea:

```bash
cp .env.example .env
```

El archivo `.env` **no se commitea** (está ignorado por Git). La contraseña debe cumplir la política de SQL Server: mínimo 8 caracteres con mayúsculas, minúsculas, números y símbolos.

### 2. Levantar el contenedor

```bash
docker compose up -d
```

Esto crea el contenedor `eduapoyos-sqlserver` escuchando en `localhost:1433` y un volumen persistente `eduapoyos-sqlserver-data` para conservar los datos entre reinicios.

### 3. Verificar el estado

```bash
docker compose ps
```

El servicio debe aparecer como `healthy` en pocos segundos (el healthcheck ejecuta `SELECT 1` con `sqlcmd`).

### 4. Detener el contenedor

```bash
docker compose down
```

Para eliminar también los datos persistidos:

```bash
docker compose down -v
```

## Cadena de conexión

`src/EduApoyos.Api/appsettings.Development.json` ya apunta al contenedor local:

```
Server=localhost,1433;Database=EduApoyos;User Id=sa;Password=EduApoyos!2026;TrustServerCertificate=True;MultipleActiveResultSets=True
```

Si se cambia la contraseña en el `.env`, actualizar también esta cadena (o sobreescribirla mediante la variable de entorno `ConnectionStrings__DefaultConnection`).

## Persistencia con EF Core (Code First)

La capa `EduApoyos.Infrastructure` es la responsable del acceso a datos y aloja el
`ApplicationDbContext`, las configuraciones fluidas de cada entidad, los interceptores y las
migraciones. El proyecto se apega a las reglas RN-001 a RN-005:

- **RN-001**: el modelo de dominio + `IEntityTypeConfiguration` son la única fuente de verdad del esquema.
- **RN-002**: cualquier cambio estructural se realiza mediante migraciones EF Core (no scripts SQL manuales).
- **RN-003**: todas las claves primarias son `Guid` y se generan en el dominio.
- **RN-004**: el interceptor `UtcDateTimeSaveChangesInterceptor` normaliza todos los `DateTime` a UTC antes de persistir.
- **RN-005**: las consultas de solo lectura deben usar `AsNoTracking()`.

### Estructura de las migraciones

Cada tabla tiene su propia migración para mantener el historial ordenado:

| Orden | Migración | Contenido |
|-------|-----------|-----------|
| 1 | `InitialIdentity` | Tablas de ASP.NET Core Identity (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`) con columnas de negocio (`FullName`, `Role`, `RegisteredAt`). |
| 2 | `AddStudents` | Tabla `Students` (FK a `AspNetUsers`, índice único por documento, `CHECK` sobre `Semester`). |
| 3 | `AddSupportRequests` | Tabla `SupportRequests` (FK a `Students` y a `AspNetUsers`, índices por `Status`, `StudentId`, `AdvisorId`, `CHECK` sobre `RequestedAmount`). |
| 4 | `AddStatusHistories` | Tabla `StatusHistories` (FK a `SupportRequests` y a `AspNetUsers`, índices por `SupportRequestId`, `ChangedByUserId` y compuesto por `SupportRequestId, ChangedAt`). |

### Aplicar las migraciones

En desarrollo, la API ejecuta `Database.MigrateAsync()` durante el arranque (ver
`Api/Configuration/DatabaseStartupExtensions.cs`). No se debe habilitar la aplicación automática
en producción; ahí se despliegan las migraciones manualmente con el CLI.

Herramienta local ya restaurada (`.config/dotnet-tools.json`):

```bash
dotnet tool restore
```

Comandos habituales:

```bash
# Crear una nueva migración (una por cambio estructural, RN-002)
dotnet ef migrations add <Nombre> \
  --project src/EduApoyos.Infrastructure \
  --startup-project src/EduApoyos.Api \
  --output-dir Persistence/Migrations \
  --context ApplicationDbContext

# Aplicar todas las migraciones pendientes contra la base configurada en el entorno
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

## Ejecutar la API

```bash
dotnet run --project src/EduApoyos.Api
```

Al iniciar en `Development` se aplican automáticamente las migraciones pendientes contra el SQL
Server configurado. Swagger estará disponible en `https://localhost:7260/swagger`.

## Frontend Angular

El cliente Angular 20 vive en la carpeta `client/`.

### Requisitos

- [Node.js 20+](https://nodejs.org/)
- npm 10+

### Instalación

```bash
cd client
npm install
```

### Ejecución en desarrollo

```bash
npm start
```

El servidor se levanta en `http://localhost:4200` y proxeas las llamadas `/api/*` a la API en `https://localhost:7260` mediante `client/proxy.conf.json`. Asegurarse de tener la API corriendo antes de iniciar el frontend.

### Build de producción

```bash
npm run build:prod
```

### Estructura

```
client/src/app/
├── core/           # Auth, guards, interceptors, services, models, constants
├── shared/         # Reusable components, pipes, utilities
├── features/       # Vertical slices (auth, dashboard, students, support-requests)
└── layout/         # Main layout, sidebar, 404
```

Reglas aplicadas:

- Standalone components + Signals
- Angular Material 20
- Functional guards (`authGuard`, `guestGuard`)
- Functional HTTP interceptors (`authInterceptor`, `loadingInterceptor`, `errorInterceptor`)
- Lazy-loaded routes
- Sin NgModules, sin NgRx
