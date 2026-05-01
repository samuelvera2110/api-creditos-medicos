using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Entities;

[Table("persons", Schema = "auth")]
[Index("Lastname", "Firstname", Name = "ix_persons_lastname_firstname")]
[Index("Documenttypeid", "Documentnumber", Name = "uq_persons_document", IsUnique = true)]
[Index("Email", Name = "uq_persons_email", IsUnique = true)]
public partial class Person
{
    [Key]
    [Column("personid")]
    public int Personid { get; set; }

    [Column("documenttypeid")]
    public int Documenttypeid { get; set; }

    [Column("documentnumber")]
    [StringLength(20)]
    public string Documentnumber { get; set; } = null!;

    [Column("firstname")]
    [StringLength(80)]
    public string Firstname { get; set; } = null!;

    [Column("lastname")]
    [StringLength(80)]
    public string Lastname { get; set; } = null!;

    [Column("birthdate")]
    public DateOnly? Birthdate { get; set; }

    [Column("gender")]
    [MaxLength(1)]
    public char? Gender { get; set; }

    [Column("email")]
    [StringLength(150)]
    public string Email { get; set; } = null!;

    [Column("phonenumber")]
    [StringLength(25)]
    public string? Phonenumber { get; set; }

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
    [InverseProperty("PersonCreatedbyNavigations")]
    public virtual User? CreatedbyNavigation { get; set; }

    [ForeignKey("Documenttypeid")]
    [InverseProperty("People")]
    public virtual Documenttype Documenttype { get; set; } = null!;

    [ForeignKey("Updatedby")]
    [InverseProperty("PersonUpdatedbyNavigations")]
    public virtual User? UpdatedbyNavigation { get; set; }

    [InverseProperty("Person")]
    public virtual User? User { get; set; }
}
