namespace HealthCare.Application.Modules.Auth.DTOs.Requests;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);