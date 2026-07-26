using EduApoyos.Application.Features.Auth.Login;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EduApoyos.Tests.Features.Auth.Login;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Command_Is_Valid()
    {
        var command = new LoginCommand("juan.perez@example.com", "Password123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Should_Fail_When_Email_Is_Empty(string? email)
    {
        var command = new LoginCommand(email!, "Password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Should_Fail_When_Password_Is_Empty(string? password)
    {
        var command = new LoginCommand("juan.perez@example.com", password!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void Should_Accept_Any_Non_Empty_Email_String_To_Avoid_Leaking_Format_Validation()
    {
        // RN-004 requires a generic response, so the validator only checks presence, not shape.
        var command = new LoginCommand("not-an-email", "any-password");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Email);
    }
}
