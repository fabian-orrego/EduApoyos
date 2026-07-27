# Propuesta de Despliegue en Azure

## Introducción

Aunque el alcance del proyecto contempla únicamente la ejecución en un entorno local mediante Docker Compose, en un escenario productivo se propone la siguiente arquitectura utilizando servicios administrados de Microsoft Azure.

| Servicio | Uso propuesto | Justificación |
|----------|---------------|---------------|
| **Azure App Service** | Hospedar el Backend (.NET 8) y el Frontend Angular | Servicio PaaS administrado que simplifica el despliegue, escalado y mantenimiento. Se recomienda iniciar con el plan **Basic (B1)** y escalar según la demanda. |
| **Azure SQL Database** | Base de datos relacional | Compatible con Entity Framework Core y SQL Server. Para la carga estimada del sistema se recomienda el plan **Basic**, con posibilidad de evolucionar a **Standard** si aumenta el número de usuarios. |
| **Azure Blob Storage** | Almacenamiento de documentos (opcional) | La versión actual genera los PDF bajo demanda y no los almacena. En futuras versiones podría utilizarse para guardar constancias, documentos anexos o certificados. |
| **Azure Key Vault** | Gestión de secretos | Almacenar de forma segura cadenas de conexión, claves JWT, certificados y demás secretos de la aplicación sin incluirlos en el código fuente. |
| **Azure Application Insights** | Monitoreo y telemetría | Permite recopilar métricas, excepciones, tiempos de respuesta y trazabilidad de las solicitudes para facilitar el diagnóstico de problemas. |
| **Azure Monitor** | Supervisión de la plataforma | Centraliza métricas, alertas y registros de los recursos desplegados para monitorear la disponibilidad y el rendimiento del sistema. |
| **Azure Log Analytics Workspace** | Centralización de logs | Consolida los registros generados por la aplicación y los servicios de Azure, facilitando consultas, auditorías y análisis operativos. |

## Arquitectura propuesta

```text
                    Usuarios
                        │
                        ▼
              Azure App Service
        (Frontend Angular + API .NET)
                        │
        ┌───────────────┼────────────────┐
        ▼               ▼                ▼
 Azure SQL Database  Azure Key Vault  Azure Blob Storage
                        │
                        ▼
      Application Insights + Azure Monitor
                        │
                        ▼
             Log Analytics Workspace
```

## Conclusión

La arquitectura propuesta utiliza servicios PaaS administrados, reduciendo la carga operativa y permitiendo escalar la solución conforme aumente el número de usuarios. Para el tamaño estimado de **EduApoyos**, los planes básicos de Azure son suficientes y permiten una evolución gradual sin cambios significativos en la arquitectura de la aplicación.