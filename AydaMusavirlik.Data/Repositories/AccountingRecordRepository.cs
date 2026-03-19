using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Accounting;

namespace AydaMusavirlik.Data.Repositories;

public interface IAccountingRecordRepository : IRepository<AccountingRecord>
{
    Task<IEnumerable<AccountingRecord>> GetByCompanyAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null);
    Task<AccountingRecord?> GetWithEntriesAsync(int id);
    Task<string> GenerateDocumentNumberAsync(int companyId, RecordType recordType);
    Task<IEnumerable<AccountingRecord>> GetPendingRecordsAsync(int companyId);
    Task<decimal> GetTotalDebitAsync(int companyId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalCreditAsync(int companyId, DateTime startDate, DateTime endDate);
}

public class AccountingRecordRepository : Repository<AccountingRecord>, IAccountingRecordRepository
{
    public AccountingRecordRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AccountingRecord>> GetByCompanyAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet.Where(r => r.CompanyId == companyId);

        if (startDate.HasValue)
            query = query.Where(r => r.DocumentDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.DocumentDate <= endDate.Value);

        return await query.OrderByDescending(r => r.DocumentDate).ThenByDescending(r => r.DocumentNumber).ToListAsync();
    }

    public async Task<AccountingRecord?> GetWithEntriesAsync(int id)
    {
        return await _dbSet.Include(r => r.Entries).ThenInclude(e => e.Account).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<string> GenerateDocumentNumberAsync(int companyId, RecordType recordType)
    {
        var year = DateTime.Now.Year;
        var prefix = recordType switch
        {
            RecordType.MahsupFisi => "MHS",
            RecordType.TahsilatFisi => "THS",
            RecordType.OdemeFisi => "ODM",
            RecordType.AcilisFisi => "ACL",
            RecordType.KapanisFisi => "KPN",
            RecordType.SatisFaturasi => "STF",
            RecordType.AlisFaturasi => "ALF",
            RecordType.DekontFisi => "DKN",
            _ => "FIS"
        };

        var lastRecord = await _dbSet
            .Where(r => r.CompanyId == companyId && r.DocumentNumber.StartsWith($"{prefix}-{year}"))
            .OrderByDescending(r => r.DocumentNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastRecord != null)
        {
            var parts = lastRecord.DocumentNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}-{year}-{nextNumber:D6}";
    }

    public async Task<IEnumerable<AccountingRecord>> GetPendingRecordsAsync(int companyId)
    {
        return await _dbSet
            .Where(r => r.CompanyId == companyId && r.Status == RecordStatus.Draft)
            .OrderBy(r => r.DocumentDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalDebitAsync(int companyId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(r => r.CompanyId == companyId && r.DocumentDate >= startDate && r.DocumentDate <= endDate)
            .SumAsync(r => r.TotalDebit);
    }

    public async Task<decimal> GetTotalCreditAsync(int companyId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(r => r.CompanyId == companyId && r.DocumentDate >= startDate && r.DocumentDate <= endDate)
            .SumAsync(r => r.TotalCredit);
    }
}