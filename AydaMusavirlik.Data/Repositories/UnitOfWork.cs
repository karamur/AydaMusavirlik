namespace AydaMusavirlik.Data.Repositories;

public interface IUnitOfWork : IDisposable
{
    ICompanyRepository Companies { get; }
    IAccountRepository Accounts { get; }
    IAccountingRecordRepository AccountingRecords { get; }
    IEmployeeRepository Employees { get; }
    IPayrollRecordRepository PayrollRecords { get; }
    IArGeProjectRepository ArGeProjects { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;

    private ICompanyRepository? _companies;
    private IAccountRepository? _accounts;
    private IAccountingRecordRepository? _accountingRecords;
    private IEmployeeRepository? _employees;
    private IPayrollRecordRepository? _payrollRecords;
    private IArGeProjectRepository? _arGeProjects;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public ICompanyRepository Companies => _companies ??= new CompanyRepository(_context);
    public IAccountRepository Accounts => _accounts ??= new AccountRepository(_context);
    public IAccountingRecordRepository AccountingRecords => _accountingRecords ??= new AccountingRecordRepository(_context);
    public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(_context);
    public IPayrollRecordRepository PayrollRecords => _payrollRecords ??= new PayrollRecordRepository(_context);
    public IArGeProjectRepository ArGeProjects => _arGeProjects ??= new ArGeProjectRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
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