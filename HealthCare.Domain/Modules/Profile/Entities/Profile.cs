namespace HealthCare.Domain.Modules.Profile.Entities;

public class Profile
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string EmployeeCode { get; private set; } = string.Empty;
    public string? JobTitle { get; private set; }
    public string? Department { get; private set; }
    public DateTime? HireDate { get; private set; }
    public decimal? BaseSalary { get; private set; }
    public bool IsActive { get; private set; }

    protected Profile() { } 

    public Profile(int userId, string employeeCode)
    {
        UserId = userId;
        EmployeeCode = employeeCode;
        IsActive = true;
    }

    public void UpdateEmploymentDetails(string? jobTitle, string? department, decimal? baseSalary)
    {
        if (baseSalary < 0)
            throw new ArgumentException("El salario base no puede ser negativo.");

        JobTitle = jobTitle;
        Department = department;
        BaseSalary = baseSalary;
    }
}