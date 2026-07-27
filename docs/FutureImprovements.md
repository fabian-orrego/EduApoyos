# Posibles mejoras futuras

Aunque el proyecto cumple los requerimientos de la prueba técnica, estas mejoras podrían implementarse en una siguiente versión.

## 1. Server Side Rendering (SSR)

Migrar Angular a Angular SSR.

**Beneficios**

- Mejor SEO.
- Mejor tiempo de carga inicial.
- Mejor experiencia de usuario.

---

## 2. Observabilidad y logging

Incorporar Serilog, Seq y OpenTelemetry.

**Beneficios**

- Auditoría.
- Diagnóstico.
- Trazabilidad de errores.

---

## 3. Infraestructura como código

Administrar la infraestructura mediante Terraform.

**Beneficios**

- Versionamiento de infraestructura.
- Reproducibilidad.
- Automatización.

---

## 4. Pipeline CI/CD completo

Extender el pipeline para incluir SonarQube, análisis de cobertura, análisis de seguridad y publicación automática.

**Beneficios**

- Mayor calidad.
- Automatización.
- Integración continua real.

---

## 5. Caché distribuida

Implementar Redis para consultas frecuentes.

**Beneficios**

- Menor carga sobre SQL Server.
- Mejor rendimiento.
- Escalabilidad.

---

## Conclusión

La solución prioriza simplicidad, mantenibilidad, separación de responsabilidades y facilidad de evolución. Se evitaron decisiones que agregaran complejidad innecesaria para el alcance de la prueba técnica, manteniendo una arquitectura cercana a la usada en proyectos empresariales modernos con .NET y Angular.
