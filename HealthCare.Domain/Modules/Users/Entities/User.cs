namespace HealthCare.Domain.Modules.Users.Entities;

public class User
{
    public int Id { get; private set; }
    public int PersonId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public byte[] PasswordHash { get; private set; } = [];
    public byte[] PasswordSalt { get; private set; } = [];
    public DateTime? PasswordChangedAt { get; private set; }
    public bool MustChangePassword { get; private set; }
    
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEndUtc { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? LastLoginIp { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public int? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int? UpdatedBy { get; private set; }
    public bool IsActive { get; private set; }
    
    public Person.Entities.Person? Person { get; private set; }
    public Profile.Entities.Profile? Profile { get; private set; }
    
    private readonly List<Role.Entities.Role> _roles = new();
    public IReadOnlyCollection<Role.Entities.Role> Roles => _roles.AsReadOnly();

    protected User() { }

    public User(int personId, string username, byte[] passwordHash, byte[] passwordSalt)
    {
        PersonId = personId;
        Username = username;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        MustChangePassword = true;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }


    public void RecordSuccessfulLogin(string ipAddress)
    {
        FailedLoginAttempts = 0;
        LockoutEndUtc = null;
        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
    }

    public void RecordFailedLogin(int maxAttempts, int lockoutMinutes)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockoutEndUtc = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }
    }

    public bool IsLockedOut()
    {
        return LockoutEndUtc.HasValue && LockoutEndUtc.Value > DateTime.UtcNow;
    }

    public void UpdatePassword(byte[] newHash, byte[] newSalt)
    {
        PasswordHash = newHash;
        PasswordSalt = newSalt;
        PasswordChangedAt = DateTime.UtcNow;
        MustChangePassword = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void AddRole(Role.Entities.Role role) => _roles.Add(role);

    internal static User Reconstitute(
        int id, int personId, string username,
        byte[] passwordHash, byte[] passwordSalt,
        int failedLoginAttempts, DateTime? lockoutEndUtc,
        DateTime? lastLoginAt, string? lastLoginIp,
        DateTime? passwordChangedAt, bool mustChangePassword,
        bool isActive, DateTime createdAt, int? createdBy,
        DateTime? updatedAt, int? updatedBy)
    {
        var user = new User();
        user.Id = id;
        user.PersonId = personId;
        user.Username = username;
        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        user.FailedLoginAttempts = failedLoginAttempts;
        user.LockoutEndUtc = lockoutEndUtc;
        user.LastLoginAt = lastLoginAt;
        user.LastLoginIp = lastLoginIp;
        user.PasswordChangedAt = passwordChangedAt;
        user.MustChangePassword = mustChangePassword;
        user.IsActive = isActive;
        user.CreatedAt = createdAt;
        user.CreatedBy = createdBy;
        user.UpdatedAt = updatedAt;
        user.UpdatedBy = updatedBy;
        return user;
    }

    internal void SetPerson(Person.Entities.Person person) => Person = person;
}