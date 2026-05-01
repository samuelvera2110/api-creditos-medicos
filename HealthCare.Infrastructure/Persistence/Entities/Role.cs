using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Entities;

[Table("roles", Schema = "auth")]
[Index("Name", Name = "uq_roles_name", IsUnique = true)]
public partial class Role
{
    [Key]
    [Column("roleid")]
    public int Roleid { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(250)]
    public string? Description { get; set; }

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
    [InverseProperty("RoleCreatedbyNavigations")]
    public virtual User? CreatedbyNavigation { get; set; }

    [ForeignKey("Updatedby")]
    [InverseProperty("RoleUpdatedbyNavigations")]
    public virtual User? UpdatedbyNavigation { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<Userrole> Userroles { get; set; } = new List<Userrole>();
}
