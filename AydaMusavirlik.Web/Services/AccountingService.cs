using AydaMusavirlik.Models.Accounting;
using AydaMusavirlik.Models.Common;

namespace AydaMusavirlik.Services;

/// <summary>
/// Muhasebe servisi
/// </summary>
public class AccountingService
{
    private readonly ILogger<AccountingService> _logger;
    private readonly List<Account> _accounts = new();
    private readonly List<AccountingRecord> _records = new();

    public AccountingService(ILogger<AccountingService> logger)
    {
        _logger = logger;
        InitializeChartOfAccounts();
        InitializeSampleRecords();
    }

    private void InitializeChartOfAccounts()
    {
        // Tek Düzen Hesap Planý - Ana Gruplar
        _accounts.AddRange(new[]
        {
            // 1 - Dönen Varlýklar
            new Account { Id = 1, Code = "100", Name = "Kasa", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, CompanyId = 1 },
            new Account { Id = 2, Code = "101", Name = "Alýnan Çekler", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, CompanyId = 1 },
            new Account { Id = 3, Code = "102", Name = "Bankalar", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, CompanyId = 1 },
            new Account { Id = 4, Code = "120", Name = "Alýcýlar", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, CompanyId = 1 },
            new Account { Id = 5, Code = "153", Name = "Ticari Mallar", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, CompanyId = 1 },

            // 3 - Kýsa Vadeli Yabancý Kaynaklar
            new Account { Id = 6, Code = "300", Name = "Banka Kredileri", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, CompanyId = 1 },
            new Account { Id = 7, Code = "320", Name = "Satýcýlar", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, CompanyId = 1 },
            new Account { Id = 8, Code = "360", Name = "Ödenecek Vergi ve Fonlar", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, CompanyId = 1 },

            // 5 - Özkaynaklar
            new Account { Id = 9, Code = "500", Name = "Sermaye", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, CompanyId = 1 },
            new Account { Id = 10, Code = "570", Name = "Geçmiþ Yýllar Karlarý", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, CompanyId = 1 },

            // 6 - Gelir Tablosu Hesaplarý
            new Account { Id = 11, Code = "600", Name = "Yurtiçi Satýþlar", AccountType = AccountType.Gelir, Nature = AccountNature.Credit, Level = 1, CompanyId = 1 },
            new Account { Id = 12, Code = "602", Name = "Diðer Gelirler", AccountType = AccountType.Gelir, Nature = AccountNature.Credit, Level = 1, CompanyId = 1 },

            // 7 - Maliyet Hesaplarý
            new Account { Id = 13, Code = "620", Name = "Satýlan Malýn Maliyeti", AccountType = AccountType.Maliyet, Nature = AccountNature.Debit, Level = 1, CompanyId = 1 },
            new Account { Id = 14, Code = "770", Name = "Genel Yönetim Giderleri", AccountType = AccountType.Gider, Nature = AccountNature.Debit, Level = 1, CompanyId = 1 },
            new Account { Id = 15, Code = "780", Name = "Finansman Giderleri", AccountType = AccountType.Gider, Nature = AccountNature.Debit, Level = 1, CompanyId = 1 }
        });

        _logger.LogInformation("Hesap planý oluþturuldu: {Count} hesap", _accounts.Count);
    }

    private void InitializeSampleRecords()
    {
        _records.AddRange(new[]
        {
            new AccountingRecord
            {
                Id = 1,
                CompanyId = 1,
                DocumentNumber = "MHS-2024-001",
                DocumentDate = DateTime.Today.AddDays(-5),
                RecordType = RecordType.MahsupFisi,
                Description = "Açýlýþ kaydý",
                TotalDebit = 100000,
                TotalCredit = 100000,
                Status = RecordStatus.Posted
            },
            new AccountingRecord
            {
                Id = 2,
                CompanyId = 1,
                DocumentNumber = "TAH-2024-001",
                DocumentDate = DateTime.Today.AddDays(-3),
                RecordType = RecordType.TahsilatFisi,
                Description = "Müþteri tahsilatý",
                TotalDebit = 15000,
                TotalCredit = 15000,
                Status = RecordStatus.Posted
            },
            new AccountingRecord
            {
                Id = 3,
                CompanyId = 1,
                DocumentNumber = "ODM-2024-001",
                DocumentDate = DateTime.Today.AddDays(-1),
                RecordType = RecordType.OdemeFisi,
                Description = "Tedarikçi ödemesi",
                TotalDebit = 8500,
                TotalCredit = 8500,
                Status = RecordStatus.Approved
            }
        });

        _logger.LogInformation("Örnek muhasebe kayýtlarý oluþturuldu: {Count}", _records.Count);
    }

    // Hesap Planý Ýþlemleri
    public Task<List<Account>> GetAccountsAsync(int companyId)
    {
        return Task.FromResult(_accounts.Where(a => a.CompanyId == companyId && !a.IsDeleted).ToList());
    }

    public Task<Account?> GetAccountByCodeAsync(int companyId, string code)
    {
        var account = _accounts.FirstOrDefault(a => a.CompanyId == companyId && a.Code == code);
        return Task.FromResult(account);
    }

    public Task<Account> CreateAccountAsync(Account account)
    {
        account.Id = _accounts.Count > 0 ? _accounts.Max(a => a.Id) + 1 : 1;
        account.CreatedAt = DateTime.UtcNow;
        _accounts.Add(account);
        return Task.FromResult(account);
    }

    // Muhasebe Kaydý Ýþlemleri
    public Task<List<AccountingRecord>> GetRecordsAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _records.Where(r => r.CompanyId == companyId && !r.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(r => r.DocumentDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(r => r.DocumentDate <= endDate.Value);

        return Task.FromResult(query.OrderByDescending(r => r.DocumentDate).ToList());
    }

    public Task<AccountingRecord?> GetRecordByIdAsync(int id)
    {
        var record = _records.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(record);
    }

    public Task<AccountingRecord> CreateRecordAsync(AccountingRecord record)
    {
        record.Id = _records.Count > 0 ? _records.Max(r => r.Id) + 1 : 1;
        record.CreatedAt = DateTime.UtcNow;
        record.Status = RecordStatus.Draft;
        _records.Add(record);
        _logger.LogInformation("Muhasebe kaydý oluþturuldu: {DocumentNumber}", record.DocumentNumber);
        return Task.FromResult(record);
    }

    public Task<string> GenerateDocumentNumberAsync(int companyId, RecordType type)
    {
        var prefix = type switch
        {
            RecordType.MahsupFisi => "MHS",
            RecordType.TahsilatFisi => "TAH",
            RecordType.OdemeFisi => "ODM",
            RecordType.SatisFaturasi => "SAT",
            RecordType.AlisFaturasi => "ALS",
            _ => "FIS"
        };

        var count = _records.Count(r => r.CompanyId == companyId && r.RecordType == type) + 1;
        return Task.FromResult($"{prefix}-{DateTime.Today.Year}-{count:D3}");
    }

    // Dashboard için özet bilgiler
    public Task<AccountingSummary> GetSummaryAsync(int companyId)
    {
        var records = _records.Where(r => r.CompanyId == companyId && !r.IsDeleted).ToList();

        return Task.FromResult(new AccountingSummary
        {
            TotalRecords = records.Count,
            DraftRecords = records.Count(r => r.Status == RecordStatus.Draft),
            ApprovedRecords = records.Count(r => r.Status == RecordStatus.Approved),
            PostedRecords = records.Count(r => r.Status == RecordStatus.Posted),
            TotalDebit = records.Sum(r => r.TotalDebit),
            TotalCredit = records.Sum(r => r.TotalCredit)
        });
    }
}

public class AccountingSummary
{
    public int TotalRecords { get; set; }
    public int DraftRecords { get; set; }
    public int ApprovedRecords { get; set; }
    public int PostedRecords { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}
