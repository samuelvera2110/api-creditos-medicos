using HealthCare.Application.Modules.Users.DTOs;
using HealthCare.Domain.Modules.Users;
using MediatR;

namespace HealthCare.Application.Modules.Users.Queries.GetUserBydId;

public sealed class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
                   ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        return new UserDto(
            user.Id, user.PersonId, user.Username,
            user.Person?.FirstName ?? string.Empty,
            user.Person?.LastName  ?? string.Empty,
            user.Person?.Email     ?? string.Empty,
            user.MustChangePassword,
            user.IsActive,
            user.CreatedAt,
            user.Roles.Select(r => r.Name)
        );
    }
}