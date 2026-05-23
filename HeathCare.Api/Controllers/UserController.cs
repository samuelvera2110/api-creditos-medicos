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
