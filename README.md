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

## Ejecutar la API

```bash
dotnet run --project src/EduApoyos.Api
```

Swagger estará disponible en `https://localhost:{puerto}/swagger`.
