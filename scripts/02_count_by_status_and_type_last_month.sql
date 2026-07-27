-- =============================================================================
-- Script 2: Total de solicitudes por estado y tipo de apoyo (último mes)
-- =============================================================================
-- Cuenta las solicitudes creadas en los últimos 30 días (RequestedAt),
-- agrupadas por Status y SupportType.
--
-- Valores de Status (SupportRequestStatus):
--   1 = Pending, 2 = UnderReview, 3 = Approved, 4 = Rejected
-- Valores de SupportType:
--   1 = Scholarship (Beca), 2 = Loan (Crédito), 3 = Subsidy (Subsidio)
-- =============================================================================

USE EduApoyos;
GO

SELECT
    sr.Status,
    CASE sr.Status
        WHEN 1 THEN N'Pendiente'
        WHEN 2 THEN N'En revisión'
        WHEN 3 THEN N'Aprobada'
        WHEN 4 THEN N'Rechazada'
        ELSE N'Desconocido'
    END AS StatusName,
    sr.SupportType,
    CASE sr.SupportType
        WHEN 1 THEN N'Beca'
        WHEN 2 THEN N'Crédito'
        WHEN 3 THEN N'Subsidio'
        ELSE N'Desconocido'
    END AS SupportTypeName,
    COUNT(*) AS TotalRequests
FROM dbo.SupportRequests AS sr
WHERE sr.RequestedAt >= DATEADD(MONTH, -1, SYSUTCDATETIME())
GROUP BY sr.Status, sr.SupportType
ORDER BY sr.Status, sr.SupportType;
GO
