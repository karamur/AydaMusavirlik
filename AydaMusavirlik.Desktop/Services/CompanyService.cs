using AydaMusavirlik.Core.Models.Common;
using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Services;

namespace AydaMusavirlik.Desktop.Services;

public interface ICompanyService
{
    Task<List<CompanyDto>> GetAllAsync();
    Task<CompanyDto?> GetByIdAsync(int id);
    Task<CompanyDto?> CreateAsync(CompanyDto company);
    Task<CompanyDto?> CreateAsync(CreateCompanyDto dto);
    Task<CompanyDto?> UpdateAsync(CompanyDto company);
    Task<bool> DeleteAsync(int id);
}

public class CompanyService : ICompanyService
{
    private readonly ApiClient _apiClient;

    public CompanyService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<CompanyDto>> GetAllAsync()
    {
        var response = await _apiClient.GetAsync<List<CompanyDto>>("api/companies");
        return response.Data ?? new List<CompanyDto>();
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        var response = await _apiClient.GetAsync<CompanyDto>($"api/companies/{id}");
        return response.Data;
    }

    public async Task<CompanyDto?> CreateAsync(CompanyDto company)
    {
        var response = await _apiClient.PostAsync<CompanyDto>("api/companies", company);
        return response.Data;
    }

    public async Task<CompanyDto?> CreateAsync(CreateCompanyDto dto)
    {
        var response = await _apiClient.PostAsync<CompanyDto>("api/companies", dto);
        return response.Data;
    }

    public async Task<CompanyDto?> UpdateAsync(CompanyDto company)
    {
        var response = await _apiClient.PutAsync<CompanyDto>($"api/companies/{company.Id}", company);
        return response.Data;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _apiClient.DeleteAsync($"api/companies/{id}");
        return response.Success;
    }
}

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? TradeRegistryNumber { get; set; }
    public string? MersisNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? CompanyType { get; set; }
    public decimal? Capital { get; set; }
    public DateTime? FoundationDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? TradeRegistryNumber { get; set; }
    public string? MersisNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? CompanyType { get; set; }
    public decimal? Capital { get; set; }
    public DateTime? FoundationDate { get; set; }
}