using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Application.Features.SupportRequests.Update;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.SupportRequests.Update;

public sealed class UpdateSupportRequestCommandHandlerTests
{
    private readonly Mock<ISupportRequestRepository> _repository =
        new(MockBehavior.Strict);

    private UpdateSupportRequestCommandHandler CreateSut() => new(_repository.Object);

    private static SupportRequest BuildRequest() =>
        new(
            studentId: Guid.NewGuid(),
            supportType: SupportType.Loan,
            requestedAmount: 100_000m,
            description: "Initial description");

    [Fact]
    public async Task Handle_Should_Update_Editable_Fields_And_Refresh_UpdatedAt()
    {
        var supportRequest = BuildRequest();
        var command = new UpdateSupportRequestCommand(
            supportRequest.Id,
            SupportType.Scholarship,
            750_000m,
            "  Nueva descripción actualizada.  ");

        var initialUpdatedAt = supportRequest.UpdatedAt;

        _repository
            .Setup(r => r.GetByIdAsync(supportRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportRequest);

        _repository
            .Setup(r => r.UpdateAsync(supportRequest, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        supportRequest.SupportType.Should().Be(SupportType.Scholarship);
        supportRequest.RequestedAmount.Should().Be(750_000m);
        supportRequest.Description.Should().Be("Nueva descripción actualizada.");
        supportRequest.UpdatedAt.Should().BeOnOrAfter(initialUpdatedAt);

        result.Value.SupportType.Should().Be((int)SupportType.Scholarship);
        result.Value.RequestedAmount.Should().Be(750_000m);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Request_Does_Not_Exist()
    {
        var command = new UpdateSupportRequestCommand(
            Guid.NewGuid(),
            SupportType.Scholarship,
            750_000m,
            "Descripción actualizada.");

        _repository
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportRequest?)null);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("supportRequests.notFound");

        _repository.Verify(
            r => r.UpdateAsync(
                It.IsAny<SupportRequest>(),
                It.IsAny<StatusHistory?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Request_Is_Approved()
    {
        var supportRequest = BuildRequest();
        var advisorId = Guid.NewGuid();
        supportRequest.ChangeStatus(SupportRequestStatus.UnderReview, advisorId);
        supportRequest.ChangeStatus(SupportRequestStatus.Approved, advisorId);

        var command = new UpdateSupportRequestCommand(
            supportRequest.Id,
            SupportType.Subsidy,
            999_999m,
            "No debería aplicarse.");

        _repository
            .Setup(r => r.GetByIdAsync(supportRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportRequest);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("supportRequests.update.finalized");
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Request_Is_Rejected()
    {
        var supportRequest = BuildRequest();
        var advisorId = Guid.NewGuid();
        supportRequest.ChangeStatus(SupportRequestStatus.UnderReview, advisorId);
        supportRequest.ChangeStatus(SupportRequestStatus.Rejected, advisorId);

        var command = new UpdateSupportRequestCommand(
            supportRequest.Id,
            SupportType.Scholarship,
            250_000m,
            "No debería aplicarse.");

        _repository
            .Setup(r => r.GetByIdAsync(supportRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportRequest);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }
}
