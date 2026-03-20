using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Payroll;
using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Services;

namespace AydaMusavirlik.Desktop.Services;

public interface IPayrollService
{
    Task<IEnumerable<PayrollRecordDto>> GetByPeriodAsync(int companyId, int year, int month);
    Task<IEnumerable<PayrollRecordDto>> GetByEmployeeAsync(int employeeId);
    Task<PayrollRecordDto?> CalculateAsync(CalculatePayrollDto dto);
    Task<PayrollSummaryDto?> CalculateAllAsync(CalculateAllPayrollDto dto);
    Task<PayrollSummaryDto?> GetSummaryAsync(int companyId, int year, int month);
}

public class PayrollService : IPayrollService
{
    private readonly ISettingsService _settingsService;

    // 2025 yili parametreleri
    private const decimal SGK_WORKER_RATE = 0.14m;        // %14 SGK Isci
    private const decimal SGK_UNEMPLOYMENT_WORKER = 0.01m; // %1 Issizlik Isci
    private const decimal SGK_EMPLOYER_RATE = 0.205m;     // %20.5 SGK Isveren
    private const decimal SGK_UNEMPLOYMENT_EMPLOYER = 0.02m; // %2 Issizlik Isveren
    private const decimal STAMP_TAX_RATE = 0.00759m;      // Damga vergisi
    private const decimal MIN_WAGE_2025 = 22104.67m;      // 2025 Asgari ucret

    // Gelir Vergisi Dilimleri 2025
    private static readonly (decimal Limit, decimal Rate)[] IncomeTaxBrackets = new[]
    {
        (158000m, 0.15m),
        (330000m, 0.20m),
        (800000m, 0.27m),
        (2000000m, 0.35m),
        (decimal.MaxValue, 0.40m)
    };

    public PayrollService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<IEnumerable<PayrollRecordDto>> GetByPeriodAsync(int companyId, int year, int month)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var records = await context.PayrollRecords
                .Include(p => p.Employee)
                .Where(p => p.Employee.CompanyId == companyId && p.Year == year && p.Month == month)
                .OrderBy(p => p.Employee.LastName)
                .ToListAsync();

            return records.Select(MapToDto);
        }
        catch
        {
            return Enumerable.Empty<PayrollRecordDto>();
        }
    }

    public async Task<IEnumerable<PayrollRecordDto>> GetByEmployeeAsync(int employeeId)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var records = await context.PayrollRecords
                .Include(p => p.Employee)
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync();

            return records.Select(MapToDto);
        }
        catch
        {
            return Enumerable.Empty<PayrollRecordDto>();
        }
    }

    public async Task<PayrollRecordDto?> CalculateAsync(CalculatePayrollDto dto)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            
            var employee = await context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null)
                return null;

            // Mevcut kaydi kontrol et
            var existingRecord = await context.PayrollRecords
                .FirstOrDefaultAsync(p => p.EmployeeId == dto.EmployeeId && p.Year == dto.Year && p.Month == dto.Month);

            if (existingRecord != null)
            {
                // Guncelle
                CalculatePayroll(existingRecord, employee.GrossSalary, dto.WorkingDays);
                existingRecord.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Yeni kayit
                existingRecord = new PayrollRecord
                {
                    EmployeeId = dto.EmployeeId,
                    Year = dto.Year,
                    Month = dto.Month,
                    WorkingDays = dto.WorkingDays,
                    CreatedAt = DateTime.UtcNow
                };
                CalculatePayroll(existingRecord, employee.GrossSalary, dto.WorkingDays);
                context.PayrollRecords.Add(existingRecord);
            }

            await context.SaveChangesAsync();

            existingRecord.Employee = employee;
            return MapToDto(existingRecord);
        }
        catch
        {
            return null;
        }
    }

    public async Task<PayrollSummaryDto?> CalculateAllAsync(CalculateAllPayrollDto dto)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            
            var employees = await context.Employees
                .Where(e => e.CompanyId == dto.CompanyId && e.IsActive && !e.IsDeleted && e.TerminationDate == null)
                .ToListAsync();

            foreach (var employee in employees)
            {
                var existingRecord = await context.PayrollRecords
                    .FirstOrDefaultAsync(p => p.EmployeeId == employee.Id && p.Year == dto.Year && p.Month == dto.Month);

                if (existingRecord != null)
                {
                    CalculatePayroll(existingRecord, employee.GrossSalary, dto.WorkingDays);
                    existingRecord.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newRecord = new PayrollRecord
                    {
                        EmployeeId = employee.Id,
                        Year = dto.Year,
                        Month = dto.Month,
                        WorkingDays = dto.WorkingDays,
                        CreatedAt = DateTime.UtcNow
                    };
                    CalculatePayroll(newRecord, employee.GrossSalary, dto.WorkingDays);
                    context.PayrollRecords.Add(newRecord);
                }
            }

            await context.SaveChangesAsync();

            return await GetSummaryAsync(dto.CompanyId, dto.Year, dto.Month);
        }
        catch
        {
            return null;
        }
    }

    public async Task<PayrollSummaryDto?> GetSummaryAsync(int companyId, int year, int month)
    {
        try
        {
            using var context = DatabaseFactory.CreateContext(_settingsService.Settings.Database);
            var records = await context.PayrollRecords
                .Include(p => p.Employee)
                .Where(p => p.Employee.CompanyId == companyId && p.Year == year && p.Month == month)
                .ToListAsync();

            if (!records.Any())
                return null;

            return new PayrollSummaryDto
            {
                Year = year,
                Month = month,
                EmployeeCount = records.Count,
                TotalGross = records.Sum(r => r.GrossSalary),
                TotalNet = records.Sum(r => r.NetSalary),
                TotalSgkWorker = records.Sum(r => r.SgkWorkerDeduction),
                TotalSgkEmployer = records.Sum(r => r.SgkEmployerCost),
                TotalIncomeTax = records.Sum(r => r.IncomeTax),
                TotalCost = records.Sum(r => r.GrossSalary + r.SgkEmployerCost)
            };
        }
        catch
        {
            return null;
        }
    }

    private void CalculatePayroll(PayrollRecord record, decimal grossSalary, int workingDays)
    {
        // Calisan gun hesabi (30 gun uzerinden)
        var effectiveGross = grossSalary * workingDays / 30m;
        record.GrossSalary = Math.Round(effectiveGross, 2);

        // SGK Kesintileri (Isci Payi)
        record.SgkWorkerDeduction = Math.Round(effectiveGross * SGK_WORKER_RATE, 2);
        var sgkUnemploymentWorker = Math.Round(effectiveGross * SGK_UNEMPLOYMENT_WORKER, 2);

        // SGK Matrah
        var sgkMatrah = effectiveGross - record.SgkWorkerDeduction - sgkUnemploymentWorker;

        // Gelir Vergisi (kumulatif hesap basitlestirilmis)
        record.IncomeTax = CalculateIncomeTax(sgkMatrah);

        // Damga Vergisi
        record.StampTax = Math.Round(effectiveGross * STAMP_TAX_RATE, 2);

        // Net Maas
        record.NetSalary = Math.Round(
            effectiveGross - record.SgkWorkerDeduction - sgkUnemploymentWorker - record.IncomeTax - record.StampTax, 
            2);

        // Isveren Maliyeti
        record.SgkEmployerCost = Math.Round(effectiveGross * (SGK_EMPLOYER_RATE + SGK_UNEMPLOYMENT_EMPLOYER), 2);
    }

    private decimal CalculateIncomeTax(decimal taxableIncome)
    {
        decimal tax = 0;
        decimal remainingIncome = taxableIncome * 12; // Yillik gelir tahmini
        decimal previousLimit = 0;

        foreach (var (limit, rate) in IncomeTaxBrackets)
        {
            if (remainingIncome <= 0)
                break;

            var bracketAmount = Math.Min(remainingIncome, limit - previousLimit);
            tax += bracketAmount * rate;
            remainingIncome -= bracketAmount;
            previousLimit = limit;
        }

        // Aylik vergiye cevir
        return Math.Round(tax / 12, 2);
    }

    private static PayrollRecordDto MapToDto(PayrollRecord p) => new()
    {
        Id = p.Id,
        EmployeeId = p.EmployeeId,
        EmployeeName = p.Employee != null ? $"{p.Employee.FirstName} {p.Employee.LastName}" : "",
        Year = p.Year,
        Month = p.Month,
        GrossSalary = p.GrossSalary,
        SgkWorkerDeduction = p.SgkWorkerDeduction,
        SgkUnemploymentWorker = Math.Round(p.GrossSalary * SGK_UNEMPLOYMENT_WORKER, 2),
        IncomeTax = p.IncomeTax,
        StampTax = p.StampTax,
        TotalDeductions = p.SgkWorkerDeduction + p.IncomeTax + p.StampTax,
        NetSalary = p.NetSalary,
        SgkEmployerCost = p.SgkEmployerCost,
        TotalEmployerCost = p.GrossSalary + p.SgkEmployerCost,
        Status = "Hesaplandi"
    };
}

public class PayrollRecordDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal SgkWorkerDeduction { get; set; }
    public decimal SgkUnemploymentWorker { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal StampTax { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public decimal SgkEmployerCost { get; set; }
    public decimal TotalEmployerCost { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CalculatePayrollDto
{
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int WorkingDays { get; set; } = 30;
}

public class CalculateAllPayrollDto
{
    public int CompanyId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int WorkingDays { get; set; } = 30;
}

public class PayrollSummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalSgkWorker { get; set; }
    public decimal TotalSgkEmployer { get; set; }
    public decimal TotalIncomeTax { get; set; }
    public decimal TotalCost { get; set; }
}