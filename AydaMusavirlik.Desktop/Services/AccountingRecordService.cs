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
    Task<string> GenerateDocumentNumberAsync(int companyId, RecordType type);
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
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var records = await context.AccountingRecords
                .Include(r => r.Entries)
                .Where(r => r.CompanyId == companyId && !r.IsDeleted)
                .OrderByDescending(r => r.DocumentDate)
                .ThenByDescending(r => r.DocumentNumber)
                .ToListAsync();

            return records.Select(r => new AccountingRecordDto
            {
                Id = r.Id,
                CompanyId = r.CompanyId,
                DocumentNumber = r.DocumentNumber,
                Date = r.DocumentDate,
                RecordType = r.RecordType,
                Description = r.Description,
                TotalDebit = r.TotalDebit,
                TotalCredit = r.TotalCredit,
                Status = r.Status,
                Entries = r.Entries.Select(e => new AccountingEntryDto
                {
                    Id = e.Id,
                    AccountId = e.AccountId,
                    Description = e.Description,
                    Debit = e.Debit,
                    Credit = e.Credit
                }).ToList()
            });
        }
        catch
        {
            return Enumerable.Empty<AccountingRecordDto>();
        }
    }

    public async Task<AccountingRecordDto?> GetByIdAsync(int id)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var record = await context.AccountingRecords
                .Include(r => r.Entries)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (record == null)
                return null;

            return new AccountingRecordDto
            {
                Id = record.Id,
                CompanyId = record.CompanyId,
                DocumentNumber = record.DocumentNumber,
                Date = record.DocumentDate,
                RecordType = record.RecordType,
                Description = record.Description,
                TotalDebit = record.TotalDebit,
                TotalCredit = record.TotalCredit,
                Status = record.Status,
                Entries = record.Entries.Select(e => new AccountingEntryDto
                {
                    Id = e.Id,
                    AccountId = e.AccountId,
                    Description = e.Description,
                    Debit = e.Debit,
                    Credit = e.Credit
                }).ToList()
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<AccountingRecordDto?> CreateAsync(CreateAccountingRecordDto dto)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);

            // Fis numarasi olustur
            var documentNumber = await GenerateDocumentNumberInternalAsync(context, dto.CompanyId, dto.RecordType);

            var record = new AccountingRecord
            {
                CompanyId = dto.CompanyId,
                DocumentNumber = documentNumber,
                DocumentDate = dto.Date,
                RecordType = dto.RecordType,
                Description = dto.Description ?? string.Empty,
                TotalDebit = dto.Entries.Sum(e => e.Debit),
                TotalCredit = dto.Entries.Sum(e => e.Credit),
                Status = RecordStatus.Draft,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Satirlari ekle
            foreach (var entryDto in dto.Entries)
            {
                record.Entries.Add(new AccountingEntry
                {
                    AccountId = entryDto.AccountId,
                    Description = entryDto.Description,
                    Debit = entryDto.Debit,
                    Credit = entryDto.Credit,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.AccountingRecords.Add(record);
            await context.SaveChangesAsync();

            return new AccountingRecordDto
            {
                Id = record.Id,
                DocumentNumber = record.DocumentNumber,
                Date = record.DocumentDate,
                TotalDebit = record.TotalDebit,
                TotalCredit = record.TotalCredit,
                Status = record.Status
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ApproveAsync(int id)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var record = await context.AccountingRecords
                .Include(r => r.Entries)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null)
                return false;

            // Borc-Alacak kontrolu
            if (record.TotalDebit != record.TotalCredit)
                return false;

            record.Status = RecordStatus.Approved;
            record.UpdatedAt = DateTime.UtcNow;

            // Hesap bakiyelerini guncelle
            foreach (var entry in record.Entries)
            {
                var account = await context.Accounts.FindAsync(entry.AccountId);
                if (account != null)
                {
                    if (account.Nature == AccountNature.Debit)
                    {
                        account.CurrentBalance += entry.Debit - entry.Credit;
                    }
                    else
                    {
                        account.CurrentBalance += entry.Credit - entry.Debit;
                    }
                }
            }

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
            var record = await context.AccountingRecords.FindAsync(id);

            if (record == null)
                return false;

            // Sadece taslak fisler silinebilir
            if (record.Status != RecordStatus.Draft)
                return false;

            record.IsDeleted = true;
            record.DeletedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GenerateDocumentNumberAsync(int companyId, RecordType type)
    {
        using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
        return await GenerateDocumentNumberInternalAsync(context, companyId, type);
    }

    private async Task<string> GenerateDocumentNumberInternalAsync(AppDbContext context, int companyId, RecordType type)
    {
        var year = DateTime.Now.Year;
        var prefix = type switch
        {
            RecordType.TahsilatFisi => "TAH",
            RecordType.OdemeFisi => "ODE",
            RecordType.MahsupFisi => "MHS",
            RecordType.AcilisFisi => "ACI",
            RecordType.KapanisFisi => "KAP",
            RecordType.SatisFaturasi => "SAT",
            RecordType.AlisFaturasi => "ALI",
            _ => "FIS"
        };

        var lastRecord = await context.AccountingRecords
            .Where(r => r.CompanyId == companyId && r.DocumentDate.Year == year && r.DocumentNumber.StartsWith(prefix))
            .OrderByDescending(r => r.DocumentNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastRecord != null)
        {
            var parts = lastRecord.DocumentNumber.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}-{year}-{nextNumber:D5}";
    }
}

public class AccountingRecordDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public RecordType RecordType { get; set; }
    public string? Description { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public RecordStatus Status { get; set; }
    public List<AccountingEntryDto> Entries { get; set; } = new();
}

public class CreateAccountingRecordDto
{
    public int CompanyId { get; set; }
    public DateTime Date { get; set; }
    public RecordType RecordType { get; set; }
    public string? Description { get; set; }
    public List<CreateAccountingEntryDto> Entries { get; set; } = new();
}

public class AccountingEntryDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class CreateAccountingEntryDto
{
    public int AccountId { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}