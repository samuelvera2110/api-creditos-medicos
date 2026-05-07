namespace HealthCare.Domain.Modules.Role.Entities;

public class Role
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    protected Role() { } 

    public Role(string name, string? description)
    {
        Name = name;
        Description = description;
        IsActive = true;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void ToggleActiveStatus(bool status)
    {
        IsActive = status;
    }

    internal static Role Reconstitute(int id, string name, string? description, bool isActive)
    {
        var role = new Role();
        role.Id = id;
        role.Name = name;
        role.Description = description;
        role.IsActive = isActive;
        return role;
    }
}