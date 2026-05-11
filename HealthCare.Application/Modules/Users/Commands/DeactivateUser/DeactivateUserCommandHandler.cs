using HealthCare.Domain.Modules.Users;
using MediatR;

namespace HealthCare.Application.Modules.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<DeactivateUserCommand, Unit>
{
    public async Task<Unit> Handle(DeactivateUserCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
                   ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        if (!user.IsActive)
            throw new InvalidOperationException("User is already inactive.");

        user.Deactivate(request.UpdatedBy);
        await userRepository.UpdateAsync(user);
        return Unit.Value;
    }
}