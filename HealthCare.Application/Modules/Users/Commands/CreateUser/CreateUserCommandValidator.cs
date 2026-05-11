using FluentValidation;

namespace HealthCare.Application.Modules.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        // Persona
        RuleFor(x => x.DocumentTypeId)
            .GreaterThan(0)
            .WithMessage("El tipo de documento es requerido.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("El número de documento es requerido.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(80)
            .WithMessage("El nombre es requerido.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(80)
            .WithMessage("El apellido es requerido.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150)
            .WithMessage("El email no es válido.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(25)
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.Gender)
            .Must(g => g is null || g == 'M' || g == 'F' || g == 'O')
            .WithMessage("El género debe ser M, F u O.");

        // Cuenta
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("Username solo puede contener letras, números, '.', '_' y '-'.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches(@"[A-Z]").WithMessage("La contraseña debe tener al menos una mayúscula.")
            .Matches(@"[0-9]").WithMessage("La contraseña debe tener al menos un número.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("La contraseña debe tener al menos un carácter especial.");
    }
}