using AydaMusavirlik.Models.Common;

namespace AydaMusavirlik.Services;

/// <summary>
/// Firma yönetim servisi
/// </summary>
public class CompanyService
{
    private readonly ILogger<CompanyService> _logger;
    private readonly List<Company> _companies = new();

    public CompanyService(ILogger<CompanyService> logger)
    {
        _logger = logger;
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        _companies.AddRange(new[]
        {
            new Company
            {
                Id = 1,
                Name = "ABC Teknoloji A.Þ.",
                TaxNumber = "1234567890",
                TaxOffice = "Kadýköy",
                CompanyType = CompanyType.AnonimSirket,
                City = "Ýstanbul",
                District = "Kadýköy",
                Email = "info@abcteknoloji.com",
                Phone = "0216 123 45 67",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Company
            {
                Id = 2,
                Name = "XYZ Danýþmanlýk Ltd. Þti.",
                TaxNumber = "9876543210",
                TaxOffice = "Beþiktaþ",
                CompanyType = CompanyType.LimitedSirketi,
                City = "Ýstanbul",
                District = "Beþiktaþ",
                Email = "info@xyzdanismanlik.com",
                Phone = "0212 987 65 43",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Company
            {
                Id = 3,
                Name = "Ahmet Yýlmaz - Þahýs Firmasý",
                TaxNumber = "1112223334",
                TaxOffice = "Üsküdar",
                CompanyType = CompanyType.SahisFirmasi,
                City = "Ýstanbul",
                District = "Üsküdar",
                Email = "ahmet@gmail.com",
                Phone = "0532 111 22 33",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        });

        _logger.LogInformation("Örnek firmalar oluþturuldu: {Count}", _companies.Count);
    }

    public Task<List<Company>> GetAllAsync()
    {
        return Task.FromResult(_companies.Where(c => c.IsActive && !c.IsDeleted).ToList());
    }

    public Task<Company?> GetByIdAsync(int id)
    {
        var company = _companies.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
        return Task.FromResult(company);
    }

    public Task<Company> CreateAsync(Company company)
    {
        company.Id = _companies.Count > 0 ? _companies.Max(c => c.Id) + 1 : 1;
        company.CreatedAt = DateTime.UtcNow;
        company.IsActive = true;
        _companies.Add(company);
        _logger.LogInformation("Firma oluþturuldu: {Name}", company.Name);
        return Task.FromResult(company);
    }

    public Task<Company> UpdateAsync(Company company)
    {
        var existing = _companies.FirstOrDefault(c => c.Id == company.Id);
        if (existing != null)
        {
            var index = _companies.IndexOf(existing);
            company.UpdatedAt = DateTime.UtcNow;
            _companies[index] = company;
        }
        return Task.FromResult(company);
    }

    public Task DeleteAsync(int id)
    {
        var company = _companies.FirstOrDefault(c => c.Id == id);
        if (company != null)
        {
            company.IsDeleted = true;
            company.DeletedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task<List<Company>> SearchAsync(string searchTerm)
    {
        var results = _companies.Where(c => 
            !c.IsDeleted && 
            (c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
             c.TaxNumber?.Contains(searchTerm) == true)).ToList();
        return Task.FromResult(results);
    }
}
