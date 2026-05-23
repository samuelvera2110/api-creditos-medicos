using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Domain.Modules.Person;
using HealthCare.Domain.Modules.Person.Entities;
using HealthCare.Domain.Modules.Users;
using HealthCare.Domain.Modules.Users.Entities;
using HealthCare.Shared.Common;
using HealthCare.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Users;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetAllAsync(int page, int pageSize, bool? isActive, CancellationToken ct);
    Task<UserDto> GetByIdAsync(int id, CancellationToken ct);
    Task<UserDto> CreateAsync(CreateUserRequest request, int createdBy, CancellationToken ct);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, int updatedBy, CancellationToken ct);
    Task ActivateAsync(int id, int updatedBy, CancellationToken ct);
    Task DeactivateAsync(int id, int updatedBy, CancellationToken ct);
}

public sealed class UserService(
    IUserRepository      userRepository,
    IPersonRepository    personRepository,
    IPasswordHasher      passwordHasher,
    IEmailService        emailService,
    ILogger<UserService> logger
) : IUserService
{
    public async Task<PagedResult<UserDto>> GetAllAsync(
        int page, int pageSize, bool? isActive, CancellationToken ct)
    {
        var p    = Math.Max(1, page);
        var size = Math.Clamp(pageSize, 1, 100);
        var (items, total) = await userRepository.GetAllAsync(p, size, isActive, ct);
        return new PagedResult<UserDto>(items.Select(ToDto), total, p, size);
    }

    public async Task<UserDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Usuario {id} no encontrado.");
        return ToDto(user);
    }

    public async Task<UserDto> CreateAsync(
        CreateUserRequest request, int createdBy, CancellationToken ct)
    {
        if (await personRepository.ExistsByEmailAsync(request.Email))
            throw new AppExceptions.ConflictException($"El email '{request.Email}' ya está registrado.");

        if (await personRepository.ExistsByDocumentAsync(request.DocumentTypeId, request.DocumentNumber))
            throw new AppExceptions.ConflictException("El número de documento ya está registrado.");

        var username     = await GenerateUniqueUsernameAsync(request.FirstName, request.LastName);
        var tempPassword = GenerateTemporaryPassword();

        var person = new Person(
            request.DocumentTypeId, request.DocumentNumber,
            request.FirstName, request.LastName, request.Email);
        person.Update(
            request.FirstName, request.LastName, request.Email,
            request.PhoneNumber, request.BirthDate, request.Gender);

        // El correo se envía antes de persistir: si falla, la operación se aborta
        // sin dejar un usuario huérfano en la base de datos.
        await SendCredentialsEmailAsync(person.Email, person.FirstName, username, tempPassword, ct);

        await personRepository.AddAsync(person);

        passwordHasher.CreatePasswordHash(tempPassword, out byte[] hash, out byte[] salt);
        var user = new User(person.Id, username, hash, salt);
        await userRepository.AddAsync(user);

        var created = await userRepository.GetByUsernameAsync(username)
                      ?? throw new AppExceptions.BadRequestException("Error al crear el usuario.");

        return ToDto(created) with { TemporaryPassword = tempPassword };
    }

    public async Task<UserDto> UpdateAsync(
        int id, UpdateUserRequest request, int updatedBy, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Usuario {id} no encontrado.");

        if (user.Username != request.Username
            && await userRepository.ExistsByUsernameAsync(request.Username))
            throw new AppExceptions.ConflictException($"El username '{request.Username}' ya está en uso.");

        if (user.Person?.Email != request.Email
            && await personRepository.ExistsByEmailAsync(request.Email))
            throw new AppExceptions.ConflictException($"El email '{request.Email}' ya está registrado.");

        user.Person?.Update(
            request.FirstName, request.LastName, request.Email,
            request.PhoneNumber, request.BirthDate, request.Gender);
        user.UpdateUsername(request.Username, updatedBy);

        await userRepository.UpdateAsync(user);
        return ToDto(user);
    }

    public async Task ActivateAsync(int id, int updatedBy, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Usuario {id} no encontrado.");

        if (user.IsActive)
            throw new AppExceptions.BadRequestException("El usuario ya está activo.");

        user.Activate(updatedBy);
        await userRepository.UpdateAsync(user);
    }

    public async Task DeactivateAsync(int id, int updatedBy, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Usuario {id} no encontrado.");

        if (!user.IsActive)
            throw new AppExceptions.BadRequestException("El usuario ya está inactivo.");

        user.Deactivate(updatedBy);
        await userRepository.UpdateAsync(user);
    }

    private static UserDto ToDto(User u) => new(
        u.Id, u.PersonId, u.Username,
        u.Person?.FirstName ?? string.Empty,
        u.Person?.LastName  ?? string.Empty,
        u.Person?.Email     ?? string.Empty,
        u.MustChangePassword,
        u.IsActive,
        u.CreatedAt,
        u.Roles.Select(r => r.Name)
    );

    private async Task SendCredentialsEmailAsync(
        string email, string firstName, string username, string tempPassword, CancellationToken ct)
    {
        const string subject = "Tu cuenta de HealthCare ha sido creada";
        var body = $"""
            <p>Hola {firstName},</p>
            <p>Tu cuenta en <strong>HealthCare</strong> ha sido creada exitosamente.</p>
            <p><strong>Usuario:</strong> {username}<br/>
            <strong>Contraseña temporal:</strong> {tempPassword}</p>
            <p>Por seguridad, deberás cambiar esta contraseña la primera vez que inicies sesión.</p>
            """;
        try
        {
            await emailService.SendAsync(email, subject, body, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar el correo de credenciales a {Email}", email);
            throw new AppExceptions.BadRequestException(
                "No se pudo enviar el correo con las credenciales. El usuario no fue creado.");
        }
    }

    private async Task<string> GenerateUniqueUsernameAsync(string firstName, string lastName)
    {
        var first = Normalize(firstName.Split(' ')[0]);
        var last  = Normalize(lastName.Split(' ')[0]);
        var base_ = $"{first}.{last}";
        if (!await userRepository.ExistsByUsernameAsync(base_)) return base_;
        var counter = 2;
        string candidate;
        do { candidate = $"{base_}{counter++}"; }
        while (await userRepository.ExistsByUsernameAsync(candidate));
        return candidate;
    }

    private static string Normalize(string input) =>
        string.Concat(
            input.Normalize(System.Text.NormalizationForm.FormD)
                 .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                              != System.Globalization.UnicodeCategory.NonSpacingMark)
        ).ToLowerInvariant();

    private static string GenerateTemporaryPassword()
    {
        const string upper   = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower   = "abcdefghijklmnopqrstuvwxyz";
        const string digits  = "0123456789";
        const string special = "!@#$%&*";
        const string all     = upper + lower + digits + special;
        var chars = new char[12];
        chars[0] = upper  [System.Security.Cryptography.RandomNumberGenerator.GetInt32(upper.Length)];
        chars[1] = digits [System.Security.Cryptography.RandomNumberGenerator.GetInt32(digits.Length)];
        chars[2] = special[System.Security.Cryptography.RandomNumberGenerator.GetInt32(special.Length)];
        chars[3] = lower  [System.Security.Cryptography.RandomNumberGenerator.GetInt32(lower.Length)];
        for (int i = 4; i < 12; i++)
            chars[i] = all[System.Security.Cryptography.RandomNumberGenerator.GetInt32(all.Length)];
        return new string(
            chars.OrderBy(_ => System.Security.Cryptography.RandomNumberGenerator.GetInt32(100)).ToArray());
    }
}
