using EduApoyos.Application.Features.SupportRequests.Create;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EduApoyos.Tests.Features.SupportRequests.Create;

public sealed class CreateSupportRequestCommandValidatorTests
{
    private readonly CreateSupportRequestCommandValidator _validator = new();

    private static CreateSupportRequestCommand ValidCommand(
        Action<Builder>? mutate = null)
    {
        var builder = new Builder();
        mutate?.Invoke(builder);
        return builder.Build();
    }

    [Fact]
    public void Should_Pass_When_Command_Is_Valid()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Fail_When_StudentEmail_Is_Empty(string? email)
    {
        var command = ValidCommand(b => b.StudentEmail = email!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.StudentEmail);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@missing-user.com")]
    public void Should_Fail_When_StudentEmail_Has_Invalid_Format(string email)
    {
        var command = ValidCommand(b => b.StudentEmail = email);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.StudentEmail);
    }

    [Fact]
    public void Should_Fail_When_SupportType_Is_Not_A_Valid_Enum_Value()
    {
        var command = ValidCommand(b => b.SupportType = (SupportType)999);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.SupportType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_Fail_When_RequestedAmount_Is_Not_Positive(decimal amount)
    {
        var command = ValidCommand(b => b.RequestedAmount = amount);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.RequestedAmount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Fail_When_Description_Is_Empty(string? description)
    {
        var command = ValidCommand(b => b.Description = description!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_1000_Characters()
    {
        var command = ValidCommand(b => b.Description = new string('x', 1001));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public void Should_Pass_When_Description_Has_Exactly_1000_Characters()
    {
        var command = ValidCommand(b => b.Description = new string('x', 1000));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Description);
        result.IsValid.Should().BeTrue();
    }

    private sealed class Builder
    {
        public string StudentEmail { get; set; } = "juan.perez@example.com";
        public SupportType SupportType { get; set; } = SupportType.Scholarship;
        public decimal RequestedAmount { get; set; } = 500_000m;
        public string Description { get; set; } = "Solicito apoyo para el semestre.";

        public CreateSupportRequestCommand Build() =>
            new(StudentEmail, SupportType, RequestedAmount, Description);
    }
}
