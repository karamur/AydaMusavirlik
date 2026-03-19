using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Payroll;

namespace AydaMusavirlik.Data.Repositories;

public interface IPayrollRecordRepository : IRepository<PayrollRecord>
{
    Task<IEnumerable<PayrollRecord>> GetByPeriodAsync(int companyId, int year, int month);
    Task<IEnumerable<PayrollRecord>> GetByEmployeeAsync(int employeeId);
    Task<PayrollRecord?> GetByEmployeeAndPeriodAsync(int employeeId, int year, int month);
    Task<decimal> GetTotalGrossSalaryAsync(int companyId, int year, int month);
    Task<decimal> GetTotalNetSalaryAsync(int companyId, int year, int month);
    Task<decimal> GetTotalSgkCostAsync(int companyId, int year, int month);
}

public class PayrollRecordRepository : Repository<PayrollRecord>, IPayrollRecordRepository
{
    public PayrollRecordRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PayrollRecord>> GetByPeriodAsync(int companyId, int year, int month)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .Where(p => p.Employee.CompanyId == companyId && p.Year == year && p.Month == month)
            .OrderBy(p => p.Employee.EmployeeNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<PayrollRecord>> GetByEmployeeAsync(int employeeId)
    {
        return await _dbSet
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync();
    }

    public async Task<PayrollRecord?> GetByEmployeeAndPeriodAsync(int employeeId, int year, int month)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.Year == year && p.Month == month);
    }

    public async Task<decimal> GetTotalGrossSalaryAsync(int companyId, int year, int month)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .Where(p => p.Employee.CompanyId == companyId && p.Year == year && p.Month == month)
            .SumAsync(p => p.GrossSalary);
    }

    public async Task<decimal> GetTotalNetSalaryAsync(int companyId, int year, int month)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .Where(p => p.Employee.CompanyId == companyId && p.Year == year && p.Month == month)
            .SumAsync(p => p.NetSalary);
    }

    public async Task<decimal> GetTotalSgkCostAsync(int companyId, int year, int month)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .Where(p => p.Employee.CompanyId == companyId && p.Year == year && p.Month == month)
            .SumAsync(p => p.SgkWorkerDeduction + p.SgkEmployerCost);
    }
}