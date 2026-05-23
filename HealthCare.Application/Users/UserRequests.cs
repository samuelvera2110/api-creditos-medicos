using FluentValidation;

namespace HealthCare.Application.Users;

public sealed record CreateUserRequest(
    int       DocumentTypeId,
    string    DocumentNumber,
    string    FirstName,
    string    LastName,
    string    Email,
    string?   PhoneNumber,
    DateTime? BirthDate,
    char?     Gender
);

public sealed record UpdateUserRequest(
    string    Username,
    string    FirstName,
    string    LastName,
    string    Email,
    string?   PhoneNumber,
    DateTime? BirthDate,
    char?     Gender
);

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
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

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
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
