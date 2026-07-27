# Arquitectura de la Solución

## Introducción

**EduApoyos** es una aplicación web desarrollada para gestionar solicitudes de apoyo económico (becas, créditos y subsidios) para estudiantes de una institución de educación superior.

El proyecto fue diseñado utilizando una arquitectura moderna basada en **Clean Architecture**, **Vertical Slice Architecture** y **CQRS**, priorizando la separación de responsabilidades, la mantenibilidad, la escalabilidad y la facilidad para realizar pruebas unitarias.

Aunque el alcance corresponde a una prueba técnica, la solución sigue principios y patrones ampliamente utilizados en proyectos empresariales desarrollados con .NET y Angular.

---

# Objetivos de la arquitectura

La arquitectura busca cumplir los siguientes objetivos:

- Separar claramente las responsabilidades de cada capa.
- Reducir el acoplamiento entre componentes.
- Facilitar la evolución del sistema.
- Mejorar la mantenibilidad.
- Facilitar las pruebas unitarias.
- Mantener una alta legibilidad del código.
- Permitir agregar nuevas funcionalidades sin afectar las existentes.

---

# Arquitectura General

```
                        Angular 20
                             │
                     HttpClient + JWT
                             │
                    ASP.NET Core Web API
                             │
────────────────────────────────────────────────────────

                 Presentation (API)

────────────────────────────────────────────────────────

                  Application (CQRS)

────────────────────────────────────────────────────────

                       Domain

────────────────────────────────────────────────────────

                   Infrastructure

────────────────────────────────────────────────────────

                    SQL Server
```

Cada capa tiene una responsabilidad claramente definida y únicamente puede depender de las capas inferiores autorizadas.

---

# Clean Architecture

La arquitectura principal del proyecto es **Clean Architecture**.

Su objetivo es aislar la lógica de negocio de cualquier detalle de infraestructura como bases de datos, frameworks o servicios externos.

Las dependencias siempre apuntan hacia el dominio.

```
API
        ↓

Application
        ↓

Domain

Infrastructure
        ↓
Domain
```

Esto permite que la lógica de negocio permanezca independiente de la tecnología utilizada.

---

# Vertical Slice Architecture

Además de Clean Architecture, el proyecto utiliza **Vertical Slice Architecture**.

En lugar de organizar el código únicamente por capas técnicas (Controllers, Services, Repositories), cada funcionalidad se desarrolla como un módulo independiente.

Ejemplo:

```
SupportRequests

    Commands

        CreateSupportRequest

            Command

            Handler

            Validator

            DTO

    Queries

        GetSupportRequest

            Query

            Handler

            DTO
```

Cada Slice contiene únicamente los archivos necesarios para implementar una funcionalidad específica.

### Beneficios

- Menor acoplamiento.
- Mayor cohesión.
- Más fácil de mantener.
- Facilita el trabajo con CQRS.

---

# CQRS

La capa Application implementa el patrón **Command Query Responsibility Segregation (CQRS)**.

Las operaciones de escritura y lectura se encuentran completamente separadas.

## Commands

Los Commands representan operaciones que modifican el estado del sistema.

Ejemplos:

- Crear estudiante.
- Crear solicitud.
- Cambiar estado.
- Registrar usuario.

Los Commands nunca devuelven colecciones de datos.

---

## Queries

Las Queries representan operaciones de consulta.

Ejemplos:

- Obtener estudiantes.
- Consultar solicitud.
- Consultar historial.

Las Queries nunca modifican información.

Todas las consultas utilizan:

```
AsNoTracking()
```

para mejorar el rendimiento.

---

# MediatR

Todos los casos de uso son ejecutados mediante **MediatR**.

Los Controllers únicamente reciben la petición HTTP y delegan la ejecución al correspondiente Command o Query.

```
Controller

↓

Mediator

↓

Handler

↓

Repository
```

Esto reduce considerablemente el acoplamiento entre la API y la lógica de negocio.

---

# Organización de la solución

La solución está dividida en cuatro proyectos principales.

```
EduApoyos.Api

EduApoyos.Application

EduApoyos.Domain

EduApoyos.Infrastructure
```

---

# EduApoyos.Domain

Es la capa más estable del sistema.

Contiene:

- Entidades
- Enumeraciones
- Interfaces
- Reglas del dominio
- Constantes

No tiene dependencias hacia ninguna otra capa.

Nunca conoce:

- Entity Framework
- SQL Server
- ASP.NET Core
- Identity
- MediatR

---

# EduApoyos.Application

Implementa toda la lógica de negocio.

Contiene:

- Commands
- Queries
- Handlers
- Validators
- DTOs
- Interfaces
- Result Pattern

Esta capa conoce únicamente al dominio.

No conoce:

- SQL Server
- EF Core
- Controllers
- Swagger

---

# EduApoyos.Infrastructure

Implementa todos los detalles técnicos.

Incluye:

- Entity Framework Core
- SQL Server
- ASP.NET Identity
- JWT
- QuestPDF
- Repositorios
- Persistencia

Esta capa implementa las interfaces definidas por Application y Domain.

---

# EduApoyos.Api

Es la capa de presentación.

Su responsabilidad es:

- Exponer endpoints REST.
- Configurar autenticación.
- Configurar autorización.
- Configurar Swagger.
- Registrar dependencias.
- Configurar Middlewares.

Los Controllers nunca contienen lógica de negocio.

---

# Flujo de una petición

El siguiente diagrama muestra el recorrido de una petición.

```
Cliente

↓

Controller

↓

MediatR

↓

Handler

↓

Repository

↓

DbContext

↓

SQL Server
```

La respuesta sigue el mismo recorrido en sentido inverso.

---

# Persistencia

La persistencia se implementa mediante:

- Entity Framework Core 8
- SQL Server
- Code First
- Fluent API

Todas las entidades son configuradas utilizando:

```
IEntityTypeConfiguration<TEntity>
```

No se utilizan DataAnnotations para mapear entidades.

---

# Validaciones

Todas las validaciones se implementan mediante **FluentValidation**.

Las reglas de validación permanecen separadas de la lógica de negocio.

Los Controllers y Handlers nunca realizan validaciones manuales.

---

# Manejo de errores

La aplicación utiliza dos mecanismos complementarios.

## Result Pattern

Los errores de negocio no generan excepciones.

Cada caso de uso retorna un objeto Result indicando éxito o fallo.

Esto hace el flujo mucho más explícito y fácil de mantener.

---

## ProblemDetails

Las excepciones inesperadas son capturadas por un Middleware global.

Las respuestas HTTP utilizan el estándar RFC 7807 mediante ProblemDetails.

Esto garantiza respuestas consistentes para todos los consumidores del API.

---

# Seguridad

La autenticación utiliza:

- ASP.NET Core Identity
- JWT Bearer Authentication

La autorización se implementa mediante Roles.

Roles soportados:

- Asesor
- Estudiante

Adicionalmente, algunas operaciones utilizan autorización por recurso para impedir que un estudiante consulte solicitudes de otros usuarios.

---

# Frontend

El frontend fue desarrollado utilizando:

- Angular 20
- Standalone Components
- Signals
- Angular Material

No se utiliza NgRx debido al tamaño del proyecto.

La comunicación con el backend se realiza mediante HttpClient y JWT.

---

# Base de Datos

La base de datos se ejecuta mediante SQL Server utilizando Docker Compose.

El esquema se genera utilizando migraciones de Entity Framework Core.

La aplicación aplica automáticamente las migraciones pendientes al iniciar en ambiente de desarrollo.

---

# Pruebas

Las pruebas unitarias se concentran en la capa Application.

Se utilizan:

- xUnit
- Moq

Se busca una cobertura mínima del 70%, enfocándose en la lógica de negocio y evitando probar componentes del framework como Entity Framework Core o ASP.NET Core Identity.

---

# Principios de Diseño

Durante el desarrollo del proyecto se siguieron los siguientes principios:

- SOLID
- DRY (Don't Repeat Yourself)
- KISS (Keep It Simple, Stupid)
- Separation of Concerns
- Single Responsibility Principle
- Dependency Inversion Principle

Estos principios ayudan a mantener un código limpio, desacoplado y fácil de extender.

---

# Beneficios de la arquitectura adoptada

La combinación de Clean Architecture, Vertical Slice y CQRS proporciona las siguientes ventajas:

- Código altamente organizado.
- Separación clara de responsabilidades.
- Facilidad para realizar pruebas unitarias.
- Menor acoplamiento entre componentes.
- Mayor facilidad para incorporar nuevas funcionalidades.
- Arquitectura alineada con buenas prácticas utilizadas en proyectos empresariales.
- Escalabilidad sin necesidad de realizar cambios estructurales importantes.

---

# Conclusión

La arquitectura de **EduApoyos** fue diseñada priorizando la mantenibilidad y la claridad del código sobre la complejidad innecesaria.

Aunque el proyecto corresponde a una prueba técnica, las decisiones adoptadas siguen prácticas ampliamente utilizadas en aplicaciones empresariales modernas desarrolladas con **.NET 8**, **Angular 20** y **Clean Architecture**, proporcionando una base sólida para futuras evoluciones del sistema.