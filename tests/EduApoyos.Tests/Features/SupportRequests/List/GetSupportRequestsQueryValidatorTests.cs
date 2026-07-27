using EduApoyos.Application.Features.SupportRequests.List;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EduApoyos.Tests.Features.SupportRequests.List;

public sealed class GetSupportRequestsQueryValidatorTests
{
    private readonly GetSupportRequestsQueryValidator _validator = new();

    private static GetSupportRequestsQuery Valid(
        int pageNumber = 1,
        int pageSize = 10,
        SupportRequestStatus? status = null,
        SupportType? supportType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null) =>
            new(pageNumber, pageSize, status, supportType, fromDate, toDate);

    [Fact]
    public void Should_Pass_When_Query_Is_Valid()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Fail_When_PageNumber_Is_Not_Positive(int pageNumber)
    {
        var result = _validator.TestValidate(Valid(pageNumber: pageNumber));
        result.ShouldHaveValidationErrorFor(q => q.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Fail_When_PageSize_Is_Not_Positive(int pageSize)
    {
        var result = _validator.TestValidate(Valid(pageSize: pageSize));
        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Should_Fail_When_PageSize_Exceeds_100()
    {
        var result = _validator.TestValidate(Valid(pageSize: 101));
        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Should_Fail_When_Status_Is_Not_A_Valid_Enum_Value()
    {
        var result = _validator.TestValidate(
            Valid(status: (SupportRequestStatus)999));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Fail_When_SupportType_Is_Not_A_Valid_Enum_Value()
    {
        var result = _validator.TestValidate(
            Valid(supportType: (SupportType)999));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Fail_When_FromDate_Is_After_ToDate()
    {
        var result = _validator.TestValidate(Valid(
            fromDate: new DateTime(2026, 5, 10),
            toDate: new DateTime(2026, 5, 1)));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Pass_When_FromDate_Equals_ToDate()
    {
        var result = _validator.TestValidate(Valid(
            fromDate: new DateTime(2026, 5, 10),
            toDate: new DateTime(2026, 5, 10)));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_Only_FromDate_Is_Provided()
    {
        var result = _validator.TestValidate(Valid(
            fromDate: new DateTime(2026, 5, 10)));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
