using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Payroll;

namespace AydaMusavirlik.Data.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<IEnumerable<Employee>> GetByCompanyAsync(int companyId);
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync(int companyId);
    Task<Employee?> GetByTcKimlikAsync(string tcKimlik);
    Task<Employee?> GetWithPayrollsAsync(int id);
}

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Employee>> GetByCompanyAsync(int companyId)
    {
        return await _dbSet.Where(e => e.CompanyId == companyId).OrderBy(e => e.EmployeeNumber).ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync(int companyId)
    {
        return await _dbSet
            .Where(e => e.CompanyId == companyId && e.TerminationDate == null)
            .OrderBy(e => e.EmployeeNumber)
            .ToListAsync();
    }

    public async Task<Employee?> GetByTcKimlikAsync(string tcKimlik)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.TcKimlikNo == tcKimlik);
    }

    public async Task<Employee?> GetWithPayrollsAsync(int id)
    {
        return await _dbSet.Include(e => e.PayrollRecords).FirstOrDefaultAsync(e => e.Id == id);
    }
}