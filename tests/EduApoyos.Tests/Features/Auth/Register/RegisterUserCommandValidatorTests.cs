using EduApoyos.Application.Features.Auth.Register;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EduApoyos.Tests.Features.Auth.Register;

public sealed class RegisterUserCommandValidatorTests
{
    private static RegisterUserCommand ValidCommand(Action<RegisterUserCommandBuilder>? mutate = null)
    {
        var builder = new RegisterUserCommandBuilder();
        mutate?.Invoke(builder);
        return builder.Build();
    }

    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Command_Is_Valid()
    {
        var command = ValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Fail_When_FullName_Is_Empty(string? fullName)
    {
        var command = ValidCommand(b => b.FullName = fullName!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FullName);
    }

    [Fact]
    public void Should_Fail_When_FullName_Exceeds_150_Characters()
    {
        var command = ValidCommand(b => b.FullName = new string('a', 151));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@missing-user.com")]
    public void Should_Fail_When_Email_Is_Invalid(string email)
    {
        var command = ValidCommand(b => b.Email = email);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Theory]
    [InlineData("short1A")]      // less than 8 chars
    [InlineData("nouppercase1")] // no uppercase
    [InlineData("NOLOWERCASE1")] // no lowercase
    [InlineData("NoDigitsHere")] // no digit
    public void Should_Fail_When_Password_Does_Not_Meet_Complexity(string password)
    {
        var command = ValidCommand(b =>
        {
            b.Password = password;
            b.ConfirmPassword = password;
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void Should_Fail_When_ConfirmPassword_Does_Not_Match()
    {
        var command = ValidCommand(b =>
        {
            b.Password = "Password123";
            b.ConfirmPassword = "Different123";
        });

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ConfirmPassword);
    }

    [Fact]
    public void Should_Fail_When_Role_Is_Not_Advisor_Or_Student()
    {
        var command = ValidCommand(b => b.Role = (UserRole)999);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Role);
    }

    [Theory]
    [InlineData(UserRole.Advisor)]
    [InlineData(UserRole.Student)]
    public void Should_Pass_When_Role_Is_Advisor_Or_Student(UserRole role)
    {
        var command = ValidCommand(b => b.Role = role);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Role);
        result.IsValid.Should().BeTrue();
    }

    private sealed class RegisterUserCommandBuilder
    {
        public string FullName { get; set; } = "Juan Pérez";
        public string Email { get; set; } = "juan.perez@example.com";
        public string Password { get; set; } = "Password123";
        public string ConfirmPassword { get; set; } = "Password123";
        public UserRole Role { get; set; } = UserRole.Student;

        public RegisterUserCommand Build() =>
            new(FullName, Email, Password, ConfirmPassword, Role);
    }
}
