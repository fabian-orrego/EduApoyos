using EduApoyos.Domain.Enums;
using FluentValidation;

namespace EduApoyos.Application.Features.Auth.Register;

/// <summary>
/// FluentValidation rules for <see cref="RegisterUserCommand"/>. Messages are user-facing
/// (Spanish) per project conventions.
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private const int FullNameMaxLength = 150;
    private const int PasswordMinLength = 8;

    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(FullNameMaxLength)
                .WithMessage($"El nombre no puede superar {FullNameMaxLength} caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(PasswordMinLength)
                .WithMessage($"La contraseña debe tener al menos {PasswordMinLength} caracteres.")
            .Matches("[A-Z]")
                .WithMessage("La contraseña debe incluir al menos una letra mayúscula.")
            .Matches("[a-z]")
                .WithMessage("La contraseña debe incluir al menos una letra minúscula.")
            .Matches("[0-9]")
                .WithMessage("La contraseña debe incluir al menos un número.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("La confirmación de contraseña es obligatoria.")
            .Equal(x => x.Password)
                .WithMessage("La confirmación no coincide con la contraseña.");

        RuleFor(x => x.Role)
            .Must(role => role == UserRole.Advisor || role == UserRole.Student)
                .WithMessage("El rol debe ser Advisor o Student.");
    }
}
