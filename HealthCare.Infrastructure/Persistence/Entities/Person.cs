using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Entities;

[Table("Persons", Schema = "auth")]
[Index("LastName", "FirstName", Name = "IX_Persons_LastName_FirstName")]
[Index("DocumentTypeId", "DocumentNumber", Name = "UQ_Persons_Document", IsUnique = true)]
[Index("Email", Name = "UQ_Persons_Email", IsUnique = true)]
public partial class Person
{
    [Key]
    public int PersonId { get; set; }

    public int DocumentTypeId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string DocumentNumber { get; set; } = null!;

    [StringLength(80)]
    [Unicode(false)]
    public string FirstName { get; set; } = null!;

    [StringLength(80)]
    [Unicode(false)]
    public string LastName { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? Gender { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(25)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("PersonCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("People")]
    public virtual DocumentType DocumentType { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("PersonUpdatedByNavigations")]
    public virtual User? UpdatedByNavigation { get; set; }

    [InverseProperty("Person")]
    public virtual User? User { get; set; }
}
