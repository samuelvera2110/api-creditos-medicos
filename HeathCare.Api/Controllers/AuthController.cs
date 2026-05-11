using System.Security.Claims;
using HealthCare.Application.Modules.Auth.Commands.ChangePassword;
using HealthCare.Application.Modules.Auth.Commands.ResetPassword;
using HealthCare.Application.Modules.Auth.DTOs;
using HealthCare.Application.Modules.Auth.DTOs.Requests;
using HealthCare.Application.Modules.Auth.DTOs.Responses;
using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Shared.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeathCare.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Authentication")]
public sealed class AuthController(
    IAuthService authService,
    IMediator    mediator
) : ControllerBase
{
    
    [HttpPost("login")]
    [EndpointSummary("Autenticación de usuarios")]
    [EndpointDescription("Valida credenciales, verifica el hash y retorna un token JWT firmado junto con la información del perfil.")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),       StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>),       StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip     = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await authService.LoginAsync(request with { IpAddress = ip });

        return Ok(ApiResponse<AuthResponse>.Ok(result, "Sesión iniciada correctamente."));
    }

    [HttpPost("change-password")]
    [Authorize]
    [EndpointSummary("Cambiar contraseña")]
    [EndpointDescription("El usuario autenticado cambia su propia contraseña verificando la contraseña actual.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized(ApiResponse<object>.Error("No se pudo identificar al usuario autenticado."));

        try
        {
            await mediator.Send(new ChangePasswordCommand(
                UserId:             userId.Value,
                CurrentPassword:    request.CurrentPassword,
                NewPassword:        request.NewPassword,
                ConfirmNewPassword: request.ConfirmNewPassword
            ));

            return Ok(ApiResponse<object>.Ok("Contraseña actualizada correctamente."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Error(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
    }

    [HttpPost("reset-password")]
    [Authorize(Roles = "Admin,HR")]
    [EndpointSummary("Resetear contraseña")]
    [EndpointDescription("Admin o RR.HH. resetea la contraseña de un usuario. Se genera una contraseña temporal y el usuario deberá cambiarla en su próximo inicio de sesión.")]
    [ProducesResponseType(typeof(ApiResponse<ResetPasswordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),                StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>),                StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var requestedBy = GetCurrentUserId();
        if (requestedBy is null)
            return Unauthorized(ApiResponse<object>.Error("No se pudo identificar al usuario autenticado."));

        try
        {
            var result = await mediator.Send(new ResetPasswordCommand(
                TargetUserId: request.TargetUserId,
                RequestedBy:  requestedBy.Value
            ));

            // TODO: en producción enviar por email y no retornar la contraseña aquí
            return Ok(ApiResponse<ResetPasswordResponse>.Ok(
                new ResetPasswordResponse(result.TemporaryPassword),
                "Contraseña reseteada. El usuario deberá cambiarla en su próximo inicio de sesión."
            ));
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