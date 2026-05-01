using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Entities;

[Table("profiles", Schema = "auth")]
[Index("Department", Name = "ix_profiles_department")]
[Index("Employeecode", Name = "uq_profiles_employeecode", IsUnique = true)]
[Index("Userid", Name = "uq_profiles_userid", IsUnique = true)]
public partial class Profile
{
    [Key]
    [Column("profileid")]
    public int Profileid { get; set; }

    [Column("userid")]
    public int Userid { get; set; }

    [Column("employeecode")]
    [StringLength(30)]
    public string Employeecode { get; set; } = null!;

    [Column("jobtitle")]
    [StringLength(100)]
    public string? Jobtitle { get; set; }

    [Column("department")]
    [StringLength(100)]
    public string? Department { get; set; }

    [Column("hiredate")]
    public DateOnly? Hiredate { get; set; }

    [Column("basesalary")]
    [Precision(12, 2)]
    public decimal? Basesalary { get; set; }

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
    [InverseProperty("ProfileCreatedbyNavigations")]
    public virtual User? CreatedbyNavigation { get; set; }

    [ForeignKey("Updatedby")]
    [InverseProperty("ProfileUpdatedbyNavigations")]
    public virtual User? UpdatedbyNavigation { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("ProfileUser")]
    public virtual User User { get; set; } = null!;
}
