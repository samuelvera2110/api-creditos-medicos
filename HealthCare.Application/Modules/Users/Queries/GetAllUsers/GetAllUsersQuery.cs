using HealthCare.Application.Modules.Users.DTOs;
using HealthCare.Shared.Common;
using MediatR;

namespace HealthCare.Application.Modules.Users.Queries.GetAllUsers;

public sealed record GetAllUsersQuery(
    int   Page     = 1,
    int   PageSize = 20,
    bool? IsActive = null
) : IRequest<PagedResult<UserDto>>;