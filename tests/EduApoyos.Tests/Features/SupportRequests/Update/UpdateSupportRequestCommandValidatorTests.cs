using EduApoyos.Application.Features.SupportRequests.Update;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EduApoyos.Tests.Features.SupportRequests.Update;

public sealed class UpdateSupportRequestCommandValidatorTests
{
    private readonly UpdateSupportRequestCommandValidator _validator = new();

    private static UpdateSupportRequestCommand Valid(
        Guid? id = null,
        SupportType supportType = SupportType.Loan,
        decimal requestedAmount = 300_000m,
        string description = "Descripción válida.") =>
            new(id ?? Guid.NewGuid(), supportType, requestedAmount, description);

    [Fact]
    public void Should_Pass_When_Command_Is_Valid()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Id_Is_Empty()
    {
        var result = _validator.TestValidate(Valid(id: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Should_Fail_When_SupportType_Is_Not_A_Valid_Enum_Value()
    {
        var result = _validator.TestValidate(Valid(supportType: (SupportType)999));
        result.ShouldHaveValidationErrorFor(c => c.SupportType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Fail_When_RequestedAmount_Is_Not_Positive(decimal amount)
    {
        var result = _validator.TestValidate(Valid(requestedAmount: amount));
        result.ShouldHaveValidationErrorFor(c => c.RequestedAmount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Fail_When_Description_Is_Empty(string? description)
    {
        var result = _validator.TestValidate(Valid(description: description!));
        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_1000_Characters()
    {
        var result = _validator.TestValidate(Valid(description: new string('x', 1001)));
        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public void Should_Pass_When_Description_Has_Exactly_1000_Characters()
    {
        var result = _validator.TestValidate(Valid(description: new string('x', 1000)));
        result.ShouldNotHaveValidationErrorFor(c => c.Description);
        result.IsValid.Should().BeTrue();
    }
}
