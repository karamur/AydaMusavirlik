using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Desktop.Services;

public interface ICompanyService
{
    Task<IEnumerable<CompanyDto>> GetAllAsync();
    Task<CompanyDto?> GetByIdAsync(int id);
    Task<CompanyDto?> CreateAsync(CreateCompanyDto dto);
    Task<bool> UpdateAsync(int id, CreateCompanyDto dto);
    Task<bool> DeleteAsync(int id);
}

public class CompanyService : ICompanyService
{
    private readonly ApiClient _apiClient;

    public CompanyService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<CompanyDto>> GetAllAsync()
    {
        var response = await _apiClient.GetAsync<IEnumerable<CompanyDto>>("api/companies");
        return response.Data ?? Enumerable.Empty<CompanyDto>();
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        var response = await _apiClient.GetAsync<CompanyDto>($"api/companies/{id}");
        return response.Data;
    }

    public async Task<CompanyDto?> CreateAsync(CreateCompanyDto dto)
    {
        var response = await _apiClient.PostAsync<CompanyDto>("api/companies", dto);
        return response.Data;
    }

    public async Task<bool> UpdateAsync(int id, CreateCompanyDto dto)
    {
        var response = await _apiClient.PutAsync<object>($"api/companies/{id}", dto);
        return response.Success;
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
    public string? Website { get; set; }
    public decimal? Capital { get; set; }
    public DateTime? FoundationDate { get; set; }
    public CompanyType CompanyType { get; set; }
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
    public string? Website { get; set; }
    public decimal? Capital { get; set; }
    public DateTime? FoundationDate { get; set; }
    public CompanyType CompanyType { get; set; }
}