using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.ArGe;

namespace AydaMusavirlik.Data.Repositories;

public interface IArGeProjectRepository : IRepository<ArGeProject>
{
    Task<IEnumerable<ArGeProject>> GetByCompanyAsync(int companyId);
    Task<IEnumerable<ArGeProject>> GetActiveProjectsAsync(int companyId);
    Task<IEnumerable<ArGeProject>> GetIncentiveProjectsAsync(int companyId);
    Task<ArGeProject?> GetWithEmployeesAsync(int id);
    Task<decimal> GetTotalBudgetAsync(int companyId);
    Task<decimal> GetTotalActualCostAsync(int companyId);
}

public class ArGeProjectRepository : Repository<ArGeProject>, IArGeProjectRepository
{
    public ArGeProjectRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ArGeProject>> GetByCompanyAsync(int companyId)
    {
        return await _dbSet.Where(p => p.CompanyId == companyId).OrderBy(p => p.ProjectCode).ToListAsync();
    }

    public async Task<IEnumerable<ArGeProject>> GetActiveProjectsAsync(int companyId)
    {
        return await _dbSet
            .Where(p => p.CompanyId == companyId && p.Status == ArGeProjectStatus.Active)
            .OrderBy(p => p.ProjectCode)
            .ToListAsync();
    }

    public async Task<IEnumerable<ArGeProject>> GetIncentiveProjectsAsync(int companyId)
    {
        return await _dbSet
            .Where(p => p.CompanyId == companyId && p.HasIncentive)
            .OrderBy(p => p.ProjectCode)
            .ToListAsync();
    }

    public async Task<ArGeProject?> GetWithEmployeesAsync(int id)
    {
        return await _dbSet.Include(p => p.ArGeEmployees).ThenInclude(ae => ae.Employee).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<decimal> GetTotalBudgetAsync(int companyId)
    {
        return await _dbSet.Where(p => p.CompanyId == companyId).SumAsync(p => p.PlannedBudget);
    }

    public async Task<decimal> GetTotalActualCostAsync(int companyId)
    {
        return await _dbSet.Where(p => p.CompanyId == companyId).SumAsync(p => p.ActualCost);
    }
}