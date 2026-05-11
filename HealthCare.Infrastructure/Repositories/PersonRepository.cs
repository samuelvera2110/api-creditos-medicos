using Microsoft.EntityFrameworkCore;
using HealthCare.Domain.Modules.Person;
using HealthCare.Infrastructure.Persistence.Context;

using DomainPerson = HealthCare.Domain.Modules.Person.Entities.Person;
using InfraPerson  = HealthCare.Infrastructure.Persistence.Entities.Person;

namespace HealthCare.Infrastructure.Repositories;

public class PersonRepository(HeathCareDbContext context) : IPersonRepository
{
    // ─── GetByIdAsync ─────────────────────────────────────────────────────
    public async Task<DomainPerson?> GetByIdAsync(int id)
    {
        var entity = await context.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Personid == id);

        return entity is not null ? MapToDomain(entity) : null;
    }

    // ─── GetAllAsync ──────────────────────────────────────────────────────
    public async Task<(IEnumerable<DomainPerson> Items, int Total)> GetAllAsync(
        int page, int pageSize, bool? isActive, CancellationToken ct = default)
    {
        var query = context.Persons.AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(p => p.Isactive == isActive.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Lastname)
            .ThenBy(p => p.Firstname)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items.Select(MapToDomain), total);
    }

    // ─── ExistsByEmailAsync ───────────────────────────────────────────────
    public async Task<bool> ExistsByEmailAsync(string email)
        => await context.Persons.AnyAsync(p => p.Email == email);

    // ─── ExistsByDocumentAsync ────────────────────────────────────────────
    public async Task<bool> ExistsByDocumentAsync(int documentTypeId, string documentNumber)
        => await context.Persons.AnyAsync(p =>
            p.Documenttypeid == documentTypeId &&
            p.Documentnumber == documentNumber);

    // ─── AddAsync ─────────────────────────────────────────────────────────
    public async Task AddAsync(DomainPerson domain)
    {
        var entity = new InfraPerson
        {
            Documenttypeid = domain.DocumentTypeId,
            Documentnumber = domain.DocumentNumber,
            Firstname      = domain.FirstName,
            Lastname       = domain.LastName,
            Email          = domain.Email,
            Phonenumber    = domain.PhoneNumber,
            Birthdate      = domain.BirthDate.HasValue
                                ? DateOnly.FromDateTime(domain.BirthDate.Value)
                                : null,
            Gender         = domain.Gender,   // ← char? directo, sin ToString()
            Isactive       = domain.IsActive,
            Createdat      = DateTime.UtcNow
        };

        await context.Persons.AddAsync(entity);
        await context.SaveChangesAsync();

        domain.SetId(entity.Personid);        // ← método interno en el entity
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────
    public async Task UpdateAsync(DomainPerson domain)
    {
        var entity = await context.Persons
            .FirstOrDefaultAsync(p => p.Personid == domain.Id);

        if (entity is null) return;

        entity.Firstname   = domain.FirstName;
        entity.Lastname    = domain.LastName;
        entity.Email       = domain.Email;
        entity.Phonenumber = domain.PhoneNumber;
        entity.Birthdate   = domain.BirthDate.HasValue
                                ? DateOnly.FromDateTime(domain.BirthDate.Value)
                                : null;
        entity.Gender      = domain.Gender;   // ← char? directo, sin ToString()
        entity.Isactive    = domain.IsActive;
        entity.Updatedat   = DateTime.UtcNow;

        context.Persons.Update(entity);
        await context.SaveChangesAsync();
    }

    // ─── Mapper ───────────────────────────────────────────────────────────
    private static DomainPerson MapToDomain(InfraPerson entity) =>
        DomainPerson.Reconstitute(
            id:             entity.Personid,
            documentTypeId: entity.Documenttypeid,
            documentNumber: entity.Documentnumber,
            firstName:      entity.Firstname,
            lastName:       entity.Lastname,
            email:          entity.Email,
            isActive:       entity.Isactive
        );
}