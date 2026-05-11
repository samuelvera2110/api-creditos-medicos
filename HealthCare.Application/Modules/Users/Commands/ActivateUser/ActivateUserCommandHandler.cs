using HealthCare.Domain.Modules.Users;
using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.ActivateUser;
public sealed class ActivateUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<ActivateUserCommand, Unit>
{
    public async Task<Unit> Handle(ActivateUserCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
                   ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        if (user.IsActive)
            throw new InvalidOperationException("User is already active.");

        user.Activate(request.UpdatedBy);
        await userRepository.UpdateAsync(user);
        return Unit.Value;
    }
}
