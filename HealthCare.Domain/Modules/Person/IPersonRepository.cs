namespace HealthCare.Domain.Modules.Person;

public interface IPersonRepository
{
    Task<Entities.Person?>GetByIdAsync(int id);
    Task<(IEnumerable<Entities.Person> Items, int Total)> GetAllAsync(
        int page, int pageSize, bool? isActive, CancellationToken ct = default);

    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByDocumentAsync(int documentTypeId, string documentNumber);

    Task AddAsync(Entities.Person person);
    Task UpdateAsync(Entities.Person person);
}