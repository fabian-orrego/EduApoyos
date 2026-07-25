using EduApoyos.Domain.Common;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Entities;

public class StatusHistory : Entity
{
    private StatusHistory()
    {
    }

    public StatusHistory(
        Guid supportRequestId,
        SupportRequestStatus previousStatus,
        SupportRequestStatus newStatus,
        Guid changedByUserId,
        string? notes)
    {
        SupportRequestId = supportRequestId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedByUserId = changedByUserId;
        Notes = notes;
        ChangedAt = DateTime.UtcNow;
    }

    public Guid SupportRequestId { get; private set; }

    public SupportRequestStatus PreviousStatus { get; private set; }

    public SupportRequestStatus NewStatus { get; private set; }

    public DateTime ChangedAt { get; private set; }

    public Guid ChangedByUserId { get; private set; }

    public string? Notes { get; private set; }
}
