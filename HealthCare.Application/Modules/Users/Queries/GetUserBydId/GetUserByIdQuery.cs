using HealthCare.Application.Modules.Users.DTOs;
using MediatR;

namespace HealthCare.Application.Modules.Users.Queries.GetUserBydId;

public sealed record GetUserByIdQuery(int UserId) : IRequest<UserDto>;
