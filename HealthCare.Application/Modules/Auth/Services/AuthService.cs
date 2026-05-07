using HealthCare.Application.Modules.Auth.DTOs;
using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Domain.Modules.Users;
using HealthCare.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Modules.Auth.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider,
    ILogger<AuthService> logger) : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username);

        if (user == null || !user.IsActive)
        {
            logger.LogWarning("Login fallido: usuario '{Username}' no encontrado o inactivo", request.Username);
            throw new AppExceptions.UnauthorizedException("Usuario o contraseña incorrectos.");
        }

        if (user.IsLockedOut())
        {
            logger.LogWarning("Login bloqueado: usuario '{Username}' tiene la cuenta bloqueada hasta {LockoutEnd}",
                request.Username, user.LockoutEndUtc);
            throw new AppExceptions.UnauthorizedException("La cuenta está bloqueada temporalmente. Intente más tarde.");
        }

        var isPasswordValid = passwordHasher.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt);

        if (!isPasswordValid)
        {
            user.RecordFailedLogin(MaxFailedAttempts, LockoutMinutes);
            await userRepository.UpdateAsync(user);
            logger.LogWarning("Login fallido: contraseña incorrecta para '{Username}'. Intentos: {Attempts}",
                request.Username, user.FailedLoginAttempts);
            throw new AppExceptions.UnauthorizedException("Usuario o contraseña incorrectos.");
        }

        user.RecordSuccessfulLogin(request.IpAddress ?? "unknown");
        await userRepository.UpdateAsync(user);

        logger.LogInformation("Login exitoso: usuario '{Username}' desde IP {Ip}", request.Username, request.IpAddress);

        var roles = user.Roles.Select(r => r.Name).ToList();
        var token = jwtProvider.GenerateToken(user, roles);
        var fullName = $"{user.Person?.FirstName} {user.Person?.LastName}".Trim();

        return new AuthResponse(token, user.Username, fullName);
    }
}