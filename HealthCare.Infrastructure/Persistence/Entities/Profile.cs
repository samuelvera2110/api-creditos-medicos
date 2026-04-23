using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Entities;

[Table("Profiles", Schema = "auth")]
[Index("Department", Name = "IX_Profiles_Department")]
[Index("EmployeeCode", Name = "UQ_Profiles_EmployeeCode", IsUnique = true)]
[Index("UserId", Name = "UQ_Profiles_UserId", IsUnique = true)]
public partial class Profile
{
    [Key]
    public int ProfileId { get; set; }

    public int UserId { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string EmployeeCode { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? JobTitle { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Department { get; set; }

    public DateOnly? HireDate { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? BaseSalary { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("ProfileCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("ProfileUpdatedByNavigations")]
    public virtual User? UpdatedByNavigation { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ProfileUser")]
    public virtual User User { get; set; } = null!;
}
