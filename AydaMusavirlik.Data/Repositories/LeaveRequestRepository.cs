using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Payroll;

namespace AydaMusavirlik.Data.Repositories;

public interface ILeaveRequestRepository : IRepository<LeaveRequest>
{
    Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId);
    Task<IEnumerable<LeaveRequest>> GetByCompanyAsync(int companyId);
    Task<IEnumerable<LeaveRequest>> GetPendingByCompanyAsync(int companyId);
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(int companyId, LeaveStatus status);
    Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(int companyId, DateTime start, DateTime end);
    Task<int> GetUsedDaysAsync(int employeeId, LeaveType leaveType, int year);
    Task<int> GetRemainingDaysAsync(int employeeId, int year);
    Task<LeaveRequest?> GetWithDetailsAsync(int id);
    Task<bool> HasOverlappingLeaveAsync(int employeeId, DateTime start, DateTime end, int? excludeId = null);
}

public class LeaveRequestRepository : Repository<LeaveRequest>, ILeaveRequestRepository
{
    public LeaveRequestRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId)
    {
        return await _dbSet
            .Include(l => l.Employee)
            .Include(l => l.ApprovedBy)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByCompanyAsync(int companyId)
    {
        return await _dbSet
            .Include(l => l.Employee)
            .Include(l => l.ApprovedBy)
            .Where(l => l.CompanyId == companyId)
            .OrderByDescending(l => l.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingByCompanyAsync(int companyId)
    {
        return await _dbSet
            .Include(l => l.Employee)
            .Where(l => l.CompanyId == companyId && l.Status == LeaveStatus.Pending)
            .OrderBy(l => l.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(int companyId, LeaveStatus status)
    {
        return await _dbSet
            .Include(l => l.Employee)
            .Include(l => l.ApprovedBy)
            .Where(l => l.CompanyId == companyId && l.Status == status)
            .OrderByDescending(l => l.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(int companyId, DateTime start, DateTime end)
    {
        return await _dbSet
            .Include(l => l.Employee)
            .Where(l => l.CompanyId == companyId && 
                       l.StartDate <= end && l.EndDate >= start)
            .OrderBy(l => l.StartDate)
            .ToListAsync();
    }

    public async Task<int> GetUsedDaysAsync(int employeeId, LeaveType leaveType, int year)
    {
        return await _dbSet
            .Where(l => l.EmployeeId == employeeId && 
                       l.LeaveType == leaveType &&
                       l.StartDate.Year == year &&
                       (l.Status == LeaveStatus.Approved || l.Status == LeaveStatus.InProgress))
            .SumAsync(l => l.TotalDays);
    }

    public async Task<int> GetRemainingDaysAsync(int employeeId, int year)
    {
        var employee = await _context.Set<Employee>().FindAsync(employeeId);
        if (employee == null) return 0;

        // Çalýþma yýlýna göre hak edilen izin
        var workYears = (DateTime.Now - employee.HireDate).Days / 365;
        var entitledDays = workYears < 1 ? 0 : (workYears < 5 ? 14 : (workYears < 15 ? 20 : 26));

        // Kullanýlan izin
        var usedDays = await GetUsedDaysAsync(employeeId, LeaveType.Annual, year);

        return entitledDays - usedDays;
    }

    public async Task<LeaveRequest?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(l => l.Employee)
            .Include(l => l.ApprovedBy)
            .Include(l => l.DeputyEmployee)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<bool> HasOverlappingLeaveAsync(int employeeId, DateTime start, DateTime end, int? excludeId = null)
    {
        var query = _dbSet.Where(l => l.EmployeeId == employeeId &&
                                     l.Status != LeaveStatus.Rejected &&
                                     l.Status != LeaveStatus.Cancelled &&
                                     l.StartDate <= end && l.EndDate >= start);

        if (excludeId.HasValue)
            query = query.Where(l => l.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}