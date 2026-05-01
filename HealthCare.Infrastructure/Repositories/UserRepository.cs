using Microsoft.EntityFrameworkCore;
using HealthCare.Domain.Modules.Users;
using HealthCare.Infrastructure.Persistence.Context;
using System.Reflection;

using DomainUser = HealthCare.Domain.Modules.Users.Entities.User;
using DomainRole = HealthCare.Domain.Modules.Role.Entities.Role;
using DomainPerson = HealthCare.Domain.Modules.Person.Entities.Person;

using InfraUser = HealthCare.Infrastructure.Persistence.Entities.User;


namespace HealthCare.Infrastructure.Repositories;

public class UserRepository(HeathCareDbContext context) : IUserRepository
{
    public async Task<DomainUser?> GetByIdAsync(int id)
    {
        var dbUser = await context.Users
            .Include(u => u.Person)
            .Include(u => u.ProfileUser)
            .Include(u => u.UserroleUsers)
                .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Userid == id);

        return dbUser != null ? MapToDomain(dbUser) : null;
    }

    public async Task<DomainUser?> GetByUsernameAsync(string username)
    {
        var dbUser = await context.Users
            .Include(u => u.Person)
            .Include(u => u.ProfileUser)
            .Include(u => u.UserroleUsers)
                .ThenInclude(ur => ur.Role)
            .AsNoTracking() 
            .FirstOrDefaultAsync(u => u.Username == username);

        return dbUser != null ? MapToDomain(dbUser) : null;
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> ExistsByPersonIdAsync(int personId)
    {
        return await context.Users.AnyAsync(u => u.Personid == personId);
    }

    public async Task AddAsync(DomainUser domainUser)
    {
        var entity = new InfraUser
        {
            Personid = domainUser.PersonId,
            Username = domainUser.Username,
            Passwordhash = domainUser.PasswordHash,
            Passwordsalt = domainUser.PasswordSalt,
            Mustchangepassword = domainUser.MustChangePassword,
            Isactive = domainUser.IsActive,
            Createdat = domainUser.CreatedAt
        };

        await context.Users.AddAsync(entity);

        await context.SaveChangesAsync();
    }

    public void Update(DomainUser domainUser)
    {
        var entity = context.Users.FirstOrDefault(u => u.Userid == domainUser.Id);
        
        if (entity != null)
        {
            entity.Failedloginattempts = domainUser.FailedLoginAttempts;
            entity.Lockoutendutc = domainUser.LockoutEndUtc;
            entity.Lastloginat = domainUser.LastLoginAt;
            entity.Lastloginip = domainUser.LastLoginIp;
            entity.Passwordhash = domainUser.PasswordHash;
            entity.Passwordsalt = domainUser.PasswordSalt;
            entity.Passwordchangedat = domainUser.PasswordChangedAt;
            entity.Mustchangepassword = domainUser.MustChangePassword;
            entity.Isactive = domainUser.IsActive;
            entity.Updatedat = domainUser.UpdatedAt;

            context.Users.Update(entity);
        }
    }

    
    private DomainUser MapToDomain(InfraUser infraUser)
    {
        var domainUser = new DomainUser(
            infraUser.Personid,
            infraUser.Username,
            infraUser.Passwordhash,
            infraUser.Passwordsalt
        );

        SetPrivatePropertyValue(domainUser, "Id", infraUser.Userid);
        SetPrivatePropertyValue(domainUser, "FailedLoginAttempts", infraUser.Failedloginattempts);
        SetPrivatePropertyValue(domainUser, "LockoutEndUtc", infraUser.Lockoutendutc);
        SetPrivatePropertyValue(domainUser, "IsActive", infraUser.Isactive);

        if (infraUser.Person != null)
        {
            var domainPerson = new DomainPerson(
                infraUser.Person.Documenttypeid,
                infraUser.Person.Documentnumber,
                infraUser.Person.Firstname,
                infraUser.Person.Lastname,
                infraUser.Person.Email
            );
            SetPrivatePropertyValue(domainPerson, "Id", infraUser.Person.Personid);
            SetPrivatePropertyValue(domainUser, "Person", domainPerson);
        }

        // 4. Mapear los Roles usando Reflection para agregarlos a la lista privada (_roles)
        if (infraUser.UserroleUsers != null && infraUser.UserroleUsers.Any())
        {
            var rolesField = typeof(DomainUser).GetField("_roles", BindingFlags.NonPublic | BindingFlags.Instance);
            var rolesList = (List<DomainRole>?)rolesField?.GetValue(domainUser);

            foreach (var userRole in infraUser.UserroleUsers)
            {
                if (userRole.Role != null)
                {
                    var domainRole = new DomainRole(userRole.Role.Name, userRole.Role.Description);
                    SetPrivatePropertyValue(domainRole, "Id", userRole.Role.Roleid);
                    rolesList?.Add(domainRole);
                }
            }
        }

        return domainUser;
    }

    private void SetPrivatePropertyValue<T>(T obj, string propertyName, object? value)
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value, null);
        }
    }
}