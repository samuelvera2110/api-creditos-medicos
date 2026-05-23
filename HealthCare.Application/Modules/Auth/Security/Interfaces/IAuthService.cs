using HealthCare.Application.Modules.Auth.DTOs;
using HealthCare.Application.Modules.Auth.DTOs.Requests;

namespace HealthCare.Application.Modules.Auth.Security.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct);
    Task<string> ResetPasswordAsync(int targetUserId, int requestedBy, CancellationToken ct);
}