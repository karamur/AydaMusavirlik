using FluentValidation;
using MediatR;
using AydaMusavirlik.Application.Common.Interfaces;
using AydaMusavirlik.Application.Common.Models;
using AydaMusavirlik.Core.Models.Payroll;

namespace AydaMusavirlik.Application.Features.Payroll.Commands.CalculatePayroll;

/// <summary>
/// Bordro hesapla command
/// </summary>
public record CalculatePayrollCommand : IRequest<Result<PayrollCalculationResultDto>>
{
    public int CompanyId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public int WorkingDays { get; init; } = 30;
}

public class CalculatePayrollCommandValidator : AbstractValidator<CalculatePayrollCommand>
{
    public CalculatePayrollCommandValidator()
    {
        RuleFor(v => v.CompanyId)
            .GreaterThan(0).WithMessage("Gecerli bir firma secilmelidir.");

        RuleFor(v => v.Year)
            .InclusiveBetween(2020, 2100).WithMessage("Gecerli bir yil giriniz.");

        RuleFor(v => v.Month)
            .InclusiveBetween(1, 12).WithMessage("Gecerli bir ay giriniz.");

        RuleFor(v => v.WorkingDays)
            .InclusiveBetween(1, 31).WithMessage("Calisma gunu 1-31 arasinda olmalidir.");
    }
}

public class CalculatePayrollCommandHandler : IRequestHandler<CalculatePayrollCommand, Result<PayrollCalculationResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    // 2025 parametreleri
    private const decimal SGK_WORKER_RATE = 0.14m;
    private const decimal SGK_UNEMPLOYMENT_WORKER = 0.01m;
    private const decimal SGK_EMPLOYER_RATE = 0.205m;
    private const decimal SGK_UNEMPLOYMENT_EMPLOYER = 0.02m;
    private const decimal STAMP_TAX_RATE = 0.00759m;

    private static readonly (decimal Limit, decimal Rate)[] IncomeTaxBrackets = new[]
    {
        (158000m, 0.15m),
        (330000m, 0.20m),
        (800000m, 0.27m),
        (2000000m, 0.35m),
        (decimal.MaxValue, 0.40m)
    };

    public CalculatePayrollCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PayrollCalculationResultDto>> Handle(CalculatePayrollCommand request, CancellationToken cancellationToken)
    {
        var employees = await _unitOfWork.Employees.GetByCompanyAsync(request.CompanyId, true, cancellationToken);

        if (!employees.Any())
            return Result<PayrollCalculationResultDto>.Failure("Bu firmada aktif calisan bulunamadi.");

        var results = new List<PayrollItemResultDto>();

        foreach (var employee in employees)
        {
            // Mevcut kaydi kontrol et
            var existingPayroll = await _unitOfWork.Payrolls.GetByEmployeePeriodAsync(
                employee.Id, request.Year, request.Month, cancellationToken);

            PayrollRecord payroll;

            if (existingPayroll != null)
            {
                payroll = existingPayroll;
            }
            else
            {
                payroll = new PayrollRecord
                {
                    EmployeeId = employee.Id,
                    Year = request.Year,
                    Month = request.Month,
                    WorkingDays = request.WorkingDays,
                    CreatedAt = DateTime.UtcNow
                };
            }

            // Hesaplama
            CalculatePayrollRecord(payroll, employee.GrossSalary, request.WorkingDays);

            if (existingPayroll == null)
            {
                await _unitOfWork.Payrolls.AddAsync(payroll, cancellationToken);
            }
            else
            {
                payroll.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Payrolls.Update(payroll);
            }

            results.Add(new PayrollItemResultDto
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                GrossSalary = payroll.GrossSalary,
                SgkWorkerDeduction = payroll.SgkWorkerDeduction,
                IncomeTax = payroll.IncomeTax,
                StampTax = payroll.StampTax,
                NetSalary = payroll.NetSalary,
                SgkEmployerCost = payroll.SgkEmployerCost,
                TotalCost = payroll.GrossSalary + payroll.SgkEmployerCost
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new PayrollCalculationResultDto
        {
            Year = request.Year,
            Month = request.Month,
            EmployeeCount = results.Count,
            TotalGross = results.Sum(r => r.GrossSalary),
            TotalNet = results.Sum(r => r.NetSalary),
            TotalSgkWorker = results.Sum(r => r.SgkWorkerDeduction),
            TotalSgkEmployer = results.Sum(r => r.SgkEmployerCost),
            TotalIncomeTax = results.Sum(r => r.IncomeTax),
            TotalCost = results.Sum(r => r.TotalCost),
            Items = results
        };

        return Result<PayrollCalculationResultDto>.Success(result);
    }

    private void CalculatePayrollRecord(PayrollRecord record, decimal grossSalary, int workingDays)
    {
        var effectiveGross = grossSalary * workingDays / 30m;
        record.GrossSalary = Math.Round(effectiveGross, 2);

        record.SgkWorkerDeduction = Math.Round(effectiveGross * SGK_WORKER_RATE, 2);
        var sgkUnemploymentWorker = Math.Round(effectiveGross * SGK_UNEMPLOYMENT_WORKER, 2);

        var sgkMatrah = effectiveGross - record.SgkWorkerDeduction - sgkUnemploymentWorker;
        record.IncomeTax = CalculateIncomeTax(sgkMatrah);
        record.StampTax = Math.Round(effectiveGross * STAMP_TAX_RATE, 2);

        record.NetSalary = Math.Round(effectiveGross - record.SgkWorkerDeduction - sgkUnemploymentWorker - 
                                       record.IncomeTax - record.StampTax, 2);

        record.SgkEmployerCost = Math.Round(effectiveGross * SGK_EMPLOYER_RATE, 2) +
                                  Math.Round(effectiveGross * SGK_UNEMPLOYMENT_EMPLOYER, 2);
    }

    private decimal CalculateIncomeTax(decimal monthlyMatrah)
    {
        var annualMatrah = monthlyMatrah * 12;
        decimal totalTax = 0;
        decimal remaining = annualMatrah;
        decimal previousLimit = 0;

        foreach (var (limit, rate) in IncomeTaxBrackets)
        {
            if (remaining <= 0) break;
            var taxableInBracket = Math.Min(remaining, limit - previousLimit);
            totalTax += taxableInBracket * rate;
            remaining -= taxableInBracket;
            previousLimit = limit;
        }

        return Math.Round(totalTax / 12, 2);
    }
}

public class PayrollCalculationResultDto
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
    public List<PayrollItemResultDto> Items { get; set; } = new();
}

public class PayrollItemResultDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public decimal SgkWorkerDeduction { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal StampTax { get; set; }
    public decimal NetSalary { get; set; }
    public decimal SgkEmployerCost { get; set; }
    public decimal TotalCost { get; set; }
}