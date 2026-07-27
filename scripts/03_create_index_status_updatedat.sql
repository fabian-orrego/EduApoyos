-- =============================================================================
-- Script 3: Índice no agrupado sobre SupportRequests
-- =============================================================================
--
-- Justificación
-- -------------
-- La consulta operativa de "pendientes con más de 5 días sin actualización"
-- (script 01) filtra por Status = Pending y por un rango sobre UpdatedAt, y
-- ordena por UpdatedAt ASC (antigüedad).
--
-- Hoy existe IX_SupportRequests_Status (solo Status). Eso permite localizar
-- pendientes, pero no ayuda a filtrar ni ordenar por UpdatedAt: SQL Server
-- aún debe leer las filas (lookup/scan) y ordenarlas.
--
-- Un índice compuesto (Status, UpdatedAt):
--   1. Hace seek por Status = 1 (Pending).
--   2. Recorre en orden solo el rango UpdatedAt < umbral de 5 días.
--   3. Satisface el ORDER BY UpdatedAt sin Sort adicional (mismo orden del índice).
--
-- Como Status es la columna líder, este índice también cubre búsquedas solo
-- por Status (p. ej. listados filtrados), por lo que puede reemplazar al índice
-- simple IX_SupportRequests_Status sin pérdida de cobertura.
--
-- Nota: en la aplicación el índice también se declara en
-- SupportRequestConfiguration y se aplica vía migración EF Core. Este script
-- es el entregable SQL idempotente para ejecución manual / revisión.
-- =============================================================================

USE EduApoyos;
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_SupportRequests_Status'
      AND object_id = OBJECT_ID(N'dbo.SupportRequests')
)
BEGIN
    DROP INDEX IX_SupportRequests_Status ON dbo.SupportRequests;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_SupportRequests_Status_UpdatedAt'
      AND object_id = OBJECT_ID(N'dbo.SupportRequests')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_SupportRequests_Status_UpdatedAt
        ON dbo.SupportRequests (Status, UpdatedAt);
END
GO
