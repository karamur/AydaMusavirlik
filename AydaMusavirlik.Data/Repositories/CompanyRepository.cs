using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Data.Repositories;

public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByTaxNumberAsync(string taxNumber);
    Task<IEnumerable<Company>> GetActiveCompaniesAsync();
    Task<Company?> GetWithContactsAsync(int id);
}

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    public CompanyRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Company?> GetByTaxNumberAsync(string taxNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.TaxNumber == taxNumber);
    }

    public async Task<IEnumerable<Company>> GetActiveCompaniesAsync()
    {
        return await _dbSet.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Company?> GetWithContactsAsync(int id)
    {
        return await _dbSet.Include(c => c.Contacts).FirstOrDefaultAsync(c => c.Id == id);
    }
}