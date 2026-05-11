using HealthCare.Application.Modules.Users.DTOs;
using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    int      DocumentTypeId,
    string   DocumentNumber,
    string   FirstName,
    string   LastName,
    string   Email,
    string?  PhoneNumber,
    DateTime? BirthDate,
    char?    Gender,

    string   Username,
    string   Password,
    int      CreatedBy
) : IRequest<UserDto>;