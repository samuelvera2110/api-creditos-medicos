# Diseño: Migración de CQRS/MediatR a Service Pattern

**Fecha:** 2026-05-23  
**Alcance:** `HealthCare.Application` — módulos Users y Roles  
**Estado:** Aprobado

---

## Objetivo

Reemplazar la capa CQRS (Commands, Queries, Handlers vía MediatR) por clases de servicio simples, una por dominio, con todos los métodos adentro. El módulo Auth ya usa este patrón y sirve como referencia.

---

## Estructura objetivo

```
HealthCare.Application/
  Users/
    UserService.cs      ← IUserService + UserService en el mismo archivo
    UserDto.cs          ← sin cambios
    UserRequests.cs     ← CreateUserRequest + UpdateUserRequest juntos
  Roles/
    RoleService.cs      ← IRoleService + RoleService en el mismo archivo
    RoleDto.cs          ← sin cambios
    RoleRequests.cs     ← CreateRoleRequest + UpdateRoleRequest juntos
  Auth/                 ← no cambia
    AuthService.cs
    (demás archivos existentes)
  Common/               ← no cambia
```

---

## Contratos de servicio

### IUserService / UserService

```csharp
public interface IUserService
{
    Task<PagedResult<UserDto>> GetAllAsync(int page, int pageSize, bool? isActive, CancellationToken ct);
    Task<UserDto> GetByIdAsync(int id, CancellationToken ct);
    Task<UserDto> CreateAsync(CreateUserRequest request, int createdBy, CancellationToken ct);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, int updatedBy, CancellationToken ct);
    Task ActivateAsync(int id, int updatedBy, CancellationToken ct);
    Task DeactivateAsync(int id, int updatedBy, CancellationToken ct);
}
```

### IRoleService / RoleService

```csharp
public interface IRoleService
{
    Task<PagedResult<RoleDto>> GetAllAsync(int page, int pageSize, bool? isActive, CancellationToken ct);
    Task<RoleDto> GetByIdAsync(int id, CancellationToken ct);
    Task<RoleDto> CreateAsync(CreateRoleRequest request, int? createdBy, CancellationToken ct);
    Task<RoleDto> UpdateAsync(int id, UpdateRoleRequest request, int? updatedBy, CancellationToken ct);
    Task ActivateAsync(int id, int? updatedBy, CancellationToken ct);
    Task DeactivateAsync(int id, int? updatedBy, CancellationToken ct);
}
```

La lógica de los handlers existentes se mueve directamente a estos métodos sin cambios de comportamiento.

---

## Controllers

Los controllers inyectan el service por interfaz en vez de `IMediator`:

```csharp
// Antes
public sealed class UsersController(IMediator mediator) : ControllerBase

// Después
public sealed class UsersController(IUserService userService) : ControllerBase
```

Las llamadas pasan de `mediator.Send(new CreateUserCommand(...))` a `userService.CreateAsync(request, GetCurrentUserId(), ct)`.

Los try/catch existentes en los controllers se eliminan — `ExceptionMiddleware` ya maneja todo a través de `AppExceptions.*`.

---

## Validación

Los validators se asocian a los Request DTOs (no a Commands). FluentValidation sigue corriendo automático vía `AddFluentValidationAutoValidation()` — sin cambios en la infraestructura de validación.

Validators afectados:
- `CreateUserRequestValidator` (renombrar desde `CreateUserCommandValidator`)
- `UpdateUserRequestValidator` (renombrar desde `UpdateUserCommandValidator`)
- `CreateRoleRequestValidator` (renombrar desde `CreateRoleCommandValidator`)
- `UpdateRoleRequestValidator` (renombrar desde `UpdateRoleCommandValidator`)

---

## Lo que se elimina

| Qué | Ubicación |
|-----|-----------|
| `Commands/` completo | `Application/Modules/Users/` |
| `Queries/` completo | `Application/Modules/Users/` |
| `Commands/` completo | `Application/Modules/Roles/` |
| `Queries/` completo | `Application/Modules/Roles/` |
| `ValidationBehavior.cs` | `Shared/Behaviours/` |
| Paquete `MediatR` | `Application.csproj` |
| Paquete `MediatR` | `Api.csproj` (referencia transitiva) |
| Registro MediatR + ValidationBehavior | `ServiceCollectionExtension.cs` |

---

## Lo que NO cambia

- `AppExceptions` y `ExceptionMiddleware` — siguen siendo el mecanismo de error handling
- `ApiResponse<T>`, `PagedResult<T>` — mismos wrappers de respuesta
- Repositorios e interfaces de repositorio — sin cambios
- `AuthService` y módulo Auth completo — ya usa el patrón destino
- Entidades de dominio — sin cambios
- Configuración de DI para repositorios (Scrutor scan) — sin cambios

---

## Registro de DI

El scan de Scrutor actual filtra por `InNamespaces("HealthCare.Application.Modules")`. Con la nueva estructura plana, los namespaces cambian a `HealthCare.Application.Users` y `HealthCare.Application.Roles`, por lo que ese filtro debe actualizarse en `ServiceCollectionExtension.cs`:

```csharp
// Antes
.InNamespaces("HealthCare.Application.Modules")

// Después — el sufijo "Service" ya es filtro suficiente
.AddClasses(classes => classes
    .Where(t => t.Name.EndsWith("Service")))
```

Este es el único cambio necesario en `ServiceCollectionExtension.cs`.

---

## Criterio de éxito

- El proyecto compila sin referencias a `MediatR`
- Los 3 controllers funcionan vía service directo
- Todos los endpoints responden igual que antes
- No quedan carpetas `Commands/` ni `Queries/` en Application
