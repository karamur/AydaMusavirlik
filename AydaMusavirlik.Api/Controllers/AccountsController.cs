using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AydaMusavirlik.Data.Repositories;
using AydaMusavirlik.Core.Models.Accounting;

namespace AydaMusavirlik.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("company/{companyId}")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetByCompany(int companyId)
    {
        var accounts = await _unitOfWork.Accounts.GetByCompanyAsync(companyId);
        return Ok(accounts.Select(a => MapToDto(a)));
    }

    [HttpGet("company/{companyId}/main")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetMainAccounts(int companyId)
    {
        var accounts = await _unitOfWork.Accounts.GetMainAccountsAsync(companyId);
        return Ok(accounts.Select(a => MapToDto(a)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDto>> GetById(int id)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id);
        if (account == null)
            return NotFound();

        return Ok(MapToDto(account));
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create(CreateAccountDto dto)
    {
        var existing = await _unitOfWork.Accounts.GetByCodeAsync(dto.CompanyId, dto.Code);
        if (existing != null)
            return BadRequest("Bu hesap kodu zaten mevcut.");

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
            AllowPosting = dto.AllowPosting
        };

        await _unitOfWork.Accounts.AddAsync(account);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, MapToDto(account));
    }

    [HttpPost("company/{companyId}/seed-standard")]
    public async Task<ActionResult> SeedStandardAccounts(int companyId)
    {
        var existing = await _unitOfWork.Accounts.GetByCompanyAsync(companyId);
        if (existing.Any())
            return BadRequest("Bu firma icin hesap plani zaten mevcut.");

        var accounts = GetStandardAccountPlan(companyId);
        await _unitOfWork.Accounts.AddRangeAsync(accounts);

        return Ok(new { message = $"{accounts.Count} hesap basariyla yuklendi." });
    }

    [HttpGet("company/{companyId}/trial-balance")]
    public async Task<ActionResult<TrialBalanceDto>> GetTrialBalance(int companyId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var accounts = await _unitOfWork.Accounts.GetAccountsWithBalanceAsync(companyId, startDate, endDate);

        var items = accounts.Select(a => new TrialBalanceItemDto
        {
            AccountCode = a.Code,
            AccountName = a.Name,
            TotalDebit = a.Entries.Sum(e => e.Debit),
            TotalCredit = a.Entries.Sum(e => e.Credit),
            DebitBalance = Math.Max(0, a.Entries.Sum(e => e.Debit) - a.Entries.Sum(e => e.Credit)),
            CreditBalance = Math.Max(0, a.Entries.Sum(e => e.Credit) - a.Entries.Sum(e => e.Debit))
        }).Where(i => i.TotalDebit > 0 || i.TotalCredit > 0).ToList();

        return Ok(new TrialBalanceDto
        {
            StartDate = startDate,
            EndDate = endDate,
            Items = items,
            TotalDebit = items.Sum(i => i.TotalDebit),
            TotalCredit = items.Sum(i => i.TotalCredit)
        });
    }

    private static AccountDto MapToDto(Account a) => new()
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
    };

    private List<Account> GetStandardAccountPlan(int companyId)
    {
        return new List<Account>
        {
            // 1 - DONEN VARLIKLAR
            new() { CompanyId = companyId, Code = "1", Name = "DONEN VARLIKLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "10", Name = "HAZIR DEGERLER", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "100", Name = "KASA", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "101", Name = "ALINAN CEKLER", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "102", Name = "BANKALAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "103", Name = "VERILEN CEKLER VE ODEME EMIRLERI (-)", AccountType = AccountType.Aktif, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "12", Name = "TICARI ALACAKLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "120", Name = "ALICILAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "121", Name = "ALACAK SENETLERI", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "15", Name = "STOKLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "153", Name = "TICARI MALLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "19", Name = "DIGER DONEN VARLIKLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "191", Name = "INDIRILECEK KDV", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },

            // 2 - DURAN VARLIKLAR
            new() { CompanyId = companyId, Code = "2", Name = "DURAN VARLIKLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "25", Name = "MADDI DURAN VARLIKLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "254", Name = "TASITLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "255", Name = "DEMIRBASLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "257", Name = "BIRIKMIS AMORTISMANLAR (-)", AccountType = AccountType.Aktif, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },

            // 3 - KISA VADELI YABANCI KAYNAKLAR
            new() { CompanyId = companyId, Code = "3", Name = "KISA VADELI YABANCI KAYNAKLAR", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "32", Name = "TICARI BORCLAR", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "320", Name = "SATICILAR", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "321", Name = "BORC SENETLERI", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "33", Name = "DIGER BORCLAR", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "335", Name = "PERSONELE BORCLAR", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "36", Name = "ODENECEK VERGI VE DIGER YUKUMLULUKLER", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "360", Name = "ODENECEK VERGI VE FONLAR", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "361", Name = "ODENECEK SOSYAL GUVENLIK KESINTILERI", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "391", Name = "HESAPLANAN KDV", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },

            // 5 - OZKAYNAKLAR
            new() { CompanyId = companyId, Code = "5", Name = "OZKAYNAKLAR", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "50", Name = "ODENMIS SERMAYE", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "500", Name = "SERMAYE", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "57", Name = "GECMIS YILLAR KARLARI", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "570", Name = "GECMIS YILLAR KARLARI", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "59", Name = "DONEM NET KARI (ZARARI)", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "590", Name = "DONEM NET KARI", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "591", Name = "DONEM NET ZARARI (-)", AccountType = AccountType.Pasif, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },

            // 6 - GELIR TABLOSU HESAPLARI
            new() { CompanyId = companyId, Code = "6", Name = "GELIR TABLOSU HESAPLARI", AccountType = AccountType.Gelir, Nature = AccountNature.Credit, Level = 1, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "60", Name = "BRUT SATISLAR", AccountType = AccountType.Gelir, Nature = AccountNature.Credit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "600", Name = "YURTICI SATISLAR", AccountType = AccountType.Gelir, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "601", Name = "YURTDISI SATISLAR", AccountType = AccountType.Gelir, Nature = AccountNature.Credit, Level = 3, IsHeader = false, AllowPosting = true },
            new() { CompanyId = companyId, Code = "62", Name = "SATISLARIN MALIYETI (-)", AccountType = AccountType.Gider, Nature = AccountNature.Debit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "621", Name = "SATILAN TICARI MALLAR MALIYETI (-)", AccountType = AccountType.Gider, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },

            // 7 - MALIYET HESAPLARI
            new() { CompanyId = companyId, Code = "7", Name = "MALIYET HESAPLARI", AccountType = AccountType.Maliyet, Nature = AccountNature.Debit, Level = 1, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "77", Name = "GENEL YONETIM GIDERLERI", AccountType = AccountType.Gider, Nature = AccountNature.Debit, Level = 2, IsHeader = true, AllowPosting = false },
            new() { CompanyId = companyId, Code = "770", Name = "GENEL YONETIM GIDERLERI", AccountType = AccountType.Gider, Nature = AccountNature.Debit, Level = 3, IsHeader = false, AllowPosting = true },
        };
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