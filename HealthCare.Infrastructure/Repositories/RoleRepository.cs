using Microsoft.EntityFrameworkCore;
using HealthCare.Domain.Modules.Role.Interfaces;
using HealthCare.Infrastructure.Persistence.Context;

using DomainRole = HealthCare.Domain.Modules.Role.Entities.Role;
using InfraRole = HealthCare.Infrastructure.Persistence.Entities.Role;

namespace HealthCare.Infrastructure.Repositories;

public class RoleRepository(HeathCareDbContext context) : IRoleRepository
{
    public async Task<DomainRole?> GetByIdAsync(int id)
    {
        var dbRole = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Roleid == id);

        return dbRole != null ? MapToDomain(dbRole) : null;
    }

    public async Task<DomainRole?> GetByNameAsync(string name)
    {
        var dbRole = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == name);

        return dbRole != null ? MapToDomain(dbRole) : null;
    }

    public async Task<(IEnumerable<DomainRole> Items, int Total)> GetAllAsync(
        int page, int pageSize, bool? isActive, CancellationToken ct = default)
    {
        var query = context.Roles.AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(r => r.Isactive == isActive.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(r => r.Roleid)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items.Select(MapToDomain), total);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await context.Roles.AnyAsync(r => r.Name == name);
    }

    public async Task<bool> ExistsByNameAsync(string name, int excludeId)
    {
        return await context.Roles.AnyAsync(r => r.Name == name && r.Roleid != excludeId);
    }

    public async Task AddAsync(DomainRole role)
    {
        var entity = new InfraRole
        {
            Name        = role.Name,
            Description = role.Description,
            Isactive    = role.IsActive,
            Createdat   = role.CreatedAt,
            Createdby   = role.CreatedBy
        };

        await context.Roles.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DomainRole role)
    {
        var entity = await context.Roles.FirstOrDefaultAsync(r => r.Roleid == role.Id);

        if (entity != null)
        {
            entity.Name        = role.Name;
            entity.Description = role.Description;
            entity.Isactive    = role.IsActive;
            entity.Updatedat   = role.UpdatedAt;
            entity.Updatedby   = role.UpdatedBy;

            context.Roles.Update(entity);
            await context.SaveChangesAsync();
        }
    }

    private static DomainRole MapToDomain(InfraRole infraRole) =>
        DomainRole.Reconstitute(
            id: infraRole.Roleid,
            name: infraRole.Name,
            description: infraRole.Description,
            isActive: infraRole.Isactive,
            createdAt: infraRole.Createdat,
            createdBy: infraRole.Createdby,
            updatedAt: infraRole.Updatedat,
            updatedBy: infraRole.Updatedby
        );
}
