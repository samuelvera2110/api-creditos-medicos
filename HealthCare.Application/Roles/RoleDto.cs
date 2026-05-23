namespace HealthCare.Application.Roles;

public sealed record RoleDto(
    int       Id,
    string    Name,
    string?   Description,
    bool      IsActive,
    DateTime  CreatedAt,
    DateTime? UpdatedAt
);
