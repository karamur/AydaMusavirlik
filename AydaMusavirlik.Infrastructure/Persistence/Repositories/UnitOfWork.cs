using Microsoft.EntityFrameworkCore.Storage;
using AydaMusavirlik.Application.Common.Interfaces;

namespace AydaMusavirlik.Infrastructure.Persistence.Repositories;

/// <summary>
/// Unit of Work Implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private ICompanyRepository? _companies;
    private IUserRepository? _users;
    private IAccountRepository? _accounts;
    private IAccountingRecordRepository? _accountingRecords;
    private IEmployeeRepository? _employees;
    private IPayrollRepository? _payrolls;
    private ISgkBelgeTuruRepository? _sgkBelgeTurleri;
    private IKanuniKesintiRepository? _kanuniKesintiler;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ICompanyRepository Companies => _companies ??= new CompanyRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IAccountRepository Accounts => _accounts ??= new AccountRepository(_context);
    public IAccountingRecordRepository AccountingRecords => _accountingRecords ??= new AccountingRecordRepository(_context);
    public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(_context);
    public IPayrollRepository Payrolls => _payrolls ??= new PayrollRepository(_context);
    public ISgkBelgeTuruRepository SgkBelgeTurleri => _sgkBelgeTurleri ??= new SgkBelgeTuruRepository(_context);
    public IKanuniKesintiRepository KanuniKesintiler => _kanuniKesintiler ??= new KanuniKesintiRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}