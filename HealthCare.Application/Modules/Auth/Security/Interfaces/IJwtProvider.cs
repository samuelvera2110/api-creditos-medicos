using HealthCare.Domain.Modules.Users.Entities;

namespace HealthCare.Application.Modules.Auth.Security.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user, IEnumerable<string> roles);
}