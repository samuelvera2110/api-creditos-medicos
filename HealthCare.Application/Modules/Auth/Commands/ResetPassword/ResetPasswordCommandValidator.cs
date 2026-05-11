using FluentValidation;

namespace HealthCare.Application.Modules.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator 
    : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.TargetUserId)
            .GreaterThan(0)
            .WithMessage("El usuario destino es requerido.");

        RuleFor(x => x.RequestedBy)
            .GreaterThan(0)
            .WithMessage("El usuario solicitante es requerido.");
    }
}