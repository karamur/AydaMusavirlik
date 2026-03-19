using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Accounting;

namespace AydaMusavirlik.Data.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task<IEnumerable<Account>> GetByCompanyAsync(int companyId);
    Task<Account?> GetByCodeAsync(int companyId, string code);
    Task<IEnumerable<Account>> GetMainAccountsAsync(int companyId);
    Task<IEnumerable<Account>> GetSubAccountsAsync(int companyId, int parentId);
    Task<IEnumerable<Account>> GetAccountsWithBalanceAsync(int companyId, DateTime startDate, DateTime endDate);
}

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Account>> GetByCompanyAsync(int companyId)
    {
        return await _dbSet.Where(a => a.CompanyId == companyId).OrderBy(a => a.Code).ToListAsync();
    }

    public async Task<Account?> GetByCodeAsync(int companyId, string code)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Code == code);
    }

    public async Task<IEnumerable<Account>> GetMainAccountsAsync(int companyId)
    {
        return await _dbSet.Where(a => a.CompanyId == companyId && a.ParentId == null)
            .OrderBy(a => a.Code).ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetSubAccountsAsync(int companyId, int parentId)
    {
        return await _dbSet.Where(a => a.CompanyId == companyId && a.ParentId == parentId)
            .OrderBy(a => a.Code).ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetAccountsWithBalanceAsync(int companyId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(a => a.Entries.Where(e => e.AccountingRecord.DocumentDate >= startDate && e.AccountingRecord.DocumentDate <= endDate))
            .Where(a => a.CompanyId == companyId)
            .OrderBy(a => a.Code)
            .ToListAsync();
    }
}