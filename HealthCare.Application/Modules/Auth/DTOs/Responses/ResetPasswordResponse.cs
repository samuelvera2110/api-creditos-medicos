namespace HealthCare.Application.Modules.Auth.DTOs.Responses;

public sealed record ResetPasswordResponse(
    string TemporaryPassword
);