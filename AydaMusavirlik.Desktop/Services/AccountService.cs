using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Accounting;
using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Services;

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
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var accounts = await context.Accounts
                .Where(a => a.CompanyId == companyId && a.IsActive)
                .OrderBy(a => a.Code)
                .ToListAsync();

            return accounts.Select(a => new AccountDto
            {
                Id = a.Id,
                CompanyId = a.CompanyId,
                Code = a.Code,
                Name = a.Name,
                ParentId = a.ParentId,
                AccountType = a.AccountType,
                Nature = a.Nature,
                Level = a.Level,
                IsHeader = a.IsHeader,
                AllowPosting = a.AllowPosting,
                CurrentBalance = a.CurrentBalance
            });
        }
        catch
        {
            return Enumerable.Empty<AccountDto>();
        }
    }

    public async Task<IEnumerable<AccountDto>> GetMainAccountsAsync(int companyId)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var accounts = await context.Accounts
                .Where(a => a.CompanyId == companyId && a.IsActive && a.Level == 1)
                .OrderBy(a => a.Code)
                .ToListAsync();

            return accounts.Select(a => new AccountDto
            {
                Id = a.Id,
                CompanyId = a.CompanyId,
                Code = a.Code,
                Name = a.Name,
                AccountType = a.AccountType,
                Nature = a.Nature,
                Level = a.Level,
                IsHeader = a.IsHeader
            });
        }
        catch
        {
            return Enumerable.Empty<AccountDto>();
        }
    }

    public async Task<AccountDto?> GetByIdAsync(int id)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var account = await context.Accounts.FindAsync(id);
            
            if (account == null || !account.IsActive)
                return null;

            return new AccountDto
            {
                Id = account.Id,
                CompanyId = account.CompanyId,
                Code = account.Code,
                Name = account.Name,
                ParentId = account.ParentId,
                AccountType = account.AccountType,
                Nature = account.Nature,
                Level = account.Level,
                IsHeader = account.IsHeader,
                AllowPosting = account.AllowPosting,
                CurrentBalance = account.CurrentBalance
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<AccountDto?> CreateAsync(CreateAccountDto dto)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            
            var account = new Account
            {
                CompanyId = dto.CompanyId,
                Code = dto.Code,
                Name = dto.Name,
                ParentId = dto.ParentId,
                AccountType = dto.AccountType,
                Nature = dto.Nature,
                Level = dto.Level,
                IsHeader = dto.IsHeader,
                AllowPosting = dto.AllowPosting,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            return new AccountDto
            {
                Id = account.Id,
                CompanyId = account.CompanyId,
                Code = account.Code,
                Name = account.Name,
                AccountType = account.AccountType,
                Nature = account.Nature,
                Level = account.Level
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> SeedStandardAccountsAsync(int companyId)
    {
        // SeedDataService hesap plani olusturur
        return await Task.FromResult(true);
    }

    public async Task<TrialBalanceDto?> GetTrialBalanceAsync(int companyId, DateTime startDate, DateTime endDate)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            
            var accounts = await context.Accounts
                .Where(a => a.CompanyId == companyId && a.IsActive)
                .OrderBy(a => a.Code)
                .ToListAsync();

            var items = accounts.Select(a => new TrialBalanceItemDto
            {
                AccountCode = a.Code,
                AccountName = a.Name,
                Debit = a.Nature == AccountNature.Debit ? a.CurrentBalance : 0,
                Credit = a.Nature == AccountNature.Credit ? a.CurrentBalance : 0,
                DebitBalance = a.CurrentBalance > 0 && a.Nature == AccountNature.Debit ? a.CurrentBalance : 0,
                CreditBalance = a.CurrentBalance > 0 && a.Nature == AccountNature.Credit ? a.CurrentBalance : 0
            }).ToList();

            return new TrialBalanceDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Items = items,
                TotalDebit = items.Sum(i => i.Debit),
                TotalCredit = items.Sum(i => i.Credit)
            };
        }
        catch
        {
            return null;
        }
    }
}

public class AccountDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public AccountType AccountType { get; set; }
    public AccountNature Nature { get; set; }
    public int Level { get; set; }
    public bool IsHeader { get; set; }
    public bool AllowPosting { get; set; }
    public decimal CurrentBalance { get; set; }
}

public class CreateAccountDto
{
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public AccountType AccountType { get; set; }
    public AccountNature Nature { get; set; }
    public int Level { get; set; }
    public bool IsHeader { get; set; }
    public bool AllowPosting { get; set; } = true;
}

public class TrialBalanceDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<TrialBalanceItemDto> Items { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

public class TrialBalanceItemDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
    
    // View tarafindan kullanilan property'ler
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}