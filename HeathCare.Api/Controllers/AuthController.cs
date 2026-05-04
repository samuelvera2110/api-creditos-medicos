using HealthCare.Application.Modules.Auth.DTOs;
using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Shared.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace HeathCare.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Authentication")]
public class AuthController(IAuthService authService) : ControllerBase 
{
    [HttpPost("login")]
    [EndpointSummary("Autenticación de Usuarios")]
    [EndpointDescription("Valida credenciales, verifica el hash y retorna un token JWT firmado junto con la información del perfil.")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        var response = ApiResponse<AuthResponse>.Ok(result, "Sesión iniciada correctamente.");
        
        return Ok(response);
    }
}