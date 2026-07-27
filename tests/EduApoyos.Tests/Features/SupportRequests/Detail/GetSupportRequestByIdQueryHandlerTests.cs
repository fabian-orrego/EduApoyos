using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Application.Features.SupportRequests.Detail;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.SupportRequests.Detail;

public sealed class GetSupportRequestByIdQueryHandlerTests
{
    private readonly Mock<ISupportRequestRepository> _supportRequestRepository =
        new(MockBehavior.Strict);
    private readonly Mock<IStudentRepository> _studentRepository = new(MockBehavior.Loose);
    private readonly Mock<ICurrentUserService> _currentUser = new(MockBehavior.Loose);

    private GetSupportRequestByIdQueryHandler CreateSut() =>
        new(_supportRequestRepository.Object, _studentRepository.Object, _currentUser.Object);

    private static SupportRequestDetail BuildDetail(Guid? studentId = null) =>
        new(
            Id: Guid.NewGuid(),
            StudentId: studentId ?? Guid.NewGuid(),
            StudentFullName: "Juan Pérez",
            StudentEmail: "juan.perez@example.com",
            StudentDocumentNumber: "1234567890",
            StudentDocumentType: 1,
            StudentAcademicProgram: "Ingeniería de Software",
            StudentSemester: 4,
            SupportType: (int)SupportType.Scholarship,
            RequestedAmount: 500_000m,
            Description: "Solicito apoyo.",
            Status: (int)SupportRequestStatus.Pending,
            RequestedAt: DateTime.UtcNow.AddDays(-2),
            UpdatedAt: DateTime.UtcNow.AddDays(-1),
            AdvisorId: null,
            AdvisorFullName: null,
            History: Array.Empty<SupportRequestHistoryItem>());

    [Fact]
    public async Task Handle_Should_Return_Detail_When_Caller_Is_Advisor()
    {
        var detail = BuildDetail();
        var query = new GetSupportRequestByIdQuery(detail.Id);

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Advisor);
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());

        _supportRequestRepository
            .Setup(r => r.GetDetailAsync(detail.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(detail);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Request_Does_Not_Exist()
    {
        var query = new GetSupportRequestByIdQuery(Guid.NewGuid());

        _supportRequestRepository
            .Setup(r => r.GetDetailAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportRequestDetail?)null);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("supportRequests.notFound");
    }

    [Fact]
    public async Task Handle_Should_Return_Detail_When_Student_Owns_The_Request()
    {
        var studentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var detail = BuildDetail(studentId);
        var query = new GetSupportRequestByIdQuery(detail.Id);

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Student);
        _currentUser.SetupGet(c => c.UserId).Returns(userId);

        _supportRequestRepository
            .Setup(r => r.GetDetailAsync(detail.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentId);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(detail);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Student_Requests_Foreign_Record()
    {
        var detail = BuildDetail();
        var query = new GetSupportRequestByIdQuery(detail.Id);
        var userId = Guid.NewGuid();

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Student);
        _currentUser.SetupGet(c => c.UserId).Returns(userId);

        _supportRequestRepository
            .Setup(r => r.GetDetailAsync(detail.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
        result.Error.Code.Should().Be("supportRequests.forbidden");
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Student_Has_No_Linked_Student_Record()
    {
        var detail = BuildDetail();
        var query = new GetSupportRequestByIdQuery(detail.Id);
        var userId = Guid.NewGuid();

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Student);
        _currentUser.SetupGet(c => c.UserId).Returns(userId);

        _supportRequestRepository
            .Setup(r => r.GetDetailAsync(detail.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }
}
