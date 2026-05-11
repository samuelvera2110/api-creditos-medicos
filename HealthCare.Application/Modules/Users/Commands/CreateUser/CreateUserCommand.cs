using HealthCare.Application.Modules.Users.DTOs;
using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    int    PersonId,
    string Username,
    string Password,
    int    CreatedBy
) : IRequest<UserDto>;