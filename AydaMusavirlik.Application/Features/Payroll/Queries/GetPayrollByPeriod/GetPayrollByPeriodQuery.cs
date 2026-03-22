using MediatR;
using AydaMusavirlik.Application.Common.Interfaces;
using AydaMusavirlik.Application.Common.Models;

namespace AydaMusavirlik.Application.Features.Payroll.Queries.GetPayrollByPeriod;

/// <summary>
/// Donem bazli bordro getir query
/// </summary>
public record GetPayrollByPeriodQuery(int CompanyId, int Year, int Month) : IRequest<Result<PayrollPeriodDto>>;

public class GetPayrollByPeriodQueryHandler : IRequestHandler<GetPayrollByPeriodQuery, Result<PayrollPeriodDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPayrollByPeriodQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PayrollPeriodDto>> Handle(GetPayrollByPeriodQuery request, CancellationToken cancellationToken)
    {
        var payrolls = await _unitOfWork.Payrolls.GetByPeriodAsync(request.CompanyId, request.Year, request.Month, cancellationToken);

        var dto = new PayrollPeriodDto
        {
            Year = request.Year,
            Month = request.Month,
            EmployeeCount = payrolls.Count(),
            TotalGross = payrolls.Sum(p => p.GrossSalary),
            TotalNet = payrolls.Sum(p => p.NetSalary),
            TotalSgkWorker = payrolls.Sum(p => p.SgkWorkerDeduction),
            TotalSgkEmployer = payrolls.Sum(p => p.SgkEmployerCost),
            TotalIncomeTax = payrolls.Sum(p => p.IncomeTax),
            TotalStampTax = payrolls.Sum(p => p.StampTax),
            TotalCost = payrolls.Sum(p => p.GrossSalary + p.SgkEmployerCost),
            Payrolls = payrolls.Select(p => new PayrollItemDto
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                EmployeeName = p.Employee?.FirstName + " " + p.Employee?.LastName,
                GrossSalary = p.GrossSalary,
                SgkWorkerDeduction = p.SgkWorkerDeduction,
                IncomeTax = p.IncomeTax,
                StampTax = p.StampTax,
                NetSalary = p.NetSalary,
                SgkEmployerCost = p.SgkEmployerCost,
                TotalCost = p.GrossSalary + p.SgkEmployerCost
            }).ToList()
        };

        return Result<PayrollPeriodDto>.Success(dto);
    }
}

public class PayrollPeriodDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalSgkWorker { get; set; }
    public decimal TotalSgkEmployer { get; set; }
    public decimal TotalIncomeTax { get; set; }
    public decimal TotalStampTax { get; set; }
    public decimal TotalCost { get; set; }
    public List<PayrollItemDto> Payrolls { get; set; } = new();
}

public class PayrollItemDto
{
    public int Id { get; set; }
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