using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Common;
using AydaMusavirlik.Core.Models.Accounting;
using AydaMusavirlik.Core.Models.Payroll;
using AydaMusavirlik.Application.Common.Interfaces;

namespace AydaMusavirlik.Infrastructure.Persistence.Repositories;

/// <summary>
/// Company Repository
/// </summary>
public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    public CompanyRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Company?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.TaxNumber == taxNumber, cancellationToken);
    }

    public async Task<IEnumerable<Company>> GetActiveCompaniesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }
}

/// <summary>
/// User Repository
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return !await _dbSet.AnyAsync(u => u.Username == username && (excludeId == null || u.Id != excludeId), cancellationToken);
    }
}

/// <summary>
/// Account Repository
/// </summary>
public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Account>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(a => a.CompanyId == companyId).OrderBy(a => a.Code).ToListAsync(cancellationToken);
    }

    public async Task<Account?> GetByCodeAsync(int companyId, string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetChildAccountsAsync(int companyId, string parentCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(a => a.CompanyId == companyId && a.Code.StartsWith(parentCode) && a.Code != parentCode)
            .OrderBy(a => a.Code).ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Accounting Record Repository
/// </summary>
public class AccountingRecordRepository : Repository<AccountingRecord>, IAccountingRecordRepository
{
    public AccountingRecordRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<AccountingRecord>> GetByCompanyAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(r => r.Entries).Where(r => r.CompanyId == companyId);

        if (startDate.HasValue) query = query.Where(r => r.DocumentDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(r => r.DocumentDate <= endDate.Value);

        return await query.OrderByDescending(r => r.DocumentDate).ThenByDescending(r => r.DocumentNumber).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccountingRecord>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(r => r.Entries)
            .Where(r => r.Entries.Any(e => e.AccountId == accountId))
            .OrderByDescending(r => r.DocumentDate).ToListAsync(cancellationToken);
    }

    public async Task<string> GetNextVoucherNumberAsync(int companyId, DateTime date, CancellationToken cancellationToken = default)
    {
        var prefix = $"{date:yyyyMM}";
        var lastRecord = await _dbSet
            .Where(r => r.CompanyId == companyId && r.DocumentNumber.StartsWith(prefix))
            .OrderByDescending(r => r.DocumentNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastRecord == null) return $"{prefix}-0001";

        var lastNumber = int.Parse(lastRecord.DocumentNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }
}

/// <summary>
/// Employee Repository
/// </summary>
public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Employee>> GetByCompanyAsync(int companyId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(e => e.SgkBelgeTuru).Where(e => e.CompanyId == companyId);
        if (activeOnly) query = query.Where(e => e.IsActive && e.TerminationDate == null);
        return await query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync(cancellationToken);
    }

    public async Task<Employee?> GetByTcKimlikAsync(string tcKimlik, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.TcKimlikNo == tcKimlik, cancellationToken);
    }

    public async Task<Employee?> GetWithPayrollsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(e => e.PayrollRecords).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}

/// <summary>
/// Payroll Repository
/// </summary>
public class PayrollRepository : Repository<PayrollRecord>, IPayrollRepository
{
    public PayrollRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<PayrollRecord>> GetByPeriodAsync(int companyId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(p => p.Employee)
            .Where(p => p.Employee.CompanyId == companyId && p.Year == year && p.Month == month)
            .OrderBy(p => p.Employee.LastName).ThenBy(p => p.Employee.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PayrollRecord>> GetByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);
    }

    public async Task<PayrollRecord?> GetByEmployeePeriodAsync(int employeeId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.Year == year && p.Month == month, cancellationToken);
    }
}

/// <summary>
/// SGK Belge Turu Repository
/// </summary>
public class SgkBelgeTuruRepository : Repository<SgkBelgeTuru>, ISgkBelgeTuruRepository
{
    public SgkBelgeTuruRepository(ApplicationDbContext context) : base(context) { }

    public async Task<SgkBelgeTuru?> GetByKodAsync(string kod, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(b => b.Kod == kod, cancellationToken);
    }

    public async Task<IEnumerable<SgkBelgeTuru>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(b => b.IsActive).OrderBy(b => b.Kod).ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Kanuni Kesinti Repository
/// </summary>
public class KanuniKesintiRepository : Repository<KanuniKesinti>, IKanuniKesintiRepository
{
    public KanuniKesintiRepository(ApplicationDbContext context) : base(context) { }

    public async Task<KanuniKesinti?> GetByPeriodAsync(int yil, int ay, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(k => k.Yil == yil && k.Ay <= ay && k.IsActive)
            .OrderByDescending(k => k.Ay).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<KanuniKesinti?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        return await GetByPeriodAsync(now.Year, now.Month, cancellationToken);
    }
}