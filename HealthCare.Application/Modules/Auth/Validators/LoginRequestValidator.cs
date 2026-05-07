using FluentValidation;
using HealthCare.Application.Modules.Auth.DTOs;

namespace HealthCare.Application.Modules.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es requerido.")
            .MaximumLength(50).WithMessage("El nombre de usuario no puede superar 50 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(128).WithMessage("La contraseña no puede superar 128 caracteres.");
    }
}
