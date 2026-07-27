# EduApoyos

Aplicación web para registrar y gestionar solicitudes de apoyo económico (becas, créditos y subsidios) en una institución de educación superior.

Stack principal: **.NET 8**, **Angular 20**, **SQL Server**, **Docker Compose**.

---

## Documentación

| Documento | Contenido |
|-----------|-----------|
| [Architecture](docs/Architecture.md) | Clean Architecture, Vertical Slice, CQRS y estructura de la solución |
| [Key decisions](docs/keyDecisions.md) | Decisiones de negocio y técnicas del proyecto |
| [Future improvements](docs/FutureImprovements.md) | Mejoras posibles para una siguiente versión |
| [Local development](docs/LocalDevelopment.md) | Arranque de API/UI en local (hot-reload) |
| [Demo data](docs/DemoData.md) | Usuarios, contraseñas y datos de prueba |
| [Database](docs/Database.md) | EF Core, migraciones, scripts SQL e índices |
| [Azure deployment proposal](docs/AzureDeploymentProposal.md) | Propuesta de despliegue productivo en Azure (App Service, SQL, Key Vault, etc.) |
| [CI pipeline (backend)](.github/workflows/ci.yml) | GitHub Actions: restore, build Release, tests y publish de la API (verificado en ejecución) |

---

## Arranque rápido (Docker)

Con Docker Desktop puedes levantar **SQL Server + API + UI** con un solo comando.

### Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Compose v2)

### 1. Abrir una terminal en la raíz del repositorio

Abre una terminal y navega hasta la raíz del proyecto EduApoyos, donde está el archivo `docker-compose.yml`:

Los comandos siguientes asumen que trabajas desde ese directorio.

### 2. Variables de entorno

```bash
cp .env.example .env
```

El archivo `.env` **no se commitea**. Contiene la contraseña de SQL Server, la clave JWT y los puertos publicados.

### 3. Levantar todo

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

La UI sirve el build de producción y nginx reenvía `/api/*` al contenedor de la API. Al arrancar, la API aplica las migraciones EF Core y carga los [datos de prueba](docs/DemoData.md).

### 4. Detener

```bash
docker compose down
```

Para borrar también el volumen de SQL Server (incluye datos de prueba):

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

### Credenciales rápidas

| Rol | Email | Contraseña |
|-----|-------|------------|
| Asesor | `asesor@eduapoyos.local` | `Advisor1234*` |
| Estudiante | `estudiante1@eduapoyos.local` | `Student1*` |

Detalle completo (20 estudiantes, solicitudes y borrado): [Demo data](docs/DemoData.md).

---

## ¿Necesitas otra forma de arrancar?

- Hot-reload / Visual Studio → [Local development](docs/LocalDevelopment.md)
- Solo base de datos / migraciones → [Database](docs/Database.md)
