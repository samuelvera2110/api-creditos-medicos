using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Application.Modules.Users.DTOs;
using HealthCare.Domain.Modules.Users;
using HealthCare.Domain.Modules.Users.Entities;
using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.CreateUser;


public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher
) : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken ct)
    {
        if (await userRepository.ExistsByUsernameAsync(request.Username))
            throw new InvalidOperationException($"Username '{request.Username}' is already taken.");

        if (await userRepository.ExistsByPersonIdAsync(request.PersonId))
            throw new InvalidOperationException("This person already has a user account.");

        var (hash, salt) = passwordHasher.CreatePasswordHash(request.Password);

        var user = new User(request.PersonId, request.Username, hash, salt);

        await userRepository.AddAsync(user);

        var created = await userRepository.GetByUsernameAsync(request.Username)
                      ?? throw new InvalidOperationException("User creation failed.");

        return MapToDto(created);
    }

    private static UserDto MapToDto(User u) => new(
        u.Id,
        u.PersonId,
        u.Username,
        u.Person?.FirstName ?? string.Empty,
        u.Person?.LastName  ?? string.Empty,
        u.Person?.Email     ?? string.Empty,
        u.MustChangePassword,
        u.IsActive,
        u.CreatedAt,
        u.Roles.Select(r => r.Name)
    );
}