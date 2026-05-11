using HealthCare.Application.Modules.Users.DTOs;
using HealthCare.Domain.Modules.Users;
using HealthCare.Shared.Common;
using MediatR;

namespace HealthCare.Application.Modules.Users.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetAllUsersQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (items, total) = await userRepository.GetAllAsync(page, pageSize, request.IsActive, ct);

        var dtos = items.Select(u => new UserDto(
            u.Id, u.PersonId, u.Username,
            u.Person?.FirstName ?? string.Empty,
            u.Person?.LastName  ?? string.Empty,
            u.Person?.Email     ?? string.Empty,
            u.MustChangePassword,
            u.IsActive,
            u.CreatedAt,
            u.Roles.Select(r => r.Name)
        ));

        return new PagedResult<UserDto>(dtos, total, page, pageSize);
    }
}