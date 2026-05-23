# CQRS → Service Pattern — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reemplazar Commands/Queries/Handlers + MediatR por clases de servicio simples (IUserService, IRoleService) en una estructura plana dentro de `HealthCare.Application/`.

**Architecture:** Una clase de servicio por módulo (interfaz + implementación en el mismo archivo). Los validators pasan a los Request DTOs y siguen corriendo automático vía FluentValidation. Los controllers inyectan el service directamente en vez de IMediator.

**Tech Stack:** .NET 9, ASP.NET Core, FluentValidation 11, EF Core (Npgsql), Scrutor

---

## Mapa de archivos

| Acción | Archivo |
|--------|---------|
| CREAR | `HealthCare.Application/Users/UserDto.cs` |
| CREAR | `HealthCare.Application/Users/UserRequests.cs` |
| CREAR | `HealthCare.Application/Users/UserService.cs` |
| CREAR | `HealthCare.Application/Roles/RoleDto.cs` |
| CREAR | `HealthCare.Application/Roles/RoleRequests.cs` |
| CREAR | `HealthCare.Application/Roles/RoleService.cs` |
| MODIFICAR | `HeathCare.Api/Controllers/UserController.cs` |
| MODIFICAR | `HeathCare.Api/Controllers/RoleController.cs` |
| MODIFICAR | `HeathCare.Api/Extensions/ServiceCollectionExtension.cs` |
| MODIFICAR | `HealthCare.Application/HealthCare.Application.csproj` |
| MODIFICAR | `HealthCare.Shared/HealthCare.Shared.csproj` |
| ELIMINAR | `HealthCare.Application/Modules/Users/Commands/` (directorio completo) |
| ELIMINAR | `HealthCare.Application/Modules/Users/Queries/` (directorio completo) |
| ELIMINAR | `HealthCare.Application/Modules/Users/DTOs/` (directorio completo) |
| ELIMINAR | `HealthCare.Application/Modules/Roles/Commands/` (directorio completo) |
| ELIMINAR | `HealthCare.Application/Modules/Roles/Queries/` (directorio completo) |
| ELIMINAR | `HealthCare.Application/Modules/Roles/DTOs/` (directorio completo) |
| ELIMINAR | `HealthCare.Shared/Behaviours/ValidationBehavior.cs` |

---

## Task 1: Crear archivos planos del módulo Users

**Files:**
- Create: `HealthCare.Application/Users/UserDto.cs`
- Create: `HealthCare.Application/Users/UserRequests.cs`

- [ ] **Step 1: Crear directorio y UserDto.cs**

```csharp
// HealthCare.Application/Users/UserDto.cs
namespace HealthCare.Application.Users;

public sealed record UserDto(
    int      Id,
    int      PersonId,
    string   Username,
    string   FirstName,
    string   LastName,
    string   Email,
    bool     MustChangePassword,
    bool     IsActive,
    DateTime CreatedAt,
    IEnumerable<string> Roles,
    string?  TemporaryPassword = null
);
```

- [ ] **Step 2: Crear UserRequests.cs con requests y validators juntos**

```csharp
// HealthCare.Application/Users/UserRequests.cs
using FluentValidation;

namespace HealthCare.Application.Users;

public sealed record CreateUserRequest(
    int       DocumentTypeId,
    string    DocumentNumber,
    string    FirstName,
    string    LastName,
    string    Email,
    string?   PhoneNumber,
    DateTime? BirthDate,
    char?     Gender
);

public sealed record UpdateUserRequest(
    string    Username,
    string    FirstName,
    string    LastName,
    string    Email,
    string?   PhoneNumber,
    DateTime? BirthDate,
    char?     Gender
);

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.DocumentTypeId)
            .GreaterThan(0)
            .WithMessage("El tipo de documento es requerido.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(25)
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.Gender)
            .Must(g => g is null || g == 'M' || g == 'F' || g == 'O')
            .WithMessage("El género debe ser M, F u O.");
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("Username solo puede contener letras, números, '.', '_' y '-'.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(25)
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.Gender)
            .Must(g => g is null || g == 'M' || g == 'F' || g == 'O')
            .WithMessage("El género debe ser M, F u O.");
    }
}
```

---

## Task 2: Crear UserService.cs

**Files:**
- Create: `HealthCare.Application/Users/UserService.cs`

- [ ] **Step 1: Crear UserService.cs con interfaz e implementación**

```csharp
// HealthCare.Application/Users/UserService.cs
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
    IUserRepository   userRepository,
    IPersonRepository personRepository,
    IPasswordHasher   passwordHasher,
    IEmailService     emailService,
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

        var username    = await GenerateUniqueUsernameAsync(request.FirstName, request.LastName);
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
```

- [ ] **Step 2: Verificar que compila**

```bash
dotnet build HealthCare.Application/HealthCare.Application.csproj
```

Esperado: 0 errores (puede haber warnings por referencias a MediatR que aún existen, son normales en este punto).

---

## Task 3: Crear archivos planos del módulo Roles

**Files:**
- Create: `HealthCare.Application/Roles/RoleDto.cs`
- Create: `HealthCare.Application/Roles/RoleRequests.cs`

- [ ] **Step 1: Crear RoleDto.cs**

```csharp
// HealthCare.Application/Roles/RoleDto.cs
namespace HealthCare.Application.Roles;

public sealed record RoleDto(
    int       Id,
    string    Name,
    string?   Description,
    bool      IsActive,
    DateTime  CreatedAt,
    DateTime? UpdatedAt
);
```

- [ ] **Step 2: Crear RoleRequests.cs con requests y validators**

```csharp
// HealthCare.Application/Roles/RoleRequests.cs
using FluentValidation;

namespace HealthCare.Application.Roles;

public sealed record CreateRoleRequest(
    string  Name,
    string? Description
);

public sealed record UpdateRoleRequest(
    string  Name,
    string? Description
);

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(250)
            .When(x => x.Description is not null);
    }
}

public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(250)
            .When(x => x.Description is not null);
    }
}
```

---

## Task 4: Crear RoleService.cs

**Files:**
- Create: `HealthCare.Application/Roles/RoleService.cs`

- [ ] **Step 1: Crear RoleService.cs**

```csharp
// HealthCare.Application/Roles/RoleService.cs
using HealthCare.Domain.Modules.Role.Interfaces;
using HealthCare.Shared.Common;
using HealthCare.Shared.Exceptions;

using DomainRole = HealthCare.Domain.Modules.Role.Entities.Role;

namespace HealthCare.Application.Roles;

public interface IRoleService
{
    Task<PagedResult<RoleDto>> GetAllAsync(int page, int pageSize, bool? isActive, CancellationToken ct);
    Task<RoleDto> GetByIdAsync(int id, CancellationToken ct);
    Task<RoleDto> CreateAsync(CreateRoleRequest request, int? createdBy, CancellationToken ct);
    Task<RoleDto> UpdateAsync(int id, UpdateRoleRequest request, int? updatedBy, CancellationToken ct);
    Task ActivateAsync(int id, int? updatedBy, CancellationToken ct);
    Task DeactivateAsync(int id, int? updatedBy, CancellationToken ct);
}

public sealed class RoleService(IRoleRepository roleRepository) : IRoleService
{
    public async Task<PagedResult<RoleDto>> GetAllAsync(
        int page, int pageSize, bool? isActive, CancellationToken ct)
    {
        var p    = Math.Max(1, page);
        var size = Math.Clamp(pageSize, 1, 100);
        var (items, total) = await roleRepository.GetAllAsync(p, size, isActive, ct);
        return new PagedResult<RoleDto>(items.Select(ToDto), total, p, size);
    }

    public async Task<RoleDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var role = await roleRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Rol {id} no encontrado.");
        return ToDto(role);
    }

    public async Task<RoleDto> CreateAsync(
        CreateRoleRequest request, int? createdBy, CancellationToken ct)
    {
        if (await roleRepository.ExistsByNameAsync(request.Name))
            throw new AppExceptions.ConflictException($"El rol '{request.Name}' ya existe.");

        var role = new DomainRole(request.Name, request.Description, createdBy);
        await roleRepository.AddAsync(role);

        var created = await roleRepository.GetByNameAsync(request.Name)
                      ?? throw new AppExceptions.BadRequestException("Error al crear el rol.");
        return ToDto(created);
    }

    public async Task<RoleDto> UpdateAsync(
        int id, UpdateRoleRequest request, int? updatedBy, CancellationToken ct)
    {
        var role = await roleRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Rol {id} no encontrado.");

        if (await roleRepository.ExistsByNameAsync(request.Name, id))
            throw new AppExceptions.ConflictException($"El rol '{request.Name}' ya existe.");

        role.UpdateDetails(request.Name, request.Description, updatedBy);
        await roleRepository.UpdateAsync(role);
        return ToDto(role);
    }

    public async Task ActivateAsync(int id, int? updatedBy, CancellationToken ct)
    {
        var role = await roleRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Rol {id} no encontrado.");

        if (role.IsActive)
            throw new AppExceptions.BadRequestException("El rol ya está activo.");

        role.Activate(updatedBy);
        await roleRepository.UpdateAsync(role);
    }

    public async Task DeactivateAsync(int id, int? updatedBy, CancellationToken ct)
    {
        var role = await roleRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Rol {id} no encontrado.");

        if (!role.IsActive)
            throw new AppExceptions.BadRequestException("El rol ya está inactivo.");

        role.Deactivate(updatedBy);
        await roleRepository.UpdateAsync(role);
    }

    private static RoleDto ToDto(DomainRole r) =>
        new(r.Id, r.Name, r.Description, r.IsActive, r.CreatedAt, r.UpdatedAt);
}
```

- [ ] **Step 2: Commit de los 6 archivos nuevos**

```bash
git add HealthCare.Application/Users/ HealthCare.Application/Roles/
git commit -m "feat: add flat UserService and RoleService replacing CQRS handlers"
```

---

## Task 5: Actualizar UsersController

**Files:**
- Modify: `HeathCare.Api/Controllers/UserController.cs`

- [ ] **Step 1: Reemplazar el controller completo**

```csharp
// HeathCare.Api/Controllers/UserController.cs
using System.Security.Claims;
using HealthCare.Application.Users;
using HealthCare.Shared.Common;
using HealthCare.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeathCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Users")]
[Authorize(Roles = "Admin,HR")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Listar usuarios")]
    [EndpointDescription("Retorna una lista paginada de usuarios con filtro opcional por estado.")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int   page     = 1,
        [FromQuery] int   pageSize = 20,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var result = await userService.GetAllAsync(page, pageSize, isActive, ct);
        return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result, "Usuarios obtenidos correctamente."));
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Obtener usuario por ID")]
    [EndpointDescription("Retorna el detalle completo de un usuario incluyendo persona, perfil y roles asignados.")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await userService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<UserDto>.Ok(result, "Usuario obtenido correctamente."));
    }

    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Crear usuario")]
    [EndpointDescription("Crea la persona y la cuenta de usuario en una sola operación. La contraseña es hasheada con PBKDF2-SHA512.")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        var dto = await userService.CreateAsync(request, GetCurrentUserId() ?? 0, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = dto.Id },
            ApiResponse<UserDto>.Ok(dto, "Usuario creado correctamente."));
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Actualizar usuario")]
    [EndpointDescription("Actualiza el username y los datos personales del usuario en una sola operación.")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
    {
        var dto = await userService.UpdateAsync(id, request, GetCurrentUserId() ?? 0, ct);
        return Ok(ApiResponse<UserDto>.Ok(dto, "Usuario actualizado correctamente."));
    }

    [HttpPatch("{id:int}/deactivate")]
    [EndpointSummary("Desactivar usuario")]
    [EndpointDescription("Desactiva la cuenta. El usuario no podrá iniciar sesión mientras esté inactivo.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await userService.DeactivateAsync(id, GetCurrentUserId() ?? 0, ct);
        return Ok(ApiResponse<object>.Ok("Usuario desactivado correctamente."));
    }

    [HttpPatch("{id:int}/activate")]
    [EndpointSummary("Activar usuario")]
    [EndpointDescription("Reactiva una cuenta de usuario previamente desactivada.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await userService.ActivateAsync(id, GetCurrentUserId() ?? 0, ct);
        return Ok(ApiResponse<object>.Ok("Usuario activado correctamente."));
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}
```

---

## Task 6: Actualizar RolesController

**Files:**
- Modify: `HeathCare.Api/Controllers/RoleController.cs`

- [ ] **Step 1: Reemplazar el controller completo**

```csharp
// HeathCare.Api/Controllers/RoleController.cs
using System.Security.Claims;
using HealthCare.Application.Roles;
using HealthCare.Shared.Common;
using HealthCare.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeathCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Roles")]
[Authorize(Roles = "Admin")]
public sealed class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Listar roles")]
    [EndpointDescription("Retorna una lista paginada de roles con filtro opcional por estado.")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RoleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int   page     = 1,
        [FromQuery] int   pageSize = 20,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var result = await roleService.GetAllAsync(page, pageSize, isActive, ct);
        return Ok(ApiResponse<PagedResult<RoleDto>>.Ok(result, "Roles obtenidos correctamente."));
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Obtener rol por ID")]
    [EndpointDescription("Retorna el detalle de un rol.")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await roleService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<RoleDto>.Ok(result, "Rol obtenido correctamente."));
    }

    [HttpPost]
    [EndpointSummary("Crear rol")]
    [EndpointDescription("Crea un nuevo rol. El nombre debe ser único.")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoleRequest request,
        CancellationToken ct)
    {
        var dto = await roleService.CreateAsync(request, GetCurrentUserId(), ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = dto.Id },
            ApiResponse<RoleDto>.Ok(dto, "Rol creado correctamente."));
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Actualizar rol")]
    [EndpointDescription("Actualiza el nombre y la descripción de un rol.")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateRoleRequest request,
        CancellationToken ct)
    {
        var dto = await roleService.UpdateAsync(id, request, GetCurrentUserId(), ct);
        return Ok(ApiResponse<RoleDto>.Ok(dto, "Rol actualizado correctamente."));
    }

    [HttpPatch("{id:int}/deactivate")]
    [EndpointSummary("Desactivar rol")]
    [EndpointDescription("Desactiva un rol previamente activo.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await roleService.DeactivateAsync(id, GetCurrentUserId(), ct);
        return Ok(ApiResponse<object>.Ok("Rol desactivado correctamente."));
    }

    [HttpPatch("{id:int}/activate")]
    [EndpointSummary("Activar rol")]
    [EndpointDescription("Reactiva un rol previamente desactivado.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await roleService.ActivateAsync(id, GetCurrentUserId(), ct);
        return Ok(ApiResponse<object>.Ok("Rol activado correctamente."));
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}
```

- [ ] **Step 2: Commit de los controllers actualizados**

```bash
git add HeathCare.Api/Controllers/UserController.cs HeathCare.Api/Controllers/RoleController.cs
git commit -m "refactor: replace IMediator with IUserService and IRoleService in controllers"
```

---

## Task 7: Actualizar ServiceCollectionExtension

**Files:**
- Modify: `HeathCare.Api/Extensions/ServiceCollectionExtension.cs`

- [ ] **Step 1: Reemplazar el archivo completo**

```csharp
// HeathCare.Api/Extensions/ServiceCollectionExtension.cs
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Application.Modules.Auth.Services;
using HealthCare.Application.Modules.Auth.Validators;
using HealthCare.Infrastructure.Email;
using HealthCare.Infrastructure.Persistence.Context;
using HealthCare.Infrastructure.Repositories;
using HealthCare.Infrastructure.Security;
using HealthCare.Shared.Constants;
using HealthCare.Shared.Wrappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HeathCare.Api.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddSecurity(configuration);
        services.AddEmail(configuration);
        services.AddCorsPolicy(configuration);
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddScalar();
        services.AddControllers();
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        services.ConfigureApiBehavior();
    }

    private static void ConfigureApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return new BadRequestObjectResult(
                    ApiResponse<object>.Error("Errores de validación.", errors));
            };
        });
    }

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE")
            ?? configuration[ConfigurationConstants.CONNECTION_STRING_DATABASE]
            ?? throw new Exception(
                $"No se encontró la configuración: {ConfigurationConstants.CONNECTION_STRING_DATABASE}");

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<HeathCareDbContext>(options =>
            options.UseNpgsql(connectionString));
    }

    private static void AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<IJwtProvider, JwtProvider>();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "La sección 'Jwt' no está configurada en appsettings.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                Encoding.UTF8.GetBytes(jwtSettings.PrivateKey)),
                    ValidateIssuer   = true,
                    ValidIssuer      = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience    = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew        = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
    }

    private static void AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpSettings>(options =>
        {
            options.Host = configuration[ConfigurationConstants.SMTP_HOST]
                ?? throw new InvalidOperationException(
                    $"No se encontró la configuración: {ConfigurationConstants.SMTP_HOST}");

            options.User = configuration[ConfigurationConstants.SMTP_USER]
                ?? throw new InvalidOperationException(
                    $"No se encontró la configuración: {ConfigurationConstants.SMTP_USER}");

            options.Password = configuration[ConfigurationConstants.SMTP_PASSWORD]
                ?? throw new InvalidOperationException(
                    $"No se encontró la configuración: {ConfigurationConstants.SMTP_PASSWORD}");

            options.From = configuration[ConfigurationConstants.SMTP_FROM]
                ?? throw new InvalidOperationException(
                    $"No se encontró la configuración: {ConfigurationConstants.SMTP_FROM}");

            options.Port = int.TryParse(
                configuration[ConfigurationConstants.SMTP_PORT], out var port) ? port : 587;

            options.EnableSsl = !bool.TryParse(
                configuration[ConfigurationConstants.SMTP_ENABLE_SSL], out var ssl) || ssl;
        });

        services.AddScoped<IEmailService, EmailService>();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<UserRepository>()
            .AddClasses(classes => classes
                .InNamespaces("HealthCare.Infrastructure.Repositories"))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }

    private static void AddApplicationServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<AuthService>()
            .AddClasses(classes => classes
                .Where(t => t.Name.EndsWith("Service")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<IPasswordHasher, PasswordHasher>();
    }

    private static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                if (allowedOrigins.Length > 0)
                    policy.WithOrigins(allowedOrigins);
                else
                    policy.SetIsOriginAllowed(_ => false);

                policy
                    .WithHeaders("Content-Type", "Authorization")
                    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE");
            });
        });
    }

    public static void AddScalar(this IServiceCollection services)
    {
        services.AddOpenApi();
    }
}
```

Cambios clave respecto al original:
- Eliminado el método `AddMediatR()` y su llamada en `AddCore()`
- Eliminada la referencia a `ValidationBehavior` y a `CreateUserCommandHandler`
- Eliminados los `using` de MediatR y de los namespaces de Commands
- En `AddApplicationServices()`: el filtro `InNamespaces("HealthCare.Application.Modules")` reemplazado por `.Where(t => t.Name.EndsWith("Service"))` para cubrir los nuevos namespaces

- [ ] **Step 2: Commit**

```bash
git add HeathCare.Api/Extensions/ServiceCollectionExtension.cs
git commit -m "refactor: remove MediatR DI registration, update Scrutor scan for flat service namespaces"
```

---

## Task 8: Eliminar MediatR de los csproj

**Files:**
- Modify: `HealthCare.Application/HealthCare.Application.csproj`
- Modify: `HealthCare.Shared/HealthCare.Shared.csproj`

- [ ] **Step 1: Quitar MediatR de Application.csproj**

Eliminar esta línea de `HealthCare.Application/HealthCare.Application.csproj`:
```xml
<PackageReference Include="MediatR" Version="12.4.1" />
```

El archivo queda:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\HealthCare.Domain\HealthCare.Domain.csproj" />
    <ProjectReference Include="..\HealthCare.Shared\HealthCare.Shared.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.11.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.4" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Quitar MediatR de Shared.csproj**

Eliminar esta línea de `HealthCare.Shared/HealthCare.Shared.csproj`:
```xml
<PackageReference Include="MediatR" Version="12.4.1" />
```

El archivo queda:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <Folder Include="Utilities\" />
    <Folder Include="Wrappers\" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.11.0" />
  </ItemGroup>
</Project>
```

---

## Task 9: Eliminar archivos viejos

**Files:**
- Delete: carpetas `Commands/`, `Queries/`, `DTOs/` de Users y Roles
- Delete: `ValidationBehavior.cs`

- [ ] **Step 1: Eliminar módulo Users viejo**

```bash
rm -rf HealthCare.Application/Modules/Users/Commands
rm -rf HealthCare.Application/Modules/Users/Queries
rm -rf HealthCare.Application/Modules/Users/DTOs
```

Verificar que la carpeta queda vacía:
```bash
ls HealthCare.Application/Modules/Users/
```
Esperado: output vacío (la carpeta puede eliminarse también).

```bash
rmdir HealthCare.Application/Modules/Users
```

- [ ] **Step 2: Eliminar módulo Roles viejo**

```bash
rm -rf HealthCare.Application/Modules/Roles/Commands
rm -rf HealthCare.Application/Modules/Roles/Queries
rm -rf HealthCare.Application/Modules/Roles/DTOs
rmdir HealthCare.Application/Modules/Roles
```

- [ ] **Step 3: Eliminar ValidationBehavior**

```bash
rm HealthCare.Shared/Behaviours/ValidationBehavior.cs
rmdir HealthCare.Shared/Behaviours
```

- [ ] **Step 4: Commit de eliminaciones**

```bash
git add -A
git commit -m "chore: delete CQRS Commands, Queries, DTOs folders and ValidationBehavior"
```

---

## Task 10: Build final y verificación

- [ ] **Step 1: Build completo de la solución**

```bash
dotnet build
```

Esperado: `Build succeeded` con 0 errores. Si hay errores de compilación, son referencias a namespaces viejos que quedaron sin limpiar — buscar con:
```bash
grep -r "HealthCare.Application.Modules.Users.Commands\|HealthCare.Application.Modules.Roles\|MediatR" --include="*.cs" .
```

- [ ] **Step 2: Levantar la API**

```bash
dotnet run --project HeathCare.Api
```

Esperado: la API arranca sin excepciones en el log de startup.

- [ ] **Step 3: Verificar endpoints en Scalar**

Abrir `http://localhost:<puerto>/scalar/v1` y confirmar que aparecen los endpoints de Users, Roles y Auth.

- [ ] **Step 4: Commit final**

```bash
git add -A
git commit -m "feat: complete migration from CQRS/MediatR to flat service pattern"
```
