using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.ActivateUser;

public sealed record ActivateUserCommand(int UserId, int UpdatedBy) : IRequest<Unit>;
