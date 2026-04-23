using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Entities;

[Table("DocumentTypes", Schema = "auth")]
[Index("Code", Name = "UQ_DocumentTypes_Code", IsUnique = true)]
public partial class DocumentType
{
    [Key]
    public int DocumentTypeId { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string Code { get; set; } = null!;

    [StringLength(80)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("DocumentTypeCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("DocumentType")]
    public virtual ICollection<Person> People { get; set; } = new List<Person>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("DocumentTypeUpdatedByNavigations")]
    public virtual User? UpdatedByNavigation { get; set; }
}
