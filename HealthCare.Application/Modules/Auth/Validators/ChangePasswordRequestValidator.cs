using FluentValidation;
using HealthCare.Application.Modules.Auth.DTOs.Requests;

namespace HealthCare.Application.Modules.Auth.Validators;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("La contraseña actual es requerida.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("La nueva contraseña es requerida.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(128)
            .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número.")
            .Matches(@"[!@#$%^&*]").WithMessage("La contraseña debe contener al menos un carácter especial.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("La confirmación de contraseña es requerida.")
            .Equal(x => x.NewPassword).WithMessage("Las contraseñas no coinciden.");
    }
}
