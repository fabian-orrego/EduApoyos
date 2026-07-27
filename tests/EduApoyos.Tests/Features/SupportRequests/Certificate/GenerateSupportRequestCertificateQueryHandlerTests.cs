using EduApoyos.Application.Common.Documents;
using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Application.Features.SupportRequests.Certificate;
using EduApoyos.Application.Features.SupportRequests.Detail;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.SupportRequests.Certificate;

public sealed class GenerateSupportRequestCertificateQueryHandlerTests
{
    private readonly Mock<ISupportRequestRepository> _supportRequestRepository =
        new(MockBehavior.Strict);
    private readonly Mock<IStudentRepository> _studentRepository = new(MockBehavior.Loose);
    private readonly Mock<ICurrentUserService> _currentUser = new(MockBehavior.Loose);
    private readonly Mock<ISupportRequestPdfGenerator> _pdfGenerator = new(MockBehavior.Loose);

    private GenerateSupportRequestCertificateQueryHandler CreateSut() =>
        new(
            _supportRequestRepository.Object,
            _studentRepository.Object,
            _currentUser.Object,
            _pdfGenerator.Object);

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
    public async Task Handle_Should_Return_Pdf_When_Owner_Requests_Certificate()
    {
        var studentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var detail = BuildDetail(studentId);
        var expected = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Student);
        _currentUser.SetupGet(c => c.UserId).Returns(userId);

        _supportRequestRepository
            .Setup(r => r.GetDetailAsync(detail.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentId);

        _pdfGenerator
            .Setup(p => p.Generate(detail, It.IsAny<DateTime>()))
            .Returns(expected);

        var result = await CreateSut()
            .Handle(new GenerateSupportRequestCertificateQuery(detail.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().BeEquivalentTo(expected);
        result.Value.ContentType.Should().Be("application/pdf");
        result.Value.FileName.Should().MatchRegex(
            $"^constancia-solicitud-{detail.Id:N}-[a-f0-9]{{8}}\\.pdf$");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Request_Does_Not_Exist()
    {
        var query = new GenerateSupportRequestCertificateQuery(Guid.NewGuid());

        _supportRequestRepository
            .Setup(r => r.GetDetailAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportRequestDetail?)null);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Caller_Is_Not_A_Student()
    {
        var detail = BuildDetail();

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Advisor);
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());

        _supportRequestRepository
            .Setup(r => r.GetDetailAsync(detail.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        var result = await CreateSut()
            .Handle(new GenerateSupportRequestCertificateQuery(detail.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Student_Does_Not_Own_Request()
    {
        var detail = BuildDetail();
        var userId = Guid.NewGuid();

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Student);
        _currentUser.SetupGet(c => c.UserId).Returns(userId);

        _supportRequestRepository
            .Setup(r => r.GetDetailAsync(detail.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var result = await CreateSut()
            .Handle(new GenerateSupportRequestCertificateQuery(detail.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }
}
