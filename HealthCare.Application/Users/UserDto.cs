namespace HealthCare.Application.Users;

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
