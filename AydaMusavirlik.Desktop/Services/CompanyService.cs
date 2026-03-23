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
    private readonly ApiClient? _apiClient;
    private readonly List<CompanyDto> _offlineCompanies = new();
    private int _nextId = 1;

    public CompanyService(ApiClient? apiClient = null)
    {
        _apiClient = apiClient;
        InitializeOfflineData();
    }

    private void InitializeOfflineData()
    {
        _offlineCompanies.AddRange(new[]
        {
            new CompanyDto { Id = 1, Name = "ABC Ticaret Ltd. Sti.", TaxNumber = "1234567890", TaxOffice = "Kadikoy", City = "Istanbul", IsActive = true },
            new CompanyDto { Id = 2, Name = "XYZ Sanayi A.S.", TaxNumber = "0987654321", TaxOffice = "Besiktas", City = "Istanbul", IsActive = true },
            new CompanyDto { Id = 3, Name = "Mehmet Yilmaz - Serbest Meslek", TaxNumber = "1122334455", TaxOffice = "Uskudar", City = "Istanbul", IsActive = true },
        });
        _nextId = 4;
    }

    public async Task<List<CompanyDto>> GetAllAsync()
    {
        if (_apiClient != null)
        {
            try
            {
                var response = await _apiClient.GetAsync<List<CompanyDto>>("api/companies");
                if (response.Success && response.Data != null)
                    return response.Data;
            }
            catch { }
        }
        
        // Offline mod
        return _offlineCompanies.Where(c => c.IsActive).ToList();
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        if (_apiClient != null)
        {
            try
            {
                var response = await _apiClient.GetAsync<CompanyDto>($"api/companies/{id}");
                if (response.Success)
                    return response.Data;
            }
            catch { }
        }
        
        return _offlineCompanies.FirstOrDefault(c => c.Id == id);
    }

    public async Task<CompanyDto?> CreateAsync(CompanyDto company)
    {
        if (_apiClient != null)
        {
            try
            {
                var response = await _apiClient.PostAsync<CompanyDto>("api/companies", company);
                if (response.Success)
                    return response.Data;
            }
            catch { }
        }
        
        // Offline mod
        company.Id = _nextId++;
        company.IsActive = true;
        _offlineCompanies.Add(company);
        return company;
    }

    public async Task<CompanyDto?> CreateAsync(CreateCompanyDto dto)
    {
        var company = new CompanyDto
        {
            Name = dto.Name,
            TaxNumber = dto.TaxNumber,
            TaxOffice = dto.TaxOffice,
            TradeRegistryNumber = dto.TradeRegistryNumber,
            MersisNumber = dto.MersisNumber,
            Address = dto.Address,
            City = dto.City,
            District = dto.District,
            Phone = dto.Phone,
            Email = dto.Email,
            CompanyType = dto.CompanyType,
            Capital = dto.Capital,
            FoundationDate = dto.FoundationDate,
            IsActive = true
        };

        if (_apiClient != null)
        {
            try
            {
                var response = await _apiClient.PostAsync<CompanyDto>("api/companies", dto);
                if (response.Success)
                    return response.Data;
            }
            catch { }
        }
        
        // Offline mod
        company.Id = _nextId++;
        _offlineCompanies.Add(company);
        return company;
    }

    public async Task<CompanyDto?> UpdateAsync(CompanyDto company)
    {
        if (_apiClient != null)
        {
            try
            {
                var response = await _apiClient.PutAsync<CompanyDto>($"api/companies/{company.Id}", company);
                if (response.Success)
                    return response.Data;
            }
            catch { }
        }
        
        // Offline mod
        var existing = _offlineCompanies.FirstOrDefault(c => c.Id == company.Id);
        if (existing != null)
        {
            var index = _offlineCompanies.IndexOf(existing);
            _offlineCompanies[index] = company;
            return company;
        }
        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (_apiClient != null)
        {
            try
            {
                var response = await _apiClient.DeleteAsync($"api/companies/{id}");
                if (response.Success)
                    return true;
            }
            catch { }
        }
        
        // Offline mod
        var company = _offlineCompanies.FirstOrDefault(c => c.Id == id);
        if (company != null)
        {
            company.IsActive = false;
            return true;
        }
        return false;
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