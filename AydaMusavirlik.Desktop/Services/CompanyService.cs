using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Common;
using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Services;

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
    private readonly ApiClient? _apiClient;
    private readonly ISettingsService _settingsService;

    public CompanyService(ISettingsService settingsService, ApiClient? apiClient = null)
    {
        _settingsService = settingsService;
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<CompanyDto>> GetAllAsync()
    {
        // Oncelikle API'den dene
        if (_apiClient != null)
        {
            try
            {
                var response = await _apiClient.GetAsync<IEnumerable<CompanyDto>>("api/companies");
                if (response.Success && response.Data != null)
                    return response.Data;
            }
            catch { }
        }

        // Veritabanindan getir
        return await GetAllFromDatabaseAsync();
    }

    private async Task<IEnumerable<CompanyDto>> GetAllFromDatabaseAsync()
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var companies = await context.Companies
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return companies.Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                TaxNumber = c.TaxNumber,
                TaxOffice = c.TaxOffice,
                TradeRegistryNumber = c.TradeRegistryNumber,
                MersisNumber = c.MersisNumber,
                Address = c.Address,
                City = c.City,
                District = c.District,
                Phone = c.Phone,
                Email = c.Email,
                Website = c.Website,
                Capital = c.Capital,
                FoundationDate = c.FoundationDate,
                CompanyType = c.CompanyType
            });
        }
        catch
        {
            return Enumerable.Empty<CompanyDto>();
        }
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var company = await context.Companies.FindAsync(id);
            
            if (company == null || company.IsDeleted)
                return null;

            return new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                TaxNumber = company.TaxNumber,
                TaxOffice = company.TaxOffice,
                TradeRegistryNumber = company.TradeRegistryNumber,
                MersisNumber = company.MersisNumber,
                Address = company.Address,
                City = company.City,
                District = company.District,
                Phone = company.Phone,
                Email = company.Email,
                Website = company.Website,
                Capital = company.Capital,
                FoundationDate = company.FoundationDate,
                CompanyType = company.CompanyType
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<CompanyDto?> CreateAsync(CreateCompanyDto dto)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            
            var company = new Company
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
                Website = dto.Website,
                Capital = dto.Capital,
                FoundationDate = dto.FoundationDate,
                CompanyType = dto.CompanyType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Companies.Add(company);
            await context.SaveChangesAsync();

            return new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                TaxNumber = company.TaxNumber,
                TaxOffice = company.TaxOffice,
                City = company.City,
                Phone = company.Phone,
                CompanyType = company.CompanyType
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateAsync(int id, CreateCompanyDto dto)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var company = await context.Companies.FindAsync(id);
            
            if (company == null)
                return false;

            company.Name = dto.Name;
            company.TaxNumber = dto.TaxNumber;
            company.TaxOffice = dto.TaxOffice;
            company.TradeRegistryNumber = dto.TradeRegistryNumber;
            company.MersisNumber = dto.MersisNumber;
            company.Address = dto.Address;
            company.City = dto.City;
            company.District = dto.District;
            company.Phone = dto.Phone;
            company.Email = dto.Email;
            company.Website = dto.Website;
            company.Capital = dto.Capital;
            company.FoundationDate = dto.FoundationDate;
            company.CompanyType = dto.CompanyType;
            company.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var company = await context.Companies.FindAsync(id);
            
            if (company == null)
                return false;

            // Soft delete
            company.IsDeleted = true;
            company.DeletedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
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