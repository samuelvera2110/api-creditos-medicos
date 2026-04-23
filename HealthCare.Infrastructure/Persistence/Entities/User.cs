using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Entities;

[Table("Users", Schema = "auth")]
[Index("IsActive", Name = "IX_Users_IsActive")]
[Index("PersonId", Name = "UQ_Users_PersonId", IsUnique = true)]
[Index("Username", Name = "UQ_Users_Username", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    public int PersonId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [MaxLength(256)]
    public byte[] PasswordHash { get; set; } = null!;

    [MaxLength(128)]
    public byte[] PasswordSalt { get; set; } = null!;

    [Precision(0)]
    public DateTime? PasswordChangedAt { get; set; }

    public bool MustChangePassword { get; set; }

    public int FailedLoginAttempts { get; set; }

    [Precision(0)]
    public DateTime? LockoutEndUtc { get; set; }

    [Precision(0)]
    public DateTime? LastLoginAt { get; set; }

    [StringLength(45)]
    [Unicode(false)]
    public string? LastLoginIp { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("InverseCreatedByNavigation")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<DocumentType> DocumentTypeCreatedByNavigations { get; set; } = new List<DocumentType>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<DocumentType> DocumentTypeUpdatedByNavigations { get; set; } = new List<DocumentType>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<User> InverseCreatedByNavigation { get; set; } = new List<User>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<User> InverseUpdatedByNavigation { get; set; } = new List<User>();

    [ForeignKey("PersonId")]
    [InverseProperty("User")]
    public virtual Person Person { get; set; } = null!;

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Person> PersonCreatedByNavigations { get; set; } = new List<Person>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Person> PersonUpdatedByNavigations { get; set; } = new List<Person>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Profile> ProfileCreatedByNavigations { get; set; } = new List<Profile>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Profile> ProfileUpdatedByNavigations { get; set; } = new List<Profile>();

    [InverseProperty("User")]
    public virtual Profile? ProfileUser { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Role> RoleCreatedByNavigations { get; set; } = new List<Role>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Role> RoleUpdatedByNavigations { get; set; } = new List<Role>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("InverseUpdatedByNavigation")]
    public virtual User? UpdatedByNavigation { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<UserRole> UserRoleCreatedByNavigations { get; set; } = new List<UserRole>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<UserRole> UserRoleUpdatedByNavigations { get; set; } = new List<UserRole>();

    [InverseProperty("User")]
    public virtual ICollection<UserRole> UserRoleUsers { get; set; } = new List<UserRole>();
}
