# Architecture Decisions

## Introducción

Este documento resume las principales decisiones funcionales y técnicas tomadas durante el diseño del proyecto **EduApoyos**.

El objetivo es dejar documentados los criterios utilizados para construir la solución y facilitar futuras modificaciones o ampliaciones del sistema.

---

# Decisiones de Negocio

## 1. Flujo de estados fijo para las solicitudes

Se definió el siguiente flujo de negocio:

```text
Pendiente
      │
      ▼
En Revisión
   │
 ┌─┴────────┐
 ▼          ▼
Aprobada  Rechazada
```

### Justificación

Este flujo cumple con los requerimientos de la prueba técnica y refleja un proceso típico de evaluación de solicitudes.

No se permiten transiciones hacia estados anteriores.

---

## 2. Historial obligatorio de cambios de estado

Cada cambio de estado genera automáticamente un registro en la tabla `StatusHistory`.

### Justificación

Permite mantener trazabilidad completa del proceso.

Facilita auditoría.

Permite conocer:

- quién realizó el cambio
- cuándo ocurrió
- estado anterior
- estado nuevo
- observación

---

## 3. El estudiante únicamente consulta sus propias solicitudes

Los estudiantes nunca podrán acceder a solicitudes de otros usuarios.

### Justificación

Se implementa autorización por recurso además de autorización por rol.

Esto evita exposición de información sensible.

---

## 4. El asesor administra la información académica

Toda la gestión de estudiantes es responsabilidad exclusiva del rol **Asesor**.

El estudiante no puede modificar su información académica.

### Justificación

Se mantiene una separación clara entre funciones administrativas y funciones de autogestión.

---

## 5. La constancia PDF se genera bajo demanda

Las constancias no serán almacenadas en la base de datos.

Serán generadas dinámicamente cada vez que el usuario las solicite.

### Justificación

Reduce almacenamiento.

Evita inconsistencias.

Siempre refleja la información actual.

---

# Decisiones Técnicas

## 1. Clean Architecture

Se adoptó Clean Architecture como arquitectura principal.

### Justificación

Permite separar responsabilidades.

Reduce el acoplamiento.

Facilita pruebas unitarias.

Hace más sencilla la evolución del sistema.

---

## 2. Vertical Slice Architecture

Cada funcionalidad se implementa como un Slice independiente.

Ejemplo:

- CreateStudent
- GetStudents
- CreateSupportRequest

### Justificación

Facilita mantenimiento.

Reduce dependencias entre funcionalidades.

Mejora la organización del proyecto.

---

## 3. CQRS

Se separan operaciones de lectura y escritura.

Commands

- modifican estado

Queries

- únicamente consultan información

### Justificación

Código más limpio.

Mayor mantenibilidad.

Mejor escalabilidad.

---

## 4. MediatR

Todos los casos de uso son ejecutados mediante MediatR.

Los Controllers únicamente envían Commands o Queries.

### Justificación

Elimina dependencias entre Controllers y lógica de negocio.

Facilita pruebas.

Reduce acoplamiento.

---

## 5. Result Pattern

No se utilizan excepciones para reglas de negocio.

Cada operación retorna un objeto Result.

### Justificación

Evita utilizar excepciones como flujo normal.

Hace explícitos los posibles resultados.

Simplifica el manejo de errores.

---

## 6. ProblemDetails

Todos los errores HTTP son devueltos utilizando ProblemDetails (RFC 7807).

### Justificación

Respuestas consistentes.

Mayor compatibilidad con clientes HTTP.

Mejor documentación en Swagger.

---

## 7. FluentValidation

Todas las validaciones se realizan mediante FluentValidation.

No se utilizan DataAnnotations.

### Justificación

Separación entre validaciones y lógica de negocio.

Mayor reutilización.

Código más limpio.

---

## 8. Entity Framework Core Code First

La base de datos es generada a partir del modelo de dominio.

Toda modificación estructural se realiza mediante migraciones.

### Justificación

Mayor control del esquema.

Versionamiento de la base de datos.

Facilidad para evolucionar el modelo.

---

## 9. SQL Server en Docker

La base de datos se ejecuta mediante Docker Compose.

### Justificación

Configuración reproducible.

No requiere instalaciones manuales.

Facilita el desarrollo local.

---

## 10. Angular Standalone + Signals

El frontend utiliza Angular 20 con Standalone Components y Signals.

No se utiliza NgRx.

### Justificación

Arquitectura moderna.

Menor complejidad.

Menor cantidad de código.

Curva de aprendizaje reducida.

---

## 11. QuestPDF

La generación de documentos PDF se realiza mediante QuestPDF.

### Justificación

API moderna.

Excelente integración con .NET.

Mayor facilidad de mantenimiento.

---

## 12. Mapeo manual

No se utiliza AutoMapper.

Todos los DTOs son construidos manualmente.

### Justificación

Mayor control.

Mejor rendimiento.

Código explícito.

Más sencillo de depurar.

---

## 13. JWT sin Refresh Token

El sistema únicamente implementa JWT.

No se implementan Refresh Tokens.

### Justificación

La prueba técnica no lo requiere.

Reduce complejidad.

La expiración del token es configurable.

---

## 14. Cobertura de pruebas

La cobertura mínima será superior al 70% en la capa Application.

### Justificación

Es la capa donde reside la lógica de negocio.

No se realizarán pruebas unitarias sobre Entity Framework ni ASP.NET Core Identity.
