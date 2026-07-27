using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Application.Features.SupportRequests.ChangeStatus;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.SupportRequests.ChangeStatus;

public sealed class ChangeSupportRequestStatusCommandHandlerTests
{
    private readonly Mock<ISupportRequestRepository> _repository =
        new(MockBehavior.Strict);
    private readonly Mock<ICurrentUserService> _currentUser = new(MockBehavior.Strict);

    private ChangeSupportRequestStatusCommandHandler CreateSut() =>
        new(_repository.Object, _currentUser.Object);

    private static SupportRequest BuildRequest() =>
        new(
            studentId: Guid.NewGuid(),
            supportType: SupportType.Loan,
            requestedAmount: 100_000m,
            description: "Descripción.");

    private void SetupAdvisor(Guid advisorId)
    {
        _currentUser.SetupGet(c => c.UserId).Returns(advisorId);
        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Advisor);
    }

    [Fact]
    public async Task Handle_Should_Move_Pending_To_UnderReview_And_Persist_History()
    {
        var supportRequest = BuildRequest();
        var advisorId = Guid.NewGuid();
        SetupAdvisor(advisorId);

        var command = new ChangeSupportRequestStatusCommand(
            supportRequest.Id,
            SupportRequestStatus.UnderReview,
            Notes: "Iniciando revisión.");

        _repository
            .Setup(r => r.GetByIdAsync(supportRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportRequest);

        StatusHistory? persistedHistory = null;
        _repository
            .Setup(r => r.UpdateAsync(
                supportRequest,
                It.IsAny<StatusHistory?>(),
                It.IsAny<CancellationToken>()))
            .Callback<SupportRequest, StatusHistory?, CancellationToken>((_, h, _) =>
                persistedHistory = h)
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        supportRequest.Status.Should().Be(SupportRequestStatus.UnderReview);
        supportRequest.AdvisorId.Should().Be(advisorId);

        persistedHistory.Should().NotBeNull();
        persistedHistory!.PreviousStatus.Should().Be(SupportRequestStatus.Pending);
        persistedHistory.NewStatus.Should().Be(SupportRequestStatus.UnderReview);
        persistedHistory.ChangedByUserId.Should().Be(advisorId);
        persistedHistory.Notes.Should().Be("Iniciando revisión.");

        result.Value.PreviousStatus.Should().Be((int)SupportRequestStatus.Pending);
        result.Value.NewStatus.Should().Be((int)SupportRequestStatus.UnderReview);
    }

    [Fact]
    public async Task Handle_Should_Move_UnderReview_To_Approved()
    {
        var supportRequest = BuildRequest();
        var advisorId = Guid.NewGuid();
        supportRequest.ChangeStatus(SupportRequestStatus.UnderReview, advisorId);

        SetupAdvisor(advisorId);

        var command = new ChangeSupportRequestStatusCommand(
            supportRequest.Id,
            SupportRequestStatus.Approved,
            Notes: "Aprobado.");

        _repository
            .Setup(r => r.GetByIdAsync(supportRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportRequest);

        _repository
            .Setup(r => r.UpdateAsync(
                supportRequest,
                It.IsAny<StatusHistory?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        supportRequest.Status.Should().Be(SupportRequestStatus.Approved);
    }

    [Fact]
    public async Task Handle_Should_Move_UnderReview_To_Rejected()
    {
        var supportRequest = BuildRequest();
        var advisorId = Guid.NewGuid();
        supportRequest.ChangeStatus(SupportRequestStatus.UnderReview, advisorId);

        SetupAdvisor(advisorId);

        var command = new ChangeSupportRequestStatusCommand(
            supportRequest.Id,
            SupportRequestStatus.Rejected,
            Notes: "Motivo de rechazo.");

        _repository
            .Setup(r => r.GetByIdAsync(supportRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportRequest);

        _repository
            .Setup(r => r.UpdateAsync(
                supportRequest,
                It.IsAny<StatusHistory?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        supportRequest.Status.Should().Be(SupportRequestStatus.Rejected);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Caller_Is_Not_Advisor()
    {
        var command = new ChangeSupportRequestStatusCommand(
            Guid.NewGuid(),
            SupportRequestStatus.UnderReview,
            null);

        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Student);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Request_Does_Not_Exist()
    {
        var advisorId = Guid.NewGuid();
        SetupAdvisor(advisorId);

        var command = new ChangeSupportRequestStatusCommand(
            Guid.NewGuid(),
            SupportRequestStatus.UnderReview,
            null);

        _repository
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportRequest?)null);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Request_Is_Already_Finalized()
    {
        var supportRequest = BuildRequest();
        var advisorId = Guid.NewGuid();
        supportRequest.ChangeStatus(SupportRequestStatus.UnderReview, advisorId);
        supportRequest.ChangeStatus(SupportRequestStatus.Approved, advisorId);

        SetupAdvisor(advisorId);

        var command = new ChangeSupportRequestStatusCommand(
            supportRequest.Id,
            SupportRequestStatus.Rejected,
            null);

        _repository
            .Setup(r => r.GetByIdAsync(supportRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportRequest);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("supportRequests.status.finalized");
    }

    [Theory]
    [InlineData(SupportRequestStatus.Pending, SupportRequestStatus.Approved)]
    [InlineData(SupportRequestStatus.Pending, SupportRequestStatus.Rejected)]
    [InlineData(SupportRequestStatus.Pending, SupportRequestStatus.Pending)]
    public async Task Handle_Should_Return_Conflict_When_Transition_Is_Not_Allowed(
        SupportRequestStatus current,
        SupportRequestStatus target)
    {
        var supportRequest = BuildRequest();
        var advisorId = Guid.NewGuid();

        if (current == SupportRequestStatus.UnderReview)
        {
            supportRequest.ChangeStatus(SupportRequestStatus.UnderReview, advisorId);
        }

        SetupAdvisor(advisorId);

        var command = new ChangeSupportRequestStatusCommand(
            supportRequest.Id,
            target,
            "Rechazo.");

        _repository
            .Setup(r => r.GetByIdAsync(supportRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportRequest);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("supportRequests.status.invalidTransition");
    }
}
