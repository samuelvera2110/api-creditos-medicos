namespace HealthCare.Application.Modules.Auth.DTOs.Requests;

public sealed record ResetPasswordRequest(
    int TargetUserId
);