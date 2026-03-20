using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Payroll;
using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Services;

namespace AydaMusavirlik.Desktop.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetByCompanyAsync(int companyId);
    Task<IEnumerable<EmployeeDto>> GetActiveByCompanyAsync(int companyId);
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto?> CreateAsync(CreateEmployeeDto dto);
    Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task<bool> DeleteAsync(int id);
}

public class EmployeeService : IEmployeeService
{
    private readonly ISettingsService _settingsService;

    public EmployeeService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<IEnumerable<EmployeeDto>> GetByCompanyAsync(int companyId)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var employees = await context.Employees
                .Where(e => e.CompanyId == companyId && !e.IsDeleted)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToDto);
        }
        catch
        {
            return Enumerable.Empty<EmployeeDto>();
        }
    }

    public async Task<IEnumerable<EmployeeDto>> GetActiveByCompanyAsync(int companyId)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var employees = await context.Employees
                .Where(e => e.CompanyId == companyId && !e.IsDeleted && e.IsActive && e.TerminationDate == null)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToDto);
        }
        catch
        {
            return Enumerable.Empty<EmployeeDto>();
        }
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var employee = await context.Employees.FindAsync(id);
            
            if (employee == null || employee.IsDeleted)
                return null;

            return MapToDto(employee);
        }
        catch
        {
            return null;
        }
    }

    public async Task<EmployeeDto?> CreateAsync(CreateEmployeeDto dto)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            
            // Sicil numarasi olustur
            var employeeNumber = await GenerateEmployeeNumberAsync(context, dto.CompanyId);

            var employee = new Employee
            {
                CompanyId = dto.CompanyId,
                EmployeeNumber = employeeNumber,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                TcKimlikNo = dto.TcKimlikNo,
                SgkNumber = dto.SgkNumber,
                Department = dto.Department,
                Position = dto.Position,
                HireDate = dto.HireDate,
                GrossSalary = dto.GrossSalary,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            return MapToDto(employee);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var employee = await context.Employees.FindAsync(id);
            
            if (employee == null)
                return false;

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.TcKimlikNo = dto.TcKimlikNo;
            employee.SgkNumber = dto.SgkNumber;
            employee.Department = dto.Department;
            employee.Position = dto.Position;
            employee.GrossSalary = dto.GrossSalary;
            employee.TerminationDate = dto.TerminationDate;
            employee.UpdatedAt = DateTime.UtcNow;

            if (dto.TerminationDate.HasValue)
            {
                employee.IsActive = false;
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var employee = await context.Employees.FindAsync(id);
            
            if (employee == null)
                return false;

            employee.IsDeleted = true;
            employee.DeletedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> GenerateEmployeeNumberAsync(AppDbContext context, int companyId)
    {
        var year = DateTime.Now.Year;
        var lastEmployee = await context.Employees
            .Where(e => e.CompanyId == companyId && e.EmployeeNumber.StartsWith($"P{year}"))
            .OrderByDescending(e => e.EmployeeNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastEmployee != null)
        {
            var parts = lastEmployee.EmployeeNumber.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"P{year}-{nextNumber:D4}";
    }

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        Id = e.Id,
        CompanyId = e.CompanyId,
        EmployeeNumber = e.EmployeeNumber,
        FirstName = e.FirstName,
        LastName = e.LastName,
        FullName = $"{e.FirstName} {e.LastName}",
        TcKimlikNo = e.TcKimlikNo,
        SgkNumber = e.SgkNumber,
        Department = e.Department,
        Position = e.Position,
        HireDate = e.HireDate,
        TerminationDate = e.TerminationDate,
        GrossSalary = e.GrossSalary,
        IsActive = e.IsActive
    };
}

public class EmployeeDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public string? SgkNumber { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal GrossSalary { get; set; }
    public bool IsActive { get; set; }
}

public class CreateEmployeeDto
{
    public int CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public string? SgkNumber { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public DateTime HireDate { get; set; }
    public decimal GrossSalary { get; set; }
}

public class UpdateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public string? SgkNumber { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public decimal GrossSalary { get; set; }
    public DateTime? TerminationDate { get; set; }
}