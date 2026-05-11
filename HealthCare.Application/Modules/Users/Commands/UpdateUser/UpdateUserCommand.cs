using HealthCare.Application.Modules.Users.DTOs;
using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.UpdateUserCommand;

public sealed record UpdateUserCommand(
    int    UserId,
    string Username,
    int    UpdatedBy
) : IRequest<UserDto>;