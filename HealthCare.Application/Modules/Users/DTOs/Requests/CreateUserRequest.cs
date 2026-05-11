namespace HealthCare.Application.Modules.Users.DTOs.Requests;

public sealed record CreateUserRequest(int PersonId, string Username, string Password);
