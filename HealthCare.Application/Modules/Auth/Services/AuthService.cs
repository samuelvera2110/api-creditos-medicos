using HealthCare.Application.Modules.Auth.DTOs;
using HealthCare.Application.Modules.Auth.Security.Interfaces;
using HealthCare.Domain.Modules.Users;
using HealthCare.Shared.Exceptions;

namespace HealthCare.Application.Modules.Auth.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username);
        
        if (user == null || !user.IsActive)
        {
            throw new AppExceptions.UnauthorizedException("Usuario o contraseña incorrectos.");
        }

        var isPasswordValid = passwordHasher.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt);
        
        if (!isPasswordValid)
        {
            throw new AppExceptions.UnauthorizedException("Usuario o contraseña incorrectos.");
        }

        var roles = user.Roles.Select(r => r.Name).ToList();
        var token = jwtProvider.GenerateToken(user, roles);
        var fullName = $"{user.Person?.FirstName} {user.Person?.LastName}".Trim();

        return new AuthResponse(token, user.Username, fullName);
    }
}