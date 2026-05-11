using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Domain.Modules.Users;
using MediatR;

namespace HealthCare.Application.Modules.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher
) : IRequestHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
                   ?? throw new KeyNotFoundException($"Usuario {request.UserId} no encontrado.");

        if (!user.IsActive)
            throw new InvalidOperationException("La cuenta está inactiva.");

        if (!passwordHasher.VerifyPasswordHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        passwordHasher.CreatePasswordHash(request.NewPassword, out byte[] newHash, out byte[] newSalt);

        user.ChangePassword(newHash, newSalt);

        await userRepository.UpdateAsync(user);

        return Unit.Value;
    }
}