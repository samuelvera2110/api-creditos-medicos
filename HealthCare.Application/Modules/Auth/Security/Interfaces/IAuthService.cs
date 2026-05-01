using HealthCare.Application.Modules.Auth.DTOs;
using HealthCare.Shared.Wrappers;

namespace HealthCare.Application.Modules.Auth.Security.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
}