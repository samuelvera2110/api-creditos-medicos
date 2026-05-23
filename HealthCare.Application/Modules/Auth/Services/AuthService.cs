using System.Security.Cryptography;
using HealthCare.Application.Modules.Auth.DTOs;
using HealthCare.Application.Modules.Auth.DTOs.Requests;
using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Domain.Modules.Users;
using HealthCare.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Modules.Auth.Services;

public class AuthService(
    IUserRepository      userRepository,
    IPasswordHasher      passwordHasher,
    IJwtProvider         jwtProvider,
    ILogger<AuthService> logger) : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes    = 15;
    private const int TempPasswordLength = 12;

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

        if (!passwordHasher.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
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

        var roles    = user.Roles.Select(r => r.Name).ToList();
        var token    = jwtProvider.GenerateToken(user, roles);
        var fullName = $"{user.Person?.FirstName} {user.Person?.LastName}".Trim();

        return new AuthResponse(token, user.Username, fullName);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(userId)
                   ?? throw new AppExceptions.NotFoundException($"Usuario {userId} no encontrado.");

        if (!user.IsActive)
            throw new AppExceptions.BadRequestException("La cuenta está inactiva.");

        if (!passwordHasher.VerifyPasswordHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            throw new AppExceptions.UnauthorizedException("La contraseña actual es incorrecta.");

        passwordHasher.CreatePasswordHash(request.NewPassword, out byte[] newHash, out byte[] newSalt);
        user.ChangePassword(newHash, newSalt);

        await userRepository.UpdateAsync(user);
    }

    public async Task<string> ResetPasswordAsync(int targetUserId, int requestedBy, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(targetUserId)
                   ?? throw new AppExceptions.NotFoundException($"Usuario {targetUserId} no encontrado.");

        if (!user.IsActive)
            throw new AppExceptions.BadRequestException("No se puede resetear una cuenta inactiva.");

        if (user.Id == requestedBy)
            throw new AppExceptions.BadRequestException(
                "Use el flujo de cambio de contraseña para su propia cuenta.");

        var tempPassword = GenerateTemporaryPassword();
        passwordHasher.CreatePasswordHash(tempPassword, out byte[] tempHash, out byte[] tempSalt);
        user.ResetPassword(tempHash, tempSalt, requestedBy);

        await userRepository.UpdateAsync(user);

        return tempPassword;
    }

    private static string GenerateTemporaryPassword()
    {
        const string upper   = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower   = "abcdefghijklmnopqrstuvwxyz";
        const string digits  = "0123456789";
        const string special = "!@#$%^&*";
        const string all     = upper + lower + digits + special;

        var chars = new char[TempPasswordLength];
        chars[0] = upper  [RandomNumberGenerator.GetInt32(upper.Length)];
        chars[1] = digits [RandomNumberGenerator.GetInt32(digits.Length)];
        chars[2] = special[RandomNumberGenerator.GetInt32(special.Length)];
        for (int i = 3; i < TempPasswordLength; i++)
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];

        return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(100)).ToArray());
    }
}
