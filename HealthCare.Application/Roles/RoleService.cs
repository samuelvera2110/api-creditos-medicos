using HealthCare.Domain.Modules.Role.Interfaces;
using HealthCare.Shared.Common;
using HealthCare.Shared.Exceptions;

using DomainRole = HealthCare.Domain.Modules.Role.Entities.Role;

namespace HealthCare.Application.Roles;

public interface IRoleService
{
    Task<PagedResult<RoleDto>> GetAllAsync(int page, int pageSize, bool? isActive, CancellationToken ct);
    Task<RoleDto> GetByIdAsync(int id, CancellationToken ct);
    Task<RoleDto> CreateAsync(CreateRoleRequest request, int? createdBy, CancellationToken ct);
    Task<RoleDto> UpdateAsync(int id, UpdateRoleRequest request, int? updatedBy, CancellationToken ct);
    Task ActivateAsync(int id, int? updatedBy, CancellationToken ct);
    Task DeactivateAsync(int id, int? updatedBy, CancellationToken ct);
}

public sealed class RoleService(IRoleRepository roleRepository) : IRoleService
{
    public async Task<PagedResult<RoleDto>> GetAllAsync(
        int page, int pageSize, bool? isActive, CancellationToken ct)
    {
        var p    = Math.Max(1, page);
        var size = Math.Clamp(pageSize, 1, 100);
        var (items, total) = await roleRepository.GetAllAsync(p, size, isActive, ct);
        return new PagedResult<RoleDto>(items.Select(ToDto), total, p, size);
    }

    public async Task<RoleDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var role = await roleRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Rol {id} no encontrado.");
        return ToDto(role);
    }

    public async Task<RoleDto> CreateAsync(
        CreateRoleRequest request, int? createdBy, CancellationToken ct)
    {
        if (await roleRepository.ExistsByNameAsync(request.Name))
            throw new AppExceptions.ConflictException($"El rol '{request.Name}' ya existe.");

        var role = new DomainRole(request.Name, request.Description, createdBy);
        await roleRepository.AddAsync(role);

        var created = await roleRepository.GetByNameAsync(request.Name)
                      ?? throw new AppExceptions.BadRequestException("Error al crear el rol.");
        return ToDto(created);
    }

    public async Task<RoleDto> UpdateAsync(
        int id, UpdateRoleRequest request, int? updatedBy, CancellationToken ct)
    {
        var role = await roleRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Rol {id} no encontrado.");

        if (await roleRepository.ExistsByNameAsync(request.Name, id))
            throw new AppExceptions.ConflictException($"El rol '{request.Name}' ya existe.");

        role.UpdateDetails(request.Name, request.Description, updatedBy);
        await roleRepository.UpdateAsync(role);
        return ToDto(role);
    }

    public async Task ActivateAsync(int id, int? updatedBy, CancellationToken ct)
    {
        var role = await roleRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Rol {id} no encontrado.");

        if (role.IsActive)
            throw new AppExceptions.BadRequestException("El rol ya está activo.");

        role.Activate(updatedBy);
        await roleRepository.UpdateAsync(role);
    }

    public async Task DeactivateAsync(int id, int? updatedBy, CancellationToken ct)
    {
        var role = await roleRepository.GetByIdAsync(id)
                   ?? throw new AppExceptions.NotFoundException($"Rol {id} no encontrado.");

        if (!role.IsActive)
            throw new AppExceptions.BadRequestException("El rol ya está inactivo.");

        role.Deactivate(updatedBy);
        await roleRepository.UpdateAsync(role);
    }

    private static RoleDto ToDto(DomainRole r) =>
        new(r.Id, r.Name, r.Description, r.IsActive, r.CreatedAt, r.UpdatedAt);
}
