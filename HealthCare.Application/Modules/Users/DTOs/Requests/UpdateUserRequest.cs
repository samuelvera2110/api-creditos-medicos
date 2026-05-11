namespace HealthCare.Application.Modules.Users.DTOs.Requests;

public sealed record UpdateUserRequest(
    string    Username,
    string    FirstName,
    string    LastName,
    string    Email,
    string?   PhoneNumber,
    DateTime? BirthDate,
    char?     Gender
);