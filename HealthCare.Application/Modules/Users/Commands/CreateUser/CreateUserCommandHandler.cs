using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Application.Modules.Users.DTOs;
using HealthCare.Domain.Modules.Person.Entities;
using HealthCare.Domain.Modules.Person;
using HealthCare.Domain.Modules.Users;
using HealthCare.Domain.Modules.Users.Entities;
using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IUserRepository   userRepository,
    IPersonRepository personRepository,
    IPasswordHasher   passwordHasher
) : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken ct)
    {
        if (await userRepository.ExistsByUsernameAsync(request.Username))
            throw new InvalidOperationException($"El username '{request.Username}' ya está en uso.");

        if (await personRepository.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException($"El email '{request.Email}' ya está registrado.");

        if (await personRepository.ExistsByDocumentAsync(request.DocumentTypeId, request.DocumentNumber))
            throw new InvalidOperationException("El número de documento ya está registrado.");

        var person = new Person(
            request.DocumentTypeId,
            request.DocumentNumber,
            request.FirstName,
            request.LastName,
            request.Email
        );

        person.UpdateContactInfo(request.Email, request.PhoneNumber);

        await personRepository.AddAsync(person);

        // Declaración inline de las variables out
        passwordHasher.CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

        var user = new User(person.Id, request.Username, hash, salt);

        await userRepository.AddAsync(user);

        var created = await userRepository.GetByUsernameAsync(request.Username)
            ?? throw new InvalidOperationException("Error al crear el usuario.");

        return new UserDto(
            created.Id,
            created.PersonId,
            created.Username,
            created.Person?.FirstName ?? string.Empty,
            created.Person?.LastName  ?? string.Empty,
            created.Person?.Email     ?? string.Empty,
            created.MustChangePassword,
            created.IsActive,
            created.CreatedAt,
            created.Roles.Select(r => r.Name)
        );
    }
}