using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(int UserId, int UpdatedBy) : IRequest<Unit>;
