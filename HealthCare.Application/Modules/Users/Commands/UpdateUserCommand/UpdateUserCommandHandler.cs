using HealthCare.Application.Modules.Users.DTOs;
using HealthCare.Domain.Modules.Users;
using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.UpdateUserCommand;

public sealed class UpdateUserCommandHandler(
    IUserRepository userRepository
) : IRequestHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
                   ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        if (user.Username != request.Username &&
            await userRepository.ExistsByUsernameAsync(request.Username))
            throw new InvalidOperationException($"Username '{request.Username}' is already taken.");

        user.UpdateUsername(request.Username, request.UpdatedBy);

        await userRepository.UpdateAsync(user);

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