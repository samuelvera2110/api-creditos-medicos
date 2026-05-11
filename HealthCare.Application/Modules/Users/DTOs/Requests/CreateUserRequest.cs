namespace HealthCare.Application.Modules.Users.DTOs.Requests;

public sealed record CreateUserRequest(
    int       DocumentTypeId,
    string    DocumentNumber,
    string    FirstName,
    string    LastName,
    string    Email,
    string?   PhoneNumber,
    DateTime? BirthDate,
    char?     Gender,
    string    Username,
    string    Password
);