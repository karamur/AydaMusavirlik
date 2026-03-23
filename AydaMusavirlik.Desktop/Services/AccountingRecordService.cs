using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Accounting;
using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Services;

namespace AydaMusavirlik.Desktop.Services;

public interface IAccountingRecordService
{
    Task<IEnumerable<AccountingRecordDto>> GetByCompanyAsync(int companyId);
    Task<AccountingRecordDto?> GetByIdAsync(int id);
    Task<AccountingRecordDto?> CreateAsync(CreateAccountingRecordDto dto);
    Task<bool> ApproveAsync(int id);
    Task<bool> DeleteAsync(int id);
    Task<string> GenerateDocumentNumberAsync(int companyId, string recordType);
}

public class AccountingRecordService : IAccountingRecordService
{
    private readonly ISettingsService _settingsService;

    public AccountingRecordService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<IEnumerable<AccountingRecordDto>> GetByCompanyAsync(int companyId)
    {
        await Task.Delay(100);
        return GetSampleRecords(companyId);
    }

    public async Task<AccountingRecordDto?> GetByIdAsync(int id)
    {
        await Task.Delay(100);
        return GetSampleRecords(1).FirstOrDefault(r => r.Id == id);
    }

    public async Task<AccountingRecordDto?> CreateAsync(CreateAccountingRecordDto dto)
    {
        await Task.Delay(100);
        
        return new AccountingRecordDto
        {
            Id = new Random().Next(1000, 9999),
            CompanyId = dto.CompanyId,
            DocumentNumber = await GenerateDocumentNumberAsync(dto.CompanyId, dto.RecordType),
            Date = dto.Date,
            RecordType = dto.RecordType,
            Description = dto.Description,
            TotalDebit = dto.Entries.Sum(e => e.Debit),
            TotalCredit = dto.Entries.Sum(e => e.Credit),
            Status = "Taslak",
            Entries = dto.Entries.Select((e, i) => new AccountingEntryDto
            {
                Id = i + 1,
                AccountId = e.AccountId,
                Description = e.Description,
                Debit = e.Debit,
                Credit = e.Credit
            }).ToList()
        };
    }

    public async Task<bool> ApproveAsync(int id)
    {
        await Task.Delay(100);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await Task.Delay(100);
        return true;
    }

    public async Task<string> GenerateDocumentNumberAsync(int companyId, string recordType)
    {
        await Task.Delay(50);
        var prefix = recordType switch
        {
            "Tahsil" => "TAH",
            "Tediye" => "TED",
            "Mahsup" => "MAH",
            _ => "FIS"
        };
        return $"{prefix}-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
    }

    private static List<AccountingRecordDto> GetSampleRecords(int companyId)
    {
        return new List<AccountingRecordDto>
        {
            new() { Id = 1, CompanyId = companyId, DocumentNumber = "TAH-20250101-001", Date = DateTime.Now.AddDays(-5), RecordType = "Tahsil", Description = "Musteri tahsilati", TotalDebit = 10000, TotalCredit = 10000, Status = "Onaylandi" },
            new() { Id = 2, CompanyId = companyId, DocumentNumber = "TED-20250102-001", Date = DateTime.Now.AddDays(-3), RecordType = "Tediye", Description = "Tedarikci odemesi", TotalDebit = 5000, TotalCredit = 5000, Status = "Onaylandi" },
            new() { Id = 3, CompanyId = companyId, DocumentNumber = "MAH-20250103-001", Date = DateTime.Now.AddDays(-1), RecordType = "Mahsup", Description = "Gider kaydý", TotalDebit = 2500, TotalCredit = 2500, Status = "Taslak" },
        };
    }
}

// DTOs
public class AccountingRecordDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string DocumentNumber { get; set; } = "";
    public DateTime Date { get; set; }
    public string RecordType { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string Status { get; set; } = "";
    public List<AccountingEntryDto> Entries { get; set; } = new();
}

public class AccountingEntryDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class CreateAccountingRecordDto
{
    public int CompanyId { get; set; }
    public DateTime Date { get; set; }
    public string RecordType { get; set; } = "";
    public string Description { get; set; } = "";
    public List<CreateAccountingEntryDto> Entries { get; set; } = new();
}

public class CreateAccountingEntryDto
{
    public int AccountId { get; set; }
    public string Description { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}