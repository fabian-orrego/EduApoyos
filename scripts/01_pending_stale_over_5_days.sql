-- =============================================================================
-- Script 1: Solicitudes pendientes con más de 5 días sin actualización
-- =============================================================================
-- Lista las solicitudes en estado Pending (1) cuya última actualización
-- (UpdatedAt) ocurrió hace más de 5 días, ordenadas por antigüedad
-- (las más antiguas primero).
--
-- Valores de Status (SupportRequestStatus):
--   1 = Pending, 2 = UnderReview, 3 = Approved, 4 = Rejected
-- =============================================================================

USE EduApoyos;
GO

SELECT
    sr.Id,
    sr.StudentId,
    sr.SupportType,
    CASE sr.SupportType
        WHEN 1 THEN N'Beca'
        WHEN 2 THEN N'Crédito'
        WHEN 3 THEN N'Subsidio'
        ELSE N'Desconocido'
    END AS SupportTypeName,
    sr.RequestedAmount,
    sr.Description,
    sr.Status,
    sr.RequestedAt,
    sr.UpdatedAt,
    sr.AdvisorId,
    DATEDIFF(DAY, sr.UpdatedAt, SYSUTCDATETIME()) AS DaysWithoutUpdate
FROM dbo.SupportRequests AS sr
WHERE sr.Status = 1 -- Pending
  AND sr.UpdatedAt < DATEADD(DAY, -5, SYSUTCDATETIME())
ORDER BY sr.UpdatedAt ASC;
GO
