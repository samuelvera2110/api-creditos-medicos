namespace HealthCare.Domain.Modules.Role.Entities;

public class Role
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public int? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int? UpdatedBy { get; private set; }

    protected Role() { }

    public Role(string name, string? description, int? createdBy)
    {
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    public void UpdateDetails(string name, string? description, int? updatedBy)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Activate(int? updatedBy)
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Deactivate(int? updatedBy)
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    internal static Role Reconstitute(
        int id, string name, string? description, bool isActive,
        DateTime createdAt, int? createdBy, DateTime? updatedAt, int? updatedBy)
    {
        var role = new Role();
        role.Id = id;
        role.Name = name;
        role.Description = description;
        role.IsActive = isActive;
        role.CreatedAt = createdAt;
        role.CreatedBy = createdBy;
        role.UpdatedAt = updatedAt;
        role.UpdatedBy = updatedBy;
        return role;
    }
}
