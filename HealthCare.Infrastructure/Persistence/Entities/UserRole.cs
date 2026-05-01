using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Entities;

[PrimaryKey("Userid", "Roleid")]
[Table("userroles", Schema = "auth")]
public partial class Userrole
{
    [Key]
    [Column("userid")]
    public int Userid { get; set; }

    [Key]
    [Column("roleid")]
    public int Roleid { get; set; }

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
    [InverseProperty("UserroleCreatedbyNavigations")]
    public virtual User? CreatedbyNavigation { get; set; }

    [ForeignKey("Roleid")]
    [InverseProperty("Userroles")]
    public virtual Role Role { get; set; } = null!;

    [ForeignKey("Updatedby")]
    [InverseProperty("UserroleUpdatedbyNavigations")]
    public virtual User? UpdatedbyNavigation { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("UserroleUsers")]
    public virtual User User { get; set; } = null!;
}
