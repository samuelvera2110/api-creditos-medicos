namespace HealthCare.Domain.Modules.Person.Entities;

public class Person
{
    public int Id { get; private set; }
    public int DocumentTypeId { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime? BirthDate { get; private set; }
    public char? Gender { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }

    protected Person() { } 

    public Person(int documentTypeId, string documentNumber, string firstName, string lastName, string email)
    {
        DocumentTypeId = documentTypeId;
        DocumentNumber = documentNumber;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        IsActive = true;
    }

    public void UpdateContactInfo(string email, string? phoneNumber)
    {
        Email = email;
        PhoneNumber = phoneNumber;
    }

    internal static Person Reconstitute(
        int id, int documentTypeId, string documentNumber,
        string firstName, string lastName, string email, bool isActive)
    {
        var person = new Person();
        person.Id = id;
        person.DocumentTypeId = documentTypeId;
        person.DocumentNumber = documentNumber;
        person.FirstName = firstName;
        person.LastName = lastName;
        person.Email = email;
        person.IsActive = isActive;
        return person;
    }
}