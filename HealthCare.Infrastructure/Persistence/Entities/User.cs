using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Entities;

[Table("users", Schema = "auth")]
[Index("Isactive", Name = "ix_users_isactive")]
[Index("Personid", Name = "uq_users_personid", IsUnique = true)]
[Index("Username", Name = "uq_users_username", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("userid")]
    public int Userid { get; set; }

    [Column("personid")]
    public int Personid { get; set; }

    [Column("username")]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Column("passwordhash")]
    public byte[] Passwordhash { get; set; } = null!;

    [Column("passwordsalt")]
    public byte[] Passwordsalt { get; set; } = null!;

    [Column("passwordchangedat", TypeName = "timestamp(0) without time zone")]
    public DateTime? Passwordchangedat { get; set; }

    [Column("mustchangepassword")]
    public bool Mustchangepassword { get; set; }

    [Column("failedloginattempts")]
    public int Failedloginattempts { get; set; }

    [Column("lockoutendutc", TypeName = "timestamp(0) without time zone")]
    public DateTime? Lockoutendutc { get; set; }

    [Column("lastloginat", TypeName = "timestamp(0) without time zone")]
    public DateTime? Lastloginat { get; set; }

    [Column("lastloginip")]
    [StringLength(45)]
    public string? Lastloginip { get; set; }

    [Column("createdat", TypeName = "timestamp(0) without time zone")]
    public DateTime Createdat { get; set; }

    [Column("createdby")]
    public int? Createdby { get; set; }

    [Column("updatedat", TypeName = "timestamp(0) without time zone")]
    public DateTime? Updatedat { get; set; }

    [Column("updatedby")]
    public int? Updatedby { get; set; }

    [Column("isactive")]
    public bool Isactive { get; set; }

    [ForeignKey("Createdby")]
    [InverseProperty("InverseCreatedbyNavigation")]
    public virtual User? CreatedbyNavigation { get; set; }

    [InverseProperty("CreatedbyNavigation")]
    public virtual ICollection<Documenttype> DocumenttypeCreatedbyNavigations { get; set; } = new List<Documenttype>();

    [InverseProperty("UpdatedbyNavigation")]
    public virtual ICollection<Documenttype> DocumenttypeUpdatedbyNavigations { get; set; } = new List<Documenttype>();

    [InverseProperty("CreatedbyNavigation")]
    public virtual ICollection<User> InverseCreatedbyNavigation { get; set; } = new List<User>();

    [InverseProperty("UpdatedbyNavigation")]
    public virtual ICollection<User> InverseUpdatedbyNavigation { get; set; } = new List<User>();

    [ForeignKey("Personid")]
    [InverseProperty("User")]
    public virtual Person Person { get; set; } = null!;

    [InverseProperty("CreatedbyNavigation")]
    public virtual ICollection<Person> PersonCreatedbyNavigations { get; set; } = new List<Person>();

    [InverseProperty("UpdatedbyNavigation")]
    public virtual ICollection<Person> PersonUpdatedbyNavigations { get; set; } = new List<Person>();

    [InverseProperty("CreatedbyNavigation")]
    public virtual ICollection<Profile> ProfileCreatedbyNavigations { get; set; } = new List<Profile>();

    [InverseProperty("UpdatedbyNavigation")]
    public virtual ICollection<Profile> ProfileUpdatedbyNavigations { get; set; } = new List<Profile>();

    [InverseProperty("User")]
    public virtual Profile? ProfileUser { get; set; }

    [InverseProperty("CreatedbyNavigation")]
    public virtual ICollection<Role> RoleCreatedbyNavigations { get; set; } = new List<Role>();

    [InverseProperty("UpdatedbyNavigation")]
    public virtual ICollection<Role> RoleUpdatedbyNavigations { get; set; } = new List<Role>();

    [ForeignKey("Updatedby")]
    [InverseProperty("InverseUpdatedbyNavigation")]
    public virtual User? UpdatedbyNavigation { get; set; }

    [InverseProperty("CreatedbyNavigation")]
    public virtual ICollection<Userrole> UserroleCreatedbyNavigations { get; set; } = new List<Userrole>();

    [InverseProperty("UpdatedbyNavigation")]
    public virtual ICollection<Userrole> UserroleUpdatedbyNavigations { get; set; } = new List<Userrole>();

    [InverseProperty("User")]
    public virtual ICollection<Userrole> UserroleUsers { get; set; } = new List<Userrole>();
}
