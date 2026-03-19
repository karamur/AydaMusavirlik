using AydaMusavirlik.Core.Models.Payroll;

namespace AydaMusavirlik.Desktop.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetByCompanyAsync(int companyId);
    Task<IEnumerable<EmployeeDto>> GetActiveByCompanyAsync(int companyId);
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto?> CreateAsync(CreateEmployeeDto dto);
    Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task<bool> DeleteAsync(int id);
}

public class EmployeeService : IEmployeeService
{
    private readonly ApiClient _apiClient;

    public EmployeeService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<EmployeeDto>> GetByCompanyAsync(int companyId)
    {
        var response = await _apiClient.GetAsync<IEnumerable<EmployeeDto>>($"api/employees/company/{companyId}");
        return response.Data ?? Enumerable.Empty<EmployeeDto>();
    }

    public async Task<IEnumerable<EmployeeDto>> GetActiveByCompanyAsync(int companyId)
    {
        var response = await _apiClient.GetAsync<IEnumerable<EmployeeDto>>($"api/employees/company/{companyId}/active");
        return response.Data ?? Enumerable.Empty<EmployeeDto>();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var response = await _apiClient.GetAsync<EmployeeDto>($"api/employees/{id}");
        return response.Data;
    }

    public async Task<EmployeeDto?> CreateAsync(CreateEmployeeDto dto)
    {
        var response = await _apiClient.PostAsync<EmployeeDto>("api/employees", dto);
        return response.Data;
    }

    public async Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var response = await _apiClient.PutAsync<object>($"api/employees/{id}", dto);
        return response.Success;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _apiClient.DeleteAsync($"api/employees/{id}");
        return response.Success;
    }
}

public class EmployeeDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal GrossSalary { get; set; }
    public bool IsActive { get; set; }
}

public class CreateEmployeeDto
{
    public int CompanyId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public int NumberOfChildren { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IbanNumber { get; set; }
    public string? SgkNumber { get; set; }
    public DateTime HireDate { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public WorkType WorkType { get; set; }
    public decimal GrossSalary { get; set; }
    public SalaryType SalaryType { get; set; }
    public bool IsMinimumWageExempt { get; set; }
    public bool IsDisabled { get; set; }
    public int? DisabilityDegree { get; set; }
}

public class UpdateEmployeeDto
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IbanNumber { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public decimal GrossSalary { get; set; }
    public DateTime? TerminationDate { get; set; }
}