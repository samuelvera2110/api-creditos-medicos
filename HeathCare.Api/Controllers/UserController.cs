using System.Security.Claims;
using HealthCare.Application.Modules.Users.Commands.ActivateUser;
using HealthCare.Application.Modules.Users.Commands.CreateUser;
using HealthCare.Application.Modules.Users.Commands.DeactivateUser;
using HealthCare.Application.Modules.Users.Commands.UpdateUser;
using HealthCare.Application.Modules.Users.DTOs;
using HealthCare.Application.Modules.Users.DTOs.Requests;
using HealthCare.Application.Modules.Users.Queries.GetAllUsers;
using HealthCare.Application.Modules.Users.Queries.GetUserBydId;
using HealthCare.Shared.Common;
using HealthCare.Shared.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeathCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Users")]
[Authorize(Roles = "Admin,HR")]
public sealed class UsersController(IMediator mediator) : ControllerBase
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
        var result = await mediator.Send(new GetAllUsersQuery(page, pageSize, isActive), ct);
        return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result, "Usuarios obtenidos correctamente."));
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Obtener usuario por ID")]
    [EndpointDescription("Retorna el detalle completo de un usuario incluyendo persona, perfil y roles asignados.")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),  StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetUserByIdQuery(id), ct);
            return Ok(ApiResponse<UserDto>.Ok(result, "Usuario obtenido correctamente."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Error(ex.Message));
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Crear usuario")]
    [EndpointDescription("Crea la persona y la cuenta de usuario en una sola operación. La contraseña es hasheada con PBKDF2-SHA512.")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>),  StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        try
        {
            var dto = await mediator.Send(new CreateUserCommand(
                DocumentTypeId: request.DocumentTypeId,
                DocumentNumber: request.DocumentNumber,
                FirstName:      request.FirstName,
                LastName:       request.LastName,
                Email:          request.Email,
                PhoneNumber:    request.PhoneNumber,
                BirthDate:      request.BirthDate,
                Gender:         request.Gender,

                CreatedBy:      GetCurrentUserId() ?? 0
            ), ct);

            return CreatedAtAction(
                nameof(GetById),
                new { id = dto.Id },
                ApiResponse<UserDto>.Ok(dto, "Usuario creado correctamente.")
            );
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Error(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Actualizar usuario")]
    [EndpointDescription("Actualiza el username y los datos personales del usuario en una sola operación.")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),  StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>),  StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
    {
        try
        {
            var dto = await mediator.Send(new UpdateUserCommand(
                UserId:      id,
                Username:    request.Username,
                FirstName:   request.FirstName,
                LastName:    request.LastName,
                Email:       request.Email,
                PhoneNumber: request.PhoneNumber,
                BirthDate:   request.BirthDate,
                Gender:      request.Gender,
                UpdatedBy:   GetCurrentUserId() ?? 0
            ), ct);

            return Ok(ApiResponse<UserDto>.Ok(dto, "Usuario actualizado correctamente."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Error(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Error(ex.Message));
        }
    }

    [HttpPatch("{id:int}/deactivate")]
    [EndpointSummary("Desactivar usuario")]
    [EndpointDescription("Desactiva la cuenta. El usuario no podrá iniciar sesión mientras esté inactivo.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(
                new DeactivateUserCommand(id, UpdatedBy: GetCurrentUserId() ?? 0), ct);

            return Ok(ApiResponse<object>.Ok("Usuario desactivado correctamente."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Error(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
    }

    
    [HttpPatch("{id:int}/activate")]
    [EndpointSummary("Activar usuario")]
    [EndpointDescription("Reactiva una cuenta de usuario previamente desactivada.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(
                new ActivateUserCommand(id, UpdatedBy: GetCurrentUserId() ?? 0), ct);

            return Ok(ApiResponse<object>.Ok("Usuario activado correctamente."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Error(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}