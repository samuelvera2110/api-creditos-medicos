using Microsoft.EntityFrameworkCore;
using HealthCare.Domain.Modules.Users;
using HealthCare.Infrastructure.Persistence.Context;

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

    public async Task<(IEnumerable<DomainUser> Items, int Total)> GetAllAsync(
        int page, int pageSize, bool? isActive, CancellationToken ct = default)
    {
        var query = context.Users
            .Include(u => u.Person)
            .Include(u => u.ProfileUser)
            .Include(u => u.UserroleUsers).ThenInclude(ur => ur.Role)
            .AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(u => u.Isactive == isActive.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.Userid)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items.Select(MapToDomain), total);
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

    public async Task UpdateAsync(DomainUser domainUser)
    {
        var entity = await context.Users.FirstOrDefaultAsync(u => u.Userid == domainUser.Id);

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
            await context.SaveChangesAsync();
        }
    }

    private static DomainUser MapToDomain(InfraUser infraUser)
    {
        var domainUser = DomainUser.Reconstitute(
            id: infraUser.Userid,
            personId: infraUser.Personid,
            username: infraUser.Username,
            passwordHash: infraUser.Passwordhash,
            passwordSalt: infraUser.Passwordsalt,
            failedLoginAttempts: infraUser.Failedloginattempts,
            lockoutEndUtc: infraUser.Lockoutendutc,
            lastLoginAt: infraUser.Lastloginat,
            lastLoginIp: infraUser.Lastloginip,
            passwordChangedAt: infraUser.Passwordchangedat,
            mustChangePassword: infraUser.Mustchangepassword,
            isActive: infraUser.Isactive,
            createdAt: infraUser.Createdat,
            createdBy: infraUser.Createdby,
            updatedAt: infraUser.Updatedat,
            updatedBy: infraUser.Updatedby
        );

        if (infraUser.Person != null)
        {
            var domainPerson = DomainPerson.Reconstitute(
                id: infraUser.Person.Personid,
                documentTypeId: infraUser.Person.Documenttypeid,
                documentNumber: infraUser.Person.Documentnumber,
                firstName: infraUser.Person.Firstname,
                lastName: infraUser.Person.Lastname,
                email: infraUser.Person.Email,
                isActive: infraUser.Person.Isactive
            );
            domainUser.SetPerson(domainPerson);
        }

        if (infraUser.UserroleUsers != null)
        {
            foreach (var userRole in infraUser.UserroleUsers)
            {
                if (userRole.Role != null)
                {
                    var domainRole = DomainRole.Reconstitute(
                        id: userRole.Role.Roleid,
                        name: userRole.Role.Name,
                        description: userRole.Role.Description,
                        isActive: userRole.Role.Isactive
                    );
                    domainUser.AddRole(domainRole);
                }
            }
        }

        return domainUser;
    }
}