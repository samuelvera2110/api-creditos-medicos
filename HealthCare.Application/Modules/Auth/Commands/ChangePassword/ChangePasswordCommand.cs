using MediatR;

namespace HealthCare.Application.Modules.Auth.Commands.ChangePassword;


public sealed record ChangePasswordCommand(
    int    UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest<Unit>;