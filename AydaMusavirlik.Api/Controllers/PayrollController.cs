using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AydaMusavirlik.Data.Repositories;
using AydaMusavirlik.Core.Models.Payroll;

namespace AydaMusavirlik.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public PayrollController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("company/{companyId}/{year}/{month}")]
    public async Task<ActionResult<IEnumerable<PayrollRecordDto>>> GetByPeriod(int companyId, int year, int month)
    {
        var payrolls = await _unitOfWork.PayrollRecords.GetByPeriodAsync(companyId, year, month);
        return Ok(payrolls.Select(p => MapToDto(p)));
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<PayrollRecordDto>>> GetByEmployee(int employeeId)
    {
        var payrolls = await _unitOfWork.PayrollRecords.GetByEmployeeAsync(employeeId);
        return Ok(payrolls.Select(p => MapToDto(p)));
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<PayrollRecordDto>> Calculate(CalculatePayrollDto dto)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.EmployeeId);
        if (employee == null)
            return NotFound("Personel bulunamadi");

        var existing = await _unitOfWork.PayrollRecords.GetByEmployeeAndPeriodAsync(dto.EmployeeId, dto.Year, dto.Month);
        if (existing != null)
            return BadRequest("Bu donem icin bordro zaten mevcut");

        var payroll = CalculatePayroll(employee, dto);
        await _unitOfWork.PayrollRecords.AddAsync(payroll);

        return Ok(MapToDto(payroll));
    }

    [HttpPost("calculate-all")]
    public async Task<ActionResult<PayrollSummaryDto>> CalculateAll(CalculateAllPayrollDto dto)
    {
        var employees = await _unitOfWork.Employees.GetActiveEmployeesAsync(dto.CompanyId);
        var payrolls = new List<PayrollRecord>();

        foreach (var employee in employees)
        {
            var existing = await _unitOfWork.PayrollRecords.GetByEmployeeAndPeriodAsync(employee.Id, dto.Year, dto.Month);
            if (existing != null) continue;

            var payroll = CalculatePayroll(employee, new CalculatePayrollDto
            {
                EmployeeId = employee.Id,
                Year = dto.Year,
                Month = dto.Month,
                WorkingDays = dto.WorkingDays
            });
            payrolls.Add(payroll);
        }

        if (payrolls.Any())
        {
            await _unitOfWork.PayrollRecords.AddRangeAsync(payrolls);
        }

        return Ok(new PayrollSummaryDto
        {
            Year = dto.Year,
            Month = dto.Month,
            EmployeeCount = payrolls.Count,
            TotalGross = payrolls.Sum(p => p.GrossSalary),
            TotalNet = payrolls.Sum(p => p.NetSalary),
            TotalSgkWorker = payrolls.Sum(p => p.SgkWorkerDeduction),
            TotalSgkEmployer = payrolls.Sum(p => p.SgkEmployerCost),
            TotalIncomeTax = payrolls.Sum(p => p.IncomeTax),
            TotalCost = payrolls.Sum(p => p.TotalEmployerCost)
        });
    }

    [HttpGet("summary/{companyId}/{year}/{month}")]
    public async Task<ActionResult<PayrollSummaryDto>> GetSummary(int companyId, int year, int month)
    {
        var payrolls = await _unitOfWork.PayrollRecords.GetByPeriodAsync(companyId, year, month);
        var list = payrolls.ToList();

        return Ok(new PayrollSummaryDto
        {
            Year = year,
            Month = month,
            EmployeeCount = list.Count,
            TotalGross = list.Sum(p => p.GrossSalary),
            TotalNet = list.Sum(p => p.NetSalary),
            TotalSgkWorker = list.Sum(p => p.SgkWorkerDeduction),
            TotalSgkEmployer = list.Sum(p => p.SgkEmployerCost),
            TotalIncomeTax = list.Sum(p => p.IncomeTax),
            TotalCost = list.Sum(p => p.TotalEmployerCost)
        });
    }

    private PayrollRecord CalculatePayroll(Employee employee, CalculatePayrollDto dto)
    {
        var grossSalary = employee.GrossSalary;
        var workingDays = dto.WorkingDays > 0 ? dto.WorkingDays : 30;

        // SGK Oranlari (2024)
        const decimal sgkWorkerRate = 0.14m;
        const decimal unemploymentWorkerRate = 0.01m;
        const decimal sgkEmployerRate = 0.205m;
        const decimal unemploymentEmployerRate = 0.02m;
        const decimal stampTaxRate = 0.00759m;

        // SGK tavani
        const decimal sgkCeiling = 150000m;
        var sgkBase = Math.Min(grossSalary, sgkCeiling);

        // Isci kesintileri
        var sgkWorker = sgkBase * sgkWorkerRate;
        var unemploymentWorker = sgkBase * unemploymentWorkerRate;

        // Gelir vergisi matrahi
        var incomeTaxBase = grossSalary - sgkWorker - unemploymentWorker;

        // Gelir vergisi (kumulatif matrah hesabi yapilmali - simdilik basit)
        var incomeTax = CalculateIncomeTax(incomeTaxBase);

        // Damga vergisi
        var stampTax = grossSalary * stampTaxRate;

        // Asgari ucret istisnasi (2024 - basitlestirilmis)
        decimal minimumWageExemption = 0;
        if (employee.IsMinimumWageExempt)
        {
            minimumWageExemption = Math.Min(incomeTax, 2500); // Ornek deger
        }

        // Toplam kesintiler
        var totalDeductions = sgkWorker + unemploymentWorker + incomeTax + stampTax - minimumWageExemption;

        // Net maas
        var netSalary = grossSalary - totalDeductions;

        // Isveren SGK maliyeti
        var sgkEmployer = sgkBase * sgkEmployerRate;
        var unemploymentEmployer = sgkBase * unemploymentEmployerRate;
        var totalEmployerCost = grossSalary + sgkEmployer + unemploymentEmployer;

        return new PayrollRecord
        {
            EmployeeId = employee.Id,
            Year = dto.Year,
            Month = dto.Month,
            PaymentDate = new DateTime(dto.Year, dto.Month, 1).AddMonths(1).AddDays(-1),
            WorkingDays = workingDays,
            GrossSalary = grossSalary,
            TotalGross = grossSalary,
            SgkWorkerDeduction = sgkWorker,
            SgkUnemploymentWorker = unemploymentWorker,
            SgkEmployerCost = sgkEmployer,
            SgkUnemploymentEmployer = unemploymentEmployer,
            IncomeTaxBase = incomeTaxBase,
            IncomeTax = incomeTax,
            StampTax = stampTax,
            MinimumWageExemption = minimumWageExemption,
            TotalDeductions = totalDeductions,
            NetSalary = netSalary,
            TotalEmployerCost = totalEmployerCost,
            Status = PayrollStatus.Calculated
        };
    }

    private decimal CalculateIncomeTax(decimal taxBase)
    {
        // 2024 Gelir vergisi dilimleri (basitlestirilmis)
        if (taxBase <= 110000) return taxBase * 0.15m;
        if (taxBase <= 230000) return 16500 + (taxBase - 110000) * 0.20m;
        if (taxBase <= 870000) return 40500 + (taxBase - 230000) * 0.27m;
        if (taxBase <= 3000000) return 213300 + (taxBase - 870000) * 0.35m;
        return 958800 + (taxBase - 3000000) * 0.40m;
    }

    private static PayrollRecordDto MapToDto(PayrollRecord p) => new()
    {
        Id = p.Id,
        EmployeeId = p.EmployeeId,
        EmployeeName = p.Employee?.FullName ?? "",
        Year = p.Year,
        Month = p.Month,
        GrossSalary = p.GrossSalary,
        SgkWorkerDeduction = p.SgkWorkerDeduction,
        SgkUnemploymentWorker = p.SgkUnemploymentWorker,
        IncomeTax = p.IncomeTax,
        StampTax = p.StampTax,
        TotalDeductions = p.TotalDeductions,
        NetSalary = p.NetSalary,
        SgkEmployerCost = p.SgkEmployerCost,
        TotalEmployerCost = p.TotalEmployerCost,
        Status = p.Status.ToString()
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