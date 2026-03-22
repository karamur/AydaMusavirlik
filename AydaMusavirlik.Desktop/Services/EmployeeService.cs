using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Payroll;
using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Services;

namespace AydaMusavirlik.Desktop.Services;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync();
    Task<List<EmployeeDto>> GetByCompanyAsync(int companyId);
    Task<List<EmployeeDto>> GetActiveByCompanyAsync(int companyId);
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto?> CreateAsync(EmployeeDto employee);
    Task<EmployeeDto?> UpdateAsync(EmployeeDto employee);
    Task<bool> DeleteAsync(int id);
}

public class EmployeeService : IEmployeeService
{
    private readonly ApiClient _apiClient;

    public EmployeeService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<EmployeeDto>> GetAllAsync()
    {
        var response = await _apiClient.GetAsync<List<EmployeeDto>>("api/employees");
        return response.Data ?? new List<EmployeeDto>();
    }

    public async Task<List<EmployeeDto>> GetByCompanyAsync(int companyId)
    {
        var response = await _apiClient.GetAsync<List<EmployeeDto>>($"api/employees/company/{companyId}");
        return response.Data ?? new List<EmployeeDto>();
    }

    public async Task<List<EmployeeDto>> GetActiveByCompanyAsync(int companyId)
    {
        var response = await _apiClient.GetAsync<List<EmployeeDto>>($"api/employees/company/{companyId}/active");
        if (response.Success && response.Data != null)
            return response.Data;
        
        // API yoksa tum listeyi getir ve filtrele
        var all = await GetByCompanyAsync(companyId);
        return all.Where(e => e.IsActive).ToList();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var response = await _apiClient.GetAsync<EmployeeDto>($"api/employees/{id}");
        return response.Data;
    }

    public async Task<EmployeeDto?> CreateAsync(EmployeeDto employee)
    {
        var response = await _apiClient.PostAsync<EmployeeDto>("api/employees", employee);
        return response.Data;
    }

    public async Task<EmployeeDto?> UpdateAsync(EmployeeDto employee)
    {
        var response = await _apiClient.PutAsync<EmployeeDto>($"api/employees/{employee.Id}", employee);
        return response.Data;
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
    public string? TcKimlikNo { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal? Salary { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}";
}