using HealthCare.Application.Modules.Users.DTOs;
using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.UpdateUser;


public sealed record UpdateUserCommand(
    int    UserId,
    string Username,

    string    FirstName,
    string    LastName,
    string    Email,
    string?   PhoneNumber,
    DateTime? BirthDate,
    char?     Gender,

    int UpdatedBy
) : IRequest<UserDto>;