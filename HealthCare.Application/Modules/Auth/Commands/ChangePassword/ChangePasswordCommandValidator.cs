using FluentValidation;

namespace HealthCare.Application.Modules.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator 
    : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("La contraseña actual es requerida.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("La nueva contraseña no puede ser igual a la actual.")
            .Matches(@"[A-Z]")
            .WithMessage("Debe contener al menos una letra mayúscula.")
            .Matches(@"[0-9]")
            .WithMessage("Debe contener al menos un número.")
            .Matches(@"[^a-zA-Z0-9]")
            .WithMessage("Debe contener al menos un carácter especial.");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Las contraseñas no coinciden.");
    }
}