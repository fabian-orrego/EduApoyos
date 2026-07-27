using EduApoyos.Application.Features.SupportRequests.ChangeStatus;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EduApoyos.Tests.Features.SupportRequests.ChangeStatus;

public sealed class ChangeSupportRequestStatusCommandValidatorTests
{
    private readonly ChangeSupportRequestStatusCommandValidator _validator = new();

    private static ChangeSupportRequestStatusCommand Valid(
        Guid? id = null,
        SupportRequestStatus newStatus = SupportRequestStatus.UnderReview,
        string? notes = null) =>
            new(id ?? Guid.NewGuid(), newStatus, notes);

    [Fact]
    public void Should_Pass_For_UnderReview_Without_Notes()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_For_Approved_Without_Notes()
    {
        var result = _validator.TestValidate(
            Valid(newStatus: SupportRequestStatus.Approved));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_For_Rejected_With_Notes()
    {
        var result = _validator.TestValidate(Valid(
            newStatus: SupportRequestStatus.Rejected,
            notes: "Motivo del rechazo."));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Id_Is_Empty()
    {
        var result = _validator.TestValidate(Valid(id: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Should_Fail_When_NewStatus_Is_Not_A_Valid_Enum_Value()
    {
        var result = _validator.TestValidate(
            Valid(newStatus: (SupportRequestStatus)999));

        result.ShouldHaveValidationErrorFor(c => c.NewStatus);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_When_Notes_Are_Missing_On_Rejection(string? notes)
    {
        var result = _validator.TestValidate(Valid(
            newStatus: SupportRequestStatus.Rejected,
            notes: notes));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Fail_When_Notes_Exceed_500_Characters()
    {
        var result = _validator.TestValidate(Valid(
            newStatus: SupportRequestStatus.Rejected,
            notes: new string('x', 501)));

        result.IsValid.Should().BeFalse();
    }
}
