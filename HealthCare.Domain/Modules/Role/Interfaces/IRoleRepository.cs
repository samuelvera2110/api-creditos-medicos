using DomainRole = HealthCare.Domain.Modules.Role.Entities.Role;

namespace HealthCare.Domain.Modules.Role.Interfaces;

public interface IRoleRepository
{
    Task<DomainRole?> GetByIdAsync(int id);

    Task<DomainRole?> GetByNameAsync(string name);

    Task<(IEnumerable<DomainRole> Items, int Total)> GetAllAsync(
        int page, int pageSize, bool? isActive, CancellationToken ct = default);

    Task<bool> ExistsByNameAsync(string name);

    Task<bool> ExistsByNameAsync(string name, int excludeId);

    Task AddAsync(DomainRole role);

    Task UpdateAsync(DomainRole role);
}
