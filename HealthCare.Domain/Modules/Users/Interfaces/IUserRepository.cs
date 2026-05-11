using HealthCare.Domain.Modules.Users.Entities;

namespace HealthCare.Domain.Modules.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    
    Task<User?> GetByUsernameAsync(string username);
    
    Task<(IEnumerable<User> Items, int Total)> GetAllAsync(
        int page, int pageSize, bool? isActive, CancellationToken ct = default);

    
    Task<bool> ExistsByUsernameAsync(string username);
    
    Task<bool> ExistsByPersonIdAsync(int personId);
    
    Task AddAsync(User user);
    
    Task UpdateAsync(User user);
}