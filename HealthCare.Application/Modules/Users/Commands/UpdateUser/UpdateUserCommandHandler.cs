using FluentValidation;

namespace HealthCare.Application.Modules.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUser.UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("Username solo puede contener letras, números, '.', '_' y '-'.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(25)
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.Gender)
            .Must(g => g is null || g == 'M' || g == 'F' || g == 'O')
            .WithMessage("El género debe ser M, F u O.");
    }
}