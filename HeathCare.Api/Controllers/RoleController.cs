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
