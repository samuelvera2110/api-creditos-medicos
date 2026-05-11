using HealthCare.Application.Modules.Users.DTOs;
using HealthCare.Domain.Modules.Person;
using HealthCare.Domain.Modules.Users;
using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler(
    IUserRepository   userRepository,
    IPersonRepository personRepository
) : IRequestHandler<UpdateUser.UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException($"Usuario {request.UserId} no encontrado.");

        if (user.Username != request.Username &&
            await userRepository.ExistsByUsernameAsync(request.Username))
            throw new InvalidOperationException($"El username '{request.Username}' ya está en uso.");

        var person = await personRepository.GetByIdAsync(user.PersonId)
            ?? throw new KeyNotFoundException($"Persona {user.PersonId} no encontrada.");

        if (person.Email != request.Email &&
            await personRepository.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException($"El email '{request.Email}' ya está registrado.");

        user.UpdateUsername(request.Username, request.UpdatedBy);
        await userRepository.UpdateAsync(user);

        person.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.BirthDate,
            request.Gender
        );
        await personRepository.UpdateAsync(person);

        return new UserDto(
            user.Id,
            user.PersonId,
            user.Username,
            person.FirstName,
            person.LastName,
            person.Email,
            user.MustChangePassword,
            user.IsActive,
            user.CreatedAt,
            user.Roles.Select(r => r.Name)
        );
    }
}