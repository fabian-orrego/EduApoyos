using FluentValidation;

namespace EduApoyos.Application.Features.Auth.Login;

/// <summary>
/// FluentValidation rules for <see cref="LoginCommand"/> (US-005). The rules deliberately only
/// check that the fields are present: any credential mismatch is surfaced later as a generic
/// unauthorized error to comply with RN-004 (do not disclose whether the email or the password
/// was wrong).
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}
