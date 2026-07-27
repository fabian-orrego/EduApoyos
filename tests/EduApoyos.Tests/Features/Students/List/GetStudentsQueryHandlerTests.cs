using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Features.Students.List;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.Students.List;

public sealed class GetStudentsQueryHandlerTests
{
    private readonly Mock<IStudentRepository> _studentRepository = new(MockBehavior.Strict);

    private GetStudentsQueryHandler CreateSut() => new(_studentRepository.Object);

    [Fact]
    public async Task Handle_Should_Return_Paged_Result_From_Repository()
    {
        var query = new GetStudentsQuery(2, 5);
        var page = PagedResult<StudentListItem>.Create(
            new[]
            {
                new StudentListItem(
                    Guid.NewGuid(),
                    "Juan Pérez",
                    "1234567890",
                    1,
                    "Ingeniería de Software",
                    4,
                    "juan.perez@example.com"),
            },
            page: 2,
            pageSize: 5,
            totalItems: 6);

        _studentRepository
            .Setup(r => r.GetPagedAsync(query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(page);
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.TotalItems.Should().Be(6);
        result.Value.TotalPages.Should().Be(2);
        result.Value.Items.Should().HaveCount(1);

        _studentRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Page_When_No_Records_Exist()
    {
        var query = new GetStudentsQuery(1, 10);
        var empty = PagedResult<StudentListItem>.Empty(query.PageNumber, query.PageSize);

        _studentRepository
            .Setup(r => r.GetPagedAsync(query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empty);

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Propagate_Cancellation_Token()
    {
        var query = new GetStudentsQuery(1, 10);
        using var cts = new CancellationTokenSource();
        var page = PagedResult<StudentListItem>.Empty(1, 10);

        _studentRepository
            .Setup(r => r.GetPagedAsync(query.PageNumber, query.PageSize, cts.Token))
            .ReturnsAsync(page);

        var result = await CreateSut().Handle(query, cts.Token);

        result.IsSuccess.Should().BeTrue();
        _studentRepository.VerifyAll();
    }
}
