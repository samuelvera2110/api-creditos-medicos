using FluentValidation;

namespace HealthCare.Application.Roles;

public sealed record CreateRoleRequest(
    string  Name,
    string? Description
);

public sealed record UpdateRoleRequest(
    string  Name,
    string? Description
);

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(250)
            .When(x => x.Description is not null);
    }
}

public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(250)
            .When(x => x.Description is not null);
    }
}
