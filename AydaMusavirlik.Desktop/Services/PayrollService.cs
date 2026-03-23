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
        await Task.Delay(100);
        return GetSamplePayrollRecords(companyId, year, month);
    }

    public async Task<IEnumerable<PayrollRecordDto>> GetByEmployeeAsync(int employeeId)
    {
        await Task.Delay(100);
        return GetSamplePayrollRecords(1, 2025, 1).Where(p => p.EmployeeId == employeeId);
    }

    public async Task<PayrollRecordDto?> CalculateAsync(CalculatePayrollDto dto)
    {
        await Task.Delay(100);
        
        var grossSalary = dto.GrossSalary;
        var sgkWorker = grossSalary * SGK_WORKER_RATE;
        var unemploymentWorker = grossSalary * SGK_UNEMPLOYMENT_WORKER;
        var taxBase = grossSalary - sgkWorker - unemploymentWorker;
        var incomeTax = CalculateIncomeTax(taxBase, 0);
        var stampTax = grossSalary * STAMP_TAX_RATE;
        var netSalary = grossSalary - sgkWorker - unemploymentWorker - incomeTax - stampTax;

        return new PayrollRecordDto
        {
            Id = new Random().Next(1000, 9999),
            EmployeeId = dto.EmployeeId,
            CompanyId = dto.CompanyId,
            Year = dto.Year,
            Month = dto.Month,
            GrossSalary = grossSalary,
            NetSalary = netSalary,
            SgkWorker = sgkWorker,
            SgkEmployer = grossSalary * SGK_EMPLOYER_RATE,
            UnemploymentWorker = unemploymentWorker,
            UnemploymentEmployer = grossSalary * SGK_UNEMPLOYMENT_EMPLOYER,
            IncomeTax = incomeTax,
            StampTax = stampTax,
            TotalCost = grossSalary + (grossSalary * SGK_EMPLOYER_RATE) + (grossSalary * SGK_UNEMPLOYMENT_EMPLOYER)
        };
    }

    public async Task<PayrollSummaryDto?> CalculateAllAsync(CalculateAllPayrollDto dto)
    {
        await Task.Delay(100);
        var records = GetSamplePayrollRecords(dto.CompanyId, dto.Year, dto.Month);
        
        return new PayrollSummaryDto
        {
            CompanyId = dto.CompanyId,
            Year = dto.Year,
            Month = dto.Month,
            EmployeeCount = records.Count(),
            TotalGross = records.Sum(r => r.GrossSalary),
            TotalNet = records.Sum(r => r.NetSalary),
            TotalSgk = records.Sum(r => r.SgkWorker + r.SgkEmployer),
            TotalTax = records.Sum(r => r.IncomeTax + r.StampTax),
            TotalCost = records.Sum(r => r.TotalCost),
            Payrolls = records.ToList()
        };
    }

    public async Task<PayrollSummaryDto?> GetSummaryAsync(int companyId, int year, int month)
    {
        return await CalculateAllAsync(new CalculateAllPayrollDto
        {
            CompanyId = companyId,
            Year = year,
            Month = month
        });
    }

    private decimal CalculateIncomeTax(decimal taxBase, decimal cumulativeBase)
    {
        decimal tax = 0;
        decimal remaining = taxBase;

        foreach (var (limit, rate) in IncomeTaxBrackets)
        {
            if (cumulativeBase >= limit)
                continue;

            var bracket = Math.Min(remaining, limit - cumulativeBase);
            if (bracket <= 0)
                break;

            tax += bracket * rate;
            remaining -= bracket;
            cumulativeBase += bracket;

            if (remaining <= 0)
                break;
        }

        return Math.Round(tax, 2);
    }

    private static List<PayrollRecordDto> GetSamplePayrollRecords(int companyId, int year, int month)
    {
        return new List<PayrollRecordDto>
        {
            new() { Id = 1, EmployeeId = 1, EmployeeName = "Ahmet Yilmaz", FullName = "Ahmet Yilmaz", Position = "Software Developer", CompanyId = companyId, Year = year, Month = month, GrossSalary = 30000, NetSalary = 23500, SgkWorker = 4200, SgkEmployer = 6150, IncomeTax = 1800, StampTax = 227, TotalCost = 36750 },
            new() { Id = 2, EmployeeId = 2, EmployeeName = "Ayse Demir", FullName = "Ayse Demir", Position = "Product Manager", CompanyId = companyId, Year = year, Month = month, GrossSalary = 25000, NetSalary = 19800, SgkWorker = 3500, SgkEmployer = 5125, IncomeTax = 1350, StampTax = 190, TotalCost = 30625 },
            new() { Id = 3, EmployeeId = 3, EmployeeName = "Mehmet Kaya", FullName = "Mehmet Kaya", Position = "Designer", CompanyId = companyId, Year = year, Month = month, GrossSalary = 22104, NetSalary = 17500, SgkWorker = 3095, SgkEmployer = 4531, IncomeTax = 1150, StampTax = 168, TotalCost = 27135 },
        };
    }
}

// DTOs
public class PayrollRecordDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Position { get; set; } = "";
    public int CompanyId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string Period { get; set; } = "";
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal SgkWorker { get; set; }
    public decimal SgkEmployer { get; set; }
    public decimal UnemploymentWorker { get; set; }
    public decimal UnemploymentEmployer { get; set; }
    public decimal SgkUnemploymentWorker { get; set; }
    public decimal SgkUnemploymentEmployer { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal StampTax { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalEmployerCost { get; set; }
    public decimal Deductions { get; set; }
    public decimal SgkWorkerDeduction { get; set; }
    public decimal SgkEmployerCost { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal WorkDays { get; set; } = 30;
    public decimal OvertimeHours { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal SeverancePay { get; set; }
    public decimal AgiAmount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
}

public class CalculatePayrollDto
{
    public int EmployeeId { get; set; }
    public int CompanyId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal GrossSalary { get; set; }
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
    public int CompanyId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalSgk { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalCost { get; set; }
    public List<PayrollRecordDto> Payrolls { get; set; } = new();
}