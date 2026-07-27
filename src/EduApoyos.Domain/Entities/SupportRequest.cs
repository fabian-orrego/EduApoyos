using EduApoyos.Domain.Common;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Entities;

/// <summary>
/// Aggregate root that represents a request for financial support submitted by a student
/// (US-013). The aggregate encapsulates the transitions allowed by the process (US-016)
/// so no external layer can move the request between states without going through the
/// business rules defined here.
/// </summary>
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

    /// <summary>
    /// True when the aggregate is in a terminal state (Approved / Rejected) and can no longer be
    /// modified per US-016 RN-2 and RN-3.
    /// </summary>
    public bool IsFinalized =>
        Status is SupportRequestStatus.Approved or SupportRequestStatus.Rejected;

    /// <summary>
    /// Updates the mutable business fields (type, amount, description) as long as the request
    /// has not reached a terminal state (US-016 nota #1). The caller is expected to validate
    /// field constraints beforehand via FluentValidation.
    /// </summary>
    public void UpdateDetails(
        SupportType supportType,
        decimal requestedAmount,
        string description)
    {
        if (IsFinalized)
        {
            throw new InvalidOperationException(
                "No es posible modificar una solicitud aprobada o rechazada.");
        }

        SupportType = supportType;
        RequestedAmount = requestedAmount;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Applies a status transition (US-016). The following moves are the only ones allowed:
    /// Pending → UnderReview, UnderReview → Approved, UnderReview → Rejected. Any other
    /// combination is rejected with <see cref="InvalidOperationException"/> so the invariant is
    /// enforced from the aggregate itself. The advisor id is recorded to keep traceability.
    /// </summary>
    public void ChangeStatus(
        SupportRequestStatus newStatus,
        Guid advisorId)
    {
        if (!IsTransitionAllowed(Status, newStatus))
        {
            throw new InvalidOperationException(
                $"La transición de {Status} a {newStatus} no está permitida.");
        }

        Status = newStatus;
        AdvisorId = advisorId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Encodes the state machine described by US-016. Exposed as a static helper so callers can
    /// validate the transition without mutating the aggregate.
    /// </summary>
    public static bool IsTransitionAllowed(
        SupportRequestStatus current,
        SupportRequestStatus next) =>
        (current, next) switch
        {
            (SupportRequestStatus.Pending, SupportRequestStatus.UnderReview) => true,
            (SupportRequestStatus.UnderReview, SupportRequestStatus.Approved) => true,
            (SupportRequestStatus.UnderReview, SupportRequestStatus.Rejected) => true,
            _ => false,
        };
}
