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
    private readonly ApiClient _apiClient;

    public PayrollService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<PayrollRecordDto>> GetByPeriodAsync(int companyId, int year, int month)
    {
        var response = await _apiClient.GetAsync<IEnumerable<PayrollRecordDto>>($"api/payroll/company/{companyId}/{year}/{month}");
        return response.Data ?? Enumerable.Empty<PayrollRecordDto>();
    }

    public async Task<IEnumerable<PayrollRecordDto>> GetByEmployeeAsync(int employeeId)
    {
        var response = await _apiClient.GetAsync<IEnumerable<PayrollRecordDto>>($"api/payroll/employee/{employeeId}");
        return response.Data ?? Enumerable.Empty<PayrollRecordDto>();
    }

    public async Task<PayrollRecordDto?> CalculateAsync(CalculatePayrollDto dto)
    {
        var response = await _apiClient.PostAsync<PayrollRecordDto>("api/payroll/calculate", dto);
        return response.Data;
    }

    public async Task<PayrollSummaryDto?> CalculateAllAsync(CalculateAllPayrollDto dto)
    {
        var response = await _apiClient.PostAsync<PayrollSummaryDto>("api/payroll/calculate-all", dto);
        return response.Data;
    }

    public async Task<PayrollSummaryDto?> GetSummaryAsync(int companyId, int year, int month)
    {
        var response = await _apiClient.GetAsync<PayrollSummaryDto>($"api/payroll/summary/{companyId}/{year}/{month}");
        return response.Data;
    }
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