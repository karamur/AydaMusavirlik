using AydaMusavirlik.Core.Models.Common;
using AydaMusavirlik.Core.Models.Accounting;
using AydaMusavirlik.Core.Models.Payroll;

namespace AydaMusavirlik.Application.Common.Interfaces;

/// <summary>
/// Company Repository Interface
/// </summary>
public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Company>> GetActiveCompaniesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// User Repository Interface
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameUniqueAsync(string username, int? excludeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Account Repository Interface
/// </summary>
public interface IAccountRepository : IRepository<Account>
{
    Task<IEnumerable<Account>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken = default);
    Task<Account?> GetByCodeAsync(int companyId, string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetChildAccountsAsync(int companyId, string parentCode, CancellationToken cancellationToken = default);
}

/// <summary>
/// Accounting Record Repository Interface
/// </summary>
public interface IAccountingRecordRepository : IRepository<AccountingRecord>
{
    Task<IEnumerable<AccountingRecord>> GetByCompanyAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountingRecord>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default);
    Task<string> GetNextVoucherNumberAsync(int companyId, DateTime date, CancellationToken cancellationToken = default);
}

/// <summary>
/// Employee Repository Interface
/// </summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    Task<IEnumerable<Employee>> GetByCompanyAsync(int companyId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<Employee?> GetByTcKimlikAsync(string tcKimlik, CancellationToken cancellationToken = default);
    Task<Employee?> GetWithPayrollsAsync(int id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Payroll Repository Interface
/// </summary>
public interface IPayrollRepository : IRepository<PayrollRecord>
{
    Task<IEnumerable<PayrollRecord>> GetByPeriodAsync(int companyId, int year, int month, CancellationToken cancellationToken = default);
    Task<IEnumerable<PayrollRecord>> GetByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<PayrollRecord?> GetByEmployeePeriodAsync(int employeeId, int year, int month, CancellationToken cancellationToken = default);
}

/// <summary>
/// SGK Belge Turu Repository Interface
/// </summary>
public interface ISgkBelgeTuruRepository : IRepository<SgkBelgeTuru>
{
    Task<SgkBelgeTuru?> GetByKodAsync(string kod, CancellationToken cancellationToken = default);
    Task<IEnumerable<SgkBelgeTuru>> GetActiveAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Kanuni Kesinti Repository Interface
/// </summary>
public interface IKanuniKesintiRepository : IRepository<KanuniKesinti>
{
    Task<KanuniKesinti?> GetByPeriodAsync(int yil, int ay, CancellationToken cancellationToken = default);
    Task<KanuniKesinti?> GetCurrentAsync(CancellationToken cancellationToken = default);
}