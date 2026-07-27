using EduApoyos.Application.Features.Students.List;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EduApoyos.Tests.Features.Students.List;

public sealed class GetStudentsQueryValidatorTests
{
    private readonly GetStudentsQueryValidator _validator = new();

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 10)]
    [InlineData(50, 100)]
    public void Should_Pass_When_Parameters_Are_Within_Bounds(int pageNumber, int pageSize)
    {
        var query = new GetStudentsQuery(pageNumber, pageSize);

        var result = _validator.TestValidate(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Fail_When_PageNumber_Is_Less_Than_1(int pageNumber)
    {
        var query = new GetStudentsQuery(pageNumber, 10);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_Fail_When_PageSize_Is_Less_Than_1(int pageSize)
    {
        var query = new GetStudentsQuery(1, pageSize);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Should_Fail_When_PageSize_Exceeds_100()
    {
        var query = new GetStudentsQuery(1, GetStudentsQueryValidator.MaxPageSize + 1);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }
}
