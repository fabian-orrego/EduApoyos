using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using FluentAssertions;

namespace EduApoyos.Tests.Domain;

public sealed class SupportRequestTests
{
    private static SupportRequest Build() =>
        new(
            studentId: Guid.NewGuid(),
            supportType: SupportType.Loan,
            requestedAmount: 100_000m,
            description: "Descripción.");

    [Fact]
    public void Constructor_Should_Initialize_Aggregate_With_Pending_Status()
    {
        var request = Build();

        request.Status.Should().Be(SupportRequestStatus.Pending);
        request.AdvisorId.Should().BeNull();
        request.RequestedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        request.UpdatedAt.Should().Be(request.RequestedAt);
        request.IsFinalized.Should().BeFalse();
    }

    [Fact]
    public void UpdateDetails_Should_Update_Fields_When_Not_Finalized()
    {
        var request = Build();

        request.UpdateDetails(SupportType.Subsidy, 250_000m, "Nueva descripción");

        request.SupportType.Should().Be(SupportType.Subsidy);
        request.RequestedAmount.Should().Be(250_000m);
        request.Description.Should().Be("Nueva descripción");
    }

    [Fact]
    public void UpdateDetails_Should_Throw_When_Request_Is_Approved()
    {
        var request = Build();
        var advisorId = Guid.NewGuid();
        request.ChangeStatus(SupportRequestStatus.UnderReview, advisorId);
        request.ChangeStatus(SupportRequestStatus.Approved, advisorId);

        var act = () => request.UpdateDetails(
            SupportType.Subsidy,
            250_000m,
            "Nueva descripción");

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(SupportRequestStatus.Pending, SupportRequestStatus.UnderReview, true)]
    [InlineData(SupportRequestStatus.UnderReview, SupportRequestStatus.Approved, true)]
    [InlineData(SupportRequestStatus.UnderReview, SupportRequestStatus.Rejected, true)]
    [InlineData(SupportRequestStatus.Pending, SupportRequestStatus.Approved, false)]
    [InlineData(SupportRequestStatus.Pending, SupportRequestStatus.Rejected, false)]
    [InlineData(SupportRequestStatus.Approved, SupportRequestStatus.UnderReview, false)]
    [InlineData(SupportRequestStatus.Rejected, SupportRequestStatus.UnderReview, false)]
    [InlineData(SupportRequestStatus.Pending, SupportRequestStatus.Pending, false)]
    public void IsTransitionAllowed_Should_Follow_State_Machine(
        SupportRequestStatus current,
        SupportRequestStatus target,
        bool expected)
    {
        SupportRequest.IsTransitionAllowed(current, target).Should().Be(expected);
    }

    [Fact]
    public void ChangeStatus_Should_Record_Advisor_And_UpdatedAt()
    {
        var request = Build();
        var advisorId = Guid.NewGuid();

        request.ChangeStatus(SupportRequestStatus.UnderReview, advisorId);

        request.Status.Should().Be(SupportRequestStatus.UnderReview);
        request.AdvisorId.Should().Be(advisorId);
        request.UpdatedAt.Should().BeOnOrAfter(request.RequestedAt);
    }

    [Fact]
    public void ChangeStatus_Should_Throw_On_Invalid_Transition()
    {
        var request = Build();

        var act = () => request.ChangeStatus(SupportRequestStatus.Approved, Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }
}
