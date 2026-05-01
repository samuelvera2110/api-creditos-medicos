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

    public virtual DbSet<Documenttype> Documenttypes { get; set; }

    public virtual DbSet<Person> Persons { get; set; }

    public virtual DbSet<Profile> Profiles { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userrole> Userroles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Documenttype>(entity =>
        {
            entity.HasKey(e => e.Documenttypeid).HasName("documenttypes_pkey");

            entity.Property(e => e.Documenttypeid).UseIdentityAlwaysColumn();
            entity.Property(e => e.Createdat).HasDefaultValueSql("(CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text)");
            entity.Property(e => e.Isactive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.DocumenttypeCreatedbyNavigations).HasConstraintName("fk_documenttypes_createdby");

            entity.HasOne(d => d.UpdatedbyNavigation).WithMany(p => p.DocumenttypeUpdatedbyNavigations).HasConstraintName("fk_documenttypes_updatedby");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Personid).HasName("persons_pkey");

            entity.Property(e => e.Personid).UseIdentityAlwaysColumn();
            entity.Property(e => e.Createdat).HasDefaultValueSql("(CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text)");
            entity.Property(e => e.Isactive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.PersonCreatedbyNavigations).HasConstraintName("fk_persons_createdby");

            entity.HasOne(d => d.Documenttype).WithMany(p => p.People)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_persons_documenttypes");

            entity.HasOne(d => d.UpdatedbyNavigation).WithMany(p => p.PersonUpdatedbyNavigations).HasConstraintName("fk_persons_updatedby");
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.Profileid).HasName("profiles_pkey");

            entity.Property(e => e.Profileid).UseIdentityAlwaysColumn();
            entity.Property(e => e.Createdat).HasDefaultValueSql("(CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text)");
            entity.Property(e => e.Isactive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.ProfileCreatedbyNavigations).HasConstraintName("fk_profiles_createdby");

            entity.HasOne(d => d.UpdatedbyNavigation).WithMany(p => p.ProfileUpdatedbyNavigations).HasConstraintName("fk_profiles_updatedby");

            entity.HasOne(d => d.User).WithOne(p => p.ProfileUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_profiles_users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("roles_pkey");

            entity.Property(e => e.Roleid).UseIdentityAlwaysColumn();
            entity.Property(e => e.Createdat).HasDefaultValueSql("(CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text)");
            entity.Property(e => e.Isactive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.RoleCreatedbyNavigations).HasConstraintName("fk_roles_createdby");

            entity.HasOne(d => d.UpdatedbyNavigation).WithMany(p => p.RoleUpdatedbyNavigations).HasConstraintName("fk_roles_updatedby");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.Property(e => e.Userid).UseIdentityAlwaysColumn();
            entity.Property(e => e.Createdat).HasDefaultValueSql("(CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text)");
            entity.Property(e => e.Failedloginattempts).HasDefaultValue(0);
            entity.Property(e => e.Isactive).HasDefaultValue(true);
            entity.Property(e => e.Mustchangepassword).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.InverseCreatedbyNavigation).HasConstraintName("fk_users_createdby");

            entity.HasOne(d => d.Person).WithOne(p => p.User)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_persons");

            entity.HasOne(d => d.UpdatedbyNavigation).WithMany(p => p.InverseUpdatedbyNavigation).HasConstraintName("fk_users_updatedby");
        });

        modelBuilder.Entity<Userrole>(entity =>
        {
            entity.HasKey(e => new { e.Userid, e.Roleid }).HasName("pk_userroles");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text)");
            entity.Property(e => e.Isactive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.UserroleCreatedbyNavigations).HasConstraintName("fk_userroles_createdby");

            entity.HasOne(d => d.Role).WithMany(p => p.Userroles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_userroles_roles");

            entity.HasOne(d => d.UpdatedbyNavigation).WithMany(p => p.UserroleUpdatedbyNavigations).HasConstraintName("fk_userroles_updatedby");

            entity.HasOne(d => d.User).WithMany(p => p.UserroleUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_userroles_users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
