using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Entities;

[Table("documenttypes", Schema = "auth")]
[Index("Code", Name = "uq_documenttypes_code", IsUnique = true)]
public partial class Documenttype
{
    [Key]
    [Column("documenttypeid")]
    public int Documenttypeid { get; set; }

    [Column("code")]
    [StringLength(10)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(80)]
    public string Name { get; set; } = null!;

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
    [InverseProperty("DocumenttypeCreatedbyNavigations")]
    public virtual User? CreatedbyNavigation { get; set; }

    [InverseProperty("Documenttype")]
    public virtual ICollection<Person> People { get; set; } = new List<Person>();

    [ForeignKey("Updatedby")]
    [InverseProperty("DocumenttypeUpdatedbyNavigations")]
    public virtual User? UpdatedbyNavigation { get; set; }
}
