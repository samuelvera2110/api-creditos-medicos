using System.Security.Cryptography;
using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Domain.Modules.Users;
using MediatR;

namespace HealthCare.Application.Modules.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher
) : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private const int TempPasswordLength = 12;

    public async Task<ResetPasswordResult> Handle(
        ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.TargetUserId)
            ?? throw new KeyNotFoundException(
                $"Usuario {request.TargetUserId} no encontrado.");

        if (!user.IsActive)
            throw new InvalidOperationException("No se puede resetear una cuenta inactiva.");

        if (user.Id == request.RequestedBy)
            throw new InvalidOperationException(
                "Use el flujo de cambio de contraseña para su propia cuenta.");

        var tempPassword = GenerateTemporaryPassword();
        
        // Declaración inline de las variables out
        passwordHasher.CreatePasswordHash(tempPassword, out byte[] tempHash, out byte[] tempSalt);

        user.ResetPassword(tempHash, tempSalt, request.RequestedBy);

        await userRepository.UpdateAsync(user);

        return new ResetPasswordResult(tempPassword);
    }

    /// <summary>
    /// Genera una contraseña temporal segura que cumple las reglas de validación:
    /// al menos 1 mayúscula, 1 número, 1 especial.
    /// </summary>
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