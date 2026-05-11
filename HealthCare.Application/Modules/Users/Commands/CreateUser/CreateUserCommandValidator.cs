using FluentValidation;

namespace HealthCare.Application.Modules.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.DocumentTypeId)
            .GreaterThan(0)
            .WithMessage("El tipo de documento es requerido.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty()
            .MaximumLength(20);

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