using System;
using System.Collections.Generic;
using HealthCare.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Context;

public partial class HeathCareDbContext : DbContext
{
    public HeathCareDbContext(DbContextOptions<HeathCareDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DocumentType> DocumentTypes { get; set; }

    public virtual DbSet<Person> Persons { get; set; }

    public virtual DbSet<Profile> Profiles { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.DocumentTypeCreatedByNavigations).HasConstraintName("FK_DocumentTypes_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.DocumentTypeUpdatedByNavigations).HasConstraintName("FK_DocumentTypes_UpdatedBy");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Gender).IsFixedLength();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PersonCreatedByNavigations).HasConstraintName("FK_Persons_CreatedBy");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.People)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Persons_DocumentTypes");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.PersonUpdatedByNavigations).HasConstraintName("FK_Persons_UpdatedBy");
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProfileCreatedByNavigations).HasConstraintName("FK_Profiles_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ProfileUpdatedByNavigations).HasConstraintName("FK_Profiles_UpdatedBy");

            entity.HasOne(d => d.User).WithOne(p => p.ProfileUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Profiles_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RoleCreatedByNavigations).HasConstraintName("FK_Roles_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.RoleUpdatedByNavigations).HasConstraintName("FK_Roles_UpdatedBy");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MustChangePassword).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InverseCreatedByNavigation).HasConstraintName("FK_Users_CreatedBy");

            entity.HasOne(d => d.Person).WithOne(p => p.User)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Persons");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.InverseUpdatedByNavigation).HasConstraintName("FK_Users_UpdatedBy");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.UserRoleCreatedByNavigations).HasConstraintName("FK_UserRoles_CreatedBy");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.UserRoleUpdatedByNavigations).HasConstraintName("FK_UserRoles_UpdatedBy");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoleUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
