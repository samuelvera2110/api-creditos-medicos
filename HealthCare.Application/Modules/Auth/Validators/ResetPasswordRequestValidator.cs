using FluentValidation;
using HealthCare.Application.Modules.Auth.DTOs.Requests;

namespace HealthCare.Application.Modules.Auth.Validators;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.TargetUserId)
            .GreaterThan(0).WithMessage("El ID de usuario es requerido.");
    }
}
