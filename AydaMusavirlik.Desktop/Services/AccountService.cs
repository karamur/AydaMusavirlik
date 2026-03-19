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
    private readonly ApiClient _apiClient;

    public AccountService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<AccountDto>> GetByCompanyAsync(int companyId)
    {
        var response = await _apiClient.GetAsync<IEnumerable<AccountDto>>($"api/accounts/company/{companyId}");
        return response.Data ?? Enumerable.Empty<AccountDto>();
    }

    public async Task<IEnumerable<AccountDto>> GetMainAccountsAsync(int companyId)
    {
        var response = await _apiClient.GetAsync<IEnumerable<AccountDto>>($"api/accounts/company/{companyId}/main");
        return response.Data ?? Enumerable.Empty<AccountDto>();
    }

    public async Task<AccountDto?> GetByIdAsync(int id)
    {
        var response = await _apiClient.GetAsync<AccountDto>($"api/accounts/{id}");
        return response.Data;
    }

    public async Task<AccountDto?> CreateAsync(CreateAccountDto dto)
    {
        var response = await _apiClient.PostAsync<AccountDto>("api/accounts", dto);
        return response.Data;
    }

    public async Task<bool> SeedStandardAccountsAsync(int companyId)
    {
        var response = await _apiClient.PostAsync<object>($"api/accounts/company/{companyId}/seed-standard", new { });
        return response.Success;
    }

    public async Task<TrialBalanceDto?> GetTrialBalanceAsync(int companyId, DateTime startDate, DateTime endDate)
    {
        var start = startDate.ToString("yyyy-MM-dd");
        var end = endDate.ToString("yyyy-MM-dd");
        var response = await _apiClient.GetAsync<TrialBalanceDto>($"api/accounts/company/{companyId}/trial-balance?startDate={start}&endDate={end}");
        return response.Data;
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
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
}