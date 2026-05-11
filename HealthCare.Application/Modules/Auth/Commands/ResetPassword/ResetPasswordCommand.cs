using MediatR;

namespace HealthCare.Application.Modules.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    int TargetUserId,
    int RequestedBy          
) : IRequest<ResetPasswordResult>;

public sealed record ResetPasswordResult(
    string TemporaryPassword
);