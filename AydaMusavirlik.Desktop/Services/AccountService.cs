using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Accounting;

namespace AydaMusavirlik.Desktop.Services;

public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetByCompanyAsync(int companyId);
    Task<IEnumerable<AccountDto>> GetMainAccountsAsync(int companyId);
    Task<AccountDto?> GetByIdAsync(int id);
    Task<AccountDto?> CreateAsync(CreateAccountDto dto);
    Task<bool> SeedStandardAccountsAsync(int companyId);
    Task<TrialBalanceDto?> GetTrialBalanceAsync(int companyId, DateTime startDate, DateTime endDate);
}

public class AccountService : IAccountService
{
    private readonly ApiClient? _apiClient;
    private readonly ISettingsService _settingsService;

    public AccountService(ISettingsService settingsService, ApiClient? apiClient = null)
    {
        _settingsService = settingsService;
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<AccountDto>> GetByCompanyAsync(int companyId)
    {
        // Simule data - gercek uygulamada veritabanindan cekilir
        await Task.Delay(100);
        return GetSampleAccounts(companyId);
    }

    public async Task<IEnumerable<AccountDto>> GetMainAccountsAsync(int companyId)
    {
        await Task.Delay(100);
        return GetSampleAccounts(companyId).Where(a => a.Level == 1);
    }

    public async Task<AccountDto?> GetByIdAsync(int id)
    {
        await Task.Delay(100);
        return GetSampleAccounts(1).FirstOrDefault(a => a.Id == id);
    }

    public async Task<AccountDto?> CreateAsync(CreateAccountDto dto)
    {
        await Task.Delay(100);
        return new AccountDto
        {
            Id = new Random().Next(1000, 9999),
            CompanyId = dto.CompanyId,
            Code = dto.Code,
            Name = dto.Name,
            ParentId = dto.ParentId,
            AccountType = dto.AccountType,
            Nature = dto.Nature,
            Level = dto.Level,
            IsHeader = dto.IsHeader,
            AllowPosting = dto.AllowPosting,
            IsActive = true
        };
    }

    public async Task<bool> SeedStandardAccountsAsync(int companyId)
    {
        await Task.Delay(100);
        return true;
    }

    public async Task<TrialBalanceDto?> GetTrialBalanceAsync(int companyId, DateTime startDate, DateTime endDate)
    {
        await Task.Delay(100);
        
        var accounts = GetSampleAccounts(companyId);
        var items = accounts.Select(a => new TrialBalanceItemDto
        {
            AccountCode = a.Code,
            AccountName = a.Name,
            OpeningDebit = 0,
            OpeningCredit = 0,
            PeriodDebit = new Random().Next(0, 10000),
            PeriodCredit = new Random().Next(0, 10000),
            ClosingDebit = 0,
            ClosingCredit = 0
        }).ToList();

        return new TrialBalanceDto
        {
            CompanyId = companyId,
            StartDate = startDate,
            EndDate = endDate,
            Items = items,
            TotalDebit = items.Sum(i => i.PeriodDebit),
            TotalCredit = items.Sum(i => i.PeriodCredit)
        };
    }

    private static List<AccountDto> GetSampleAccounts(int companyId)
    {
        return new List<AccountDto>
        {
            new() { Id = 1, CompanyId = companyId, Code = "100", Name = "Kasa", Level = 1, IsActive = true, AccountType = "Aktif", Nature = "Borc" },
            new() { Id = 2, CompanyId = companyId, Code = "102", Name = "Bankalar", Level = 1, IsActive = true, AccountType = "Aktif", Nature = "Borc" },
            new() { Id = 3, CompanyId = companyId, Code = "120", Name = "Alicilar", Level = 1, IsActive = true, AccountType = "Aktif", Nature = "Borc" },
            new() { Id = 4, CompanyId = companyId, Code = "320", Name = "Saticilar", Level = 1, IsActive = true, AccountType = "Pasif", Nature = "Alacak" },
            new() { Id = 5, CompanyId = companyId, Code = "600", Name = "Yurtici Satislar", Level = 1, IsActive = true, AccountType = "Gelir", Nature = "Alacak" },
            new() { Id = 6, CompanyId = companyId, Code = "770", Name = "Genel Yonetim Giderleri", Level = 1, IsActive = true, AccountType = "Gider", Nature = "Borc" },
        };
    }
}

// DTOs
public class AccountDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int? ParentId { get; set; }
    public string AccountType { get; set; } = "";
    public string Nature { get; set; } = "";
    public int Level { get; set; }
    public bool IsHeader { get; set; }
    public bool AllowPosting { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public decimal CurrentBalance { get; set; }
    public decimal DebitTotal { get; set; }
    public decimal CreditTotal { get; set; }
}

public class CreateAccountDto
{
    public int CompanyId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int? ParentId { get; set; }
    public string AccountType { get; set; } = "";
    public string Nature { get; set; } = "";
    public int Level { get; set; }
    public bool IsHeader { get; set; }
    public bool AllowPosting { get; set; } = true;
}

public class TrialBalanceDto
{
    public int CompanyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<TrialBalanceItemDto> Items { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

public class TrialBalanceItemDto
{
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
}