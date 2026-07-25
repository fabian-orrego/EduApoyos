using EduApoyos.Domain.Common;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Entities;

public class SupportRequest : Entity
{
    private SupportRequest()
    {
        Description = string.Empty;
    }

    public SupportRequest(
        Guid studentId,
        SupportType supportType,
        decimal requestedAmount,
        string description)
    {
        StudentId = studentId;
        SupportType = supportType;
        RequestedAmount = requestedAmount;
        Description = description;
        Status = SupportRequestStatus.Pending;
        RequestedAt = DateTime.UtcNow;
        UpdatedAt = RequestedAt;
    }

    public Guid StudentId { get; private set; }

    public SupportType SupportType { get; private set; }

    public decimal RequestedAmount { get; private set; }

    public string Description { get; private set; }

    public SupportRequestStatus Status { get; private set; }

    public DateTime RequestedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Guid? AdvisorId { get; private set; }
}
