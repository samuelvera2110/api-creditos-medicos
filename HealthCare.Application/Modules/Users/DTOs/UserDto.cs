namespace HealthCare.Application.Modules.Users.DTOs;

public sealed record UserDto(
    int      Id,
    int      PersonId,
    string   Username,
    string   FirstName,
    string   LastName,
    string   Email,
    bool     MustChangePassword,
    bool     IsActive,
    DateTime CreatedAt,
    IEnumerable<string> Roles,
    string?  TemporaryPassword = null 
);