# Desarrollo local

Guía para trabajar con hot-reload de Angular o depurar la API en Visual Studio / Rider. SQL Server puede seguir corriendo en Docker.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) + npm 10+
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (al menos para SQL Server)

## Solo SQL Server

```bash
cp .env.example .env
docker compose up -d sqlserver
```

Verificar:

```bash
docker compose ps
```

El servicio `sqlserver` debe aparecer como `healthy`.

## Cadena de conexión

`src/EduApoyos.Api/appsettings.Development.json` apunta a `localhost,1433`. Si cambias `MSSQL_SA_PASSWORD` en `.env`, actualiza también esa cadena (o usa la variable de entorno `ConnectionStrings__DefaultConnection`).

## API

```bash
dotnet run --project src/EduApoyos.Api
```

- Swagger: https://localhost:7260/swagger (perfil `https`)
- Migraciones y [datos de prueba](DemoData.md): se aplican al arrancar en `Development`

## Frontend

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

## Estructura del frontend

```text
client/src/app/
├── core/           # Auth, guards, interceptors, services, models
├── shared/         # Componentes y pipes reutilizables
├── features/       # Vertical slices (auth, students, support-requests, …)
└── layout/         # Layout, sidebar, 404
```

Convenciones aplicadas:

- Standalone components + Signals
- Angular Material 20
- Guards e interceptors funcionales
- Lazy-loaded routes
- Sin NgModules / sin NgRx

Para el diseño de capas y patrones del backend, ver [Architecture.md](Architecture.md).
