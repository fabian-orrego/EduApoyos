using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Application.Features.SupportRequests.Create;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.SupportRequests.Create;

public sealed class CreateSupportRequestCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityService = new(MockBehavior.Strict);
    private readonly Mock<IStudentRepository> _studentRepository = new(MockBehavior.Strict);
    private readonly Mock<ISupportRequestRepository> _supportRequestRepository =
        new(MockBehavior.Strict);
    private readonly Mock<ICurrentUserService> _currentUser = new(MockBehavior.Loose);

    private CreateSupportRequestCommandHandler CreateSut() =>
        new(
            _identityService.Object,
            _studentRepository.Object,
            _supportRequestRepository.Object,
            _currentUser.Object);

    private static CreateSupportRequestCommand BuildCommand(
        string studentEmail = "juan.perez@example.com",
        SupportType supportType = SupportType.Scholarship,
        decimal requestedAmount = 500_000m,
        string description = "Solicito apoyo para el semestre.") =>
            new(studentEmail, supportType, requestedAmount, description);

    private static UserSummary BuildUser(
        Guid? id = null,
        UserRole role = UserRole.Student,
        string email = "juan.perez@example.com") =>
            new(id ?? Guid.NewGuid(), email, "Juan Pérez", role, DateTime.UtcNow);

    [Fact]
    public async Task Handle_Should_Create_Request_And_Initial_History_When_All_Rules_Pass()
    {
        var command = BuildCommand();
        var user = BuildUser();
        var studentId = Guid.NewGuid();
        var advisorId = Guid.NewGuid();

        _currentUser.SetupGet(c => c.UserId).Returns(advisorId);

        _identityService
            .Setup(s => s.FindByEmailAsync(command.StudentEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentId);

        SupportRequest? persistedRequest = null;
        StatusHistory? persistedHistory = null;
        _supportRequestRepository
            .Setup(r => r.CreateAsync(
                It.IsAny<SupportRequest>(),
                It.IsAny<StatusHistory>(),
                It.IsAny<CancellationToken>()))
            .Callback<SupportRequest, StatusHistory, CancellationToken>((sr, history, _) =>
            {
                persistedRequest = sr;
                persistedHistory = history;
            })
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        persistedRequest.Should().NotBeNull();
        persistedRequest!.StudentId.Should().Be(studentId);
        persistedRequest.Status.Should().Be(SupportRequestStatus.Pending);
        persistedRequest.SupportType.Should().Be(command.SupportType);
        persistedRequest.RequestedAmount.Should().Be(command.RequestedAmount);
        persistedRequest.Description.Should().Be(command.Description);
        persistedRequest.AdvisorId.Should().BeNull();
        persistedRequest.RequestedAt.Should()
            .BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        persistedRequest.UpdatedAt.Should().Be(persistedRequest.RequestedAt);

        persistedHistory.Should().NotBeNull();
        persistedHistory!.SupportRequestId.Should().Be(persistedRequest.Id);
        persistedHistory.PreviousStatus.Should().Be(SupportRequestStatus.Pending);
        persistedHistory.NewStatus.Should().Be(SupportRequestStatus.Pending);
        persistedHistory.ChangedByUserId.Should().Be(advisorId);
        persistedHistory.Notes.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_Should_Fallback_To_Student_UserId_When_No_Current_User()
    {
        var command = BuildCommand();
        var user = BuildUser();
        var studentId = Guid.NewGuid();

        _currentUser.SetupGet(c => c.UserId).Returns((Guid?)null);

        _identityService
            .Setup(s => s.FindByEmailAsync(command.StudentEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentId);

        StatusHistory? capturedHistory = null;
        _supportRequestRepository
            .Setup(r => r.CreateAsync(
                It.IsAny<SupportRequest>(),
                It.IsAny<StatusHistory>(),
                It.IsAny<CancellationToken>()))
            .Callback<SupportRequest, StatusHistory, CancellationToken>((_, h, _) =>
                capturedHistory = h)
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        capturedHistory.Should().NotBeNull();
        capturedHistory!.ChangedByUserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_Should_Trim_StudentEmail_And_Description()
    {
        var command = BuildCommand(
            studentEmail: "  juan.perez@example.com  ",
            description: "  Solicito apoyo para el semestre.  ");
        var user = BuildUser();

        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());

        _identityService
            .Setup(s => s.FindByEmailAsync("juan.perez@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        SupportRequest? captured = null;
        _supportRequestRepository
            .Setup(r => r.CreateAsync(
                It.IsAny<SupportRequest>(),
                It.IsAny<StatusHistory>(),
                It.IsAny<CancellationToken>()))
            .Callback<SupportRequest, StatusHistory, CancellationToken>((sr, _, _) =>
                captured = sr)
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured!.Description.Should().Be("Solicito apoyo para el semestre.");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_User_Is_Missing()
    {
        var command = BuildCommand();

        _identityService
            .Setup(s => s.FindByEmailAsync(command.StudentEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSummary?)null);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("supportRequests.student.notFound");

        _supportRequestRepository.Verify(
            r => r.CreateAsync(
                It.IsAny<SupportRequest>(),
                It.IsAny<StatusHistory>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_User_Is_Not_A_Student()
    {
        var command = BuildCommand();

        _identityService
            .Setup(s => s.FindByEmailAsync(command.StudentEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser(role: UserRole.Advisor));

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("supportRequests.student.notFound");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_User_Has_No_Linked_Student()
    {
        var command = BuildCommand();
        var user = BuildUser();

        _identityService
            .Setup(s => s.FindByEmailAsync(command.StudentEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("supportRequests.student.notFound");
    }

    [Fact]
    public async Task Handle_Should_Propagate_Cancellation_Token()
    {
        var command = BuildCommand();
        var user = BuildUser();
        var studentId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());

        _identityService
            .Setup(s => s.FindByEmailAsync(command.StudentEmail, cts.Token))
            .ReturnsAsync(user);

        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(user.Id, cts.Token))
            .ReturnsAsync(studentId);

        _supportRequestRepository
            .Setup(r => r.CreateAsync(
                It.IsAny<SupportRequest>(),
                It.IsAny<StatusHistory>(),
                cts.Token))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, cts.Token);

        result.IsSuccess.Should().BeTrue();
        _supportRequestRepository.VerifyAll();
    }
}
