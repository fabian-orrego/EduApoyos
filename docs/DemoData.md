# Datos de prueba

Al arrancar la API en desarrollo o con Docker, si la base aún no tiene el usuario asesor demo se inserta un catálogo de prueba **idempotente**. Las contraseñas se almacenan hasheadas con ASP.NET Identity.

## Credenciales de acceso

| Rol | Email | Contraseña |
|-----|-------|------------|
| Asesor | `asesor@eduapoyos.local` | `Advisor1234*` |
| Estudiante 1 | `estudiante1@eduapoyos.local` | `Student1*` |
| Estudiante 2 | `estudiante2@eduapoyos.local` | `Student2*` |
| … | … | … |
| Estudiante 20 | `estudiante20@eduapoyos.local` | `Student20*` |

Patrón de estudiantes: email `estudianteN@eduapoyos.local` y contraseña `StudentN*` (donde `N` va de 1 a 20).

## Qué datos se crean

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

## Borrado de estudiantes

Solo se pueden eliminar estudiantes **que no tengan solicitudes de apoyo** asociadas. En el seed, el único caso listo para probar el borrado es:

| Email | Contraseña | Motivo |
|-------|------------|--------|
| `estudiante20@eduapoyos.local` | `Student20*` | No tiene solicitudes |

Si intentas borrar cualquiera de los estudiantes 1–19, la API rechazará la operación porque ya tienen solicitudes registradas.

## Regenerar el seed

Si ya existía la base (o cambiaste el seed) y quieres volver a cargar los datos desde cero:

```bash
docker compose down -v
docker compose up --build
```

En local (sin Docker para la API): elimina la base `EduApoyos` en SQL Server y vuelve a ejecutar la API.
