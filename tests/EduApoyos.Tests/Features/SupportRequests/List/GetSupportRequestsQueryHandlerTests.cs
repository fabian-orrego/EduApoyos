using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Features.SupportRequests.List;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.SupportRequests.List;

public sealed class GetSupportRequestsQueryHandlerTests
{
    private readonly Mock<ISupportRequestRepository> _repository =
        new(MockBehavior.Strict);
    private readonly Mock<IStudentRepository> _studentRepository =
        new(MockBehavior.Strict);
    private readonly Mock<ICurrentUserService> _currentUser =
        new(MockBehavior.Strict);

    private GetSupportRequestsQueryHandler CreateSut() =>
        new(_repository.Object, _studentRepository.Object, _currentUser.Object);

    [Fact]
    public async Task Handle_As_Advisor_Should_Return_Paged_Result_Without_Student_Scope()
    {
        var query = new GetSupportRequestsQuery(1, 10, null, null, null, null);
        var page = PagedResult<SupportRequestListItem>.Create(
            new[]
            {
                new SupportRequestListItem(
                    Guid.NewGuid(),
                    "Juan Pérez",
                    "1234567890",
                    (int)SupportType.Scholarship,
                    (int)SupportRequestStatus.Pending,
                    500_000m,
                    DateTime.UtcNow),
            },
            page: 1,
            pageSize: 10,
            totalItems: 1);

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Advisor);

        _repository
            .Setup(r => r.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.Status,
                query.SupportType,
                query.FromDate,
                query.ToDate,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(page);
    }

    [Fact]
    public async Task Handle_As_Advisor_Should_Return_Empty_Page_When_No_Results_Match_Filters()
    {
        var query = new GetSupportRequestsQuery(
            1,
            10,
            SupportRequestStatus.Approved,
            SupportType.Loan,
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31));

        var empty = PagedResult<SupportRequestListItem>.Empty(1, 10);

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Advisor);

        _repository
            .Setup(r => r.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.Status,
                query.SupportType,
                query.FromDate,
                query.ToDate,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(empty);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task Handle_As_Advisor_Should_Forward_All_Filters_To_Repository()
    {
        var query = new GetSupportRequestsQuery(
            2,
            25,
            SupportRequestStatus.UnderReview,
            SupportType.Subsidy,
            new DateTime(2026, 3, 1),
            new DateTime(2026, 3, 31));

        var page = PagedResult<SupportRequestListItem>.Empty(2, 25);

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Advisor);

        _repository
            .Setup(r => r.GetPagedAsync(
                2,
                25,
                SupportRequestStatus.UnderReview,
                SupportType.Subsidy,
                new DateTime(2026, 3, 1),
                new DateTime(2026, 3, 31),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repository.VerifyAll();
    }

    [Fact]
    public async Task Handle_As_Student_Should_Scope_Results_To_Own_Student_Id()
    {
        var userId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var query = new GetSupportRequestsQuery(1, 10, null, null, null, null);
        var page = PagedResult<SupportRequestListItem>.Create(
            new[]
            {
                new SupportRequestListItem(
                    Guid.NewGuid(),
                    "Ana López",
                    "9876543210",
                    (int)SupportType.Loan,
                    (int)SupportRequestStatus.UnderReview,
                    1_000_000m,
                    DateTime.UtcNow),
            },
            page: 1,
            pageSize: 10,
            totalItems: 1);

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Student);
        _currentUser.SetupGet(c => c.UserId).Returns(userId);
        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentId);
        _repository
            .Setup(r => r.GetPagedAsync(
                1,
                10,
                null,
                null,
                null,
                null,
                studentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(page);
        _repository.VerifyAll();
    }

    [Fact]
    public async Task Handle_As_Student_Without_Profile_Should_Return_Empty_Page()
    {
        var userId = Guid.NewGuid();
        var query = new GetSupportRequestsQuery(1, 10, null, null, null, null);

        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Student);
        _currentUser.SetupGet(c => c.UserId).Returns(userId);
        _studentRepository
            .Setup(r => r.GetIdByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        _repository.Verify(
            r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<SupportRequestStatus?>(),
                It.IsAny<SupportType?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Without_Recognized_Role_Should_Return_Forbidden()
    {
        var query = new GetSupportRequestsQuery(1, 10, null, null, null, null);

        _currentUser.SetupGet(c => c.Role).Returns((UserRole?)null);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("supportRequests.list.forbidden");
    }
}
