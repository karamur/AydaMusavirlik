using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AydaMusavirlik.Core.Models.Payroll;
using AydaMusavirlik.Data;
using AydaMusavirlik.Desktop.Services;
using Microsoft.EntityFrameworkCore;

namespace AydaMusavirlik.Desktop.ViewModels;

public partial class PayrollViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    [ObservableProperty]
    private ObservableCollection<PayrollRecord> _payrollRecords = new();

    [ObservableProperty]
    private Employee? _selectedEmployee;

    [ObservableProperty]
    private PayrollRecord? _selectedPayroll;

    [ObservableProperty]
    private PayrollParameter? _currentParameter;

    [ObservableProperty]
    private string _title = "Bordro Yonetimi";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _selectedYear = DateTime.Now.Year;

    [ObservableProperty]
    private int _selectedMonth = DateTime.Now.Month;

    // Hesaplama sonuclari
    [ObservableProperty]
    private decimal _totalGross;

    [ObservableProperty]
    private decimal _totalNet;

    [ObservableProperty]
    private decimal _totalEmployerCost;

    public PayrollViewModel(AppDbContext context, IDialogService dialogService)
    {
        _context = context;
        _dialogService = dialogService;
        LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            await LoadEmployeesAsync();
            await LoadPayrollParameterAsync();
            await LoadPayrollRecordsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadEmployeesAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Company)
            .Where(e => e.TerminationDate == null)
            .OrderBy(e => e.Company.Name)
            .ThenBy(e => e.LastName)
            .ToListAsync();

        Employees = new ObservableCollection<Employee>(employees);
    }

    private async Task LoadPayrollParameterAsync()
    {
        CurrentParameter = await _context.PayrollParameters
            .Where(p => p.Year == SelectedYear && p.IsActive)
            .FirstOrDefaultAsync();
    }

    private async Task LoadPayrollRecordsAsync()
    {
        var records = await _context.PayrollRecords
            .Include(p => p.Employee)
            .Where(p => p.Year == SelectedYear && p.Month == SelectedMonth)
            .OrderBy(p => p.Employee.LastName)
            .ToListAsync();

        PayrollRecords = new ObservableCollection<PayrollRecord>(records);
        
        TotalGross = records.Sum(r => r.TotalGross);
        TotalNet = records.Sum(r => r.NetSalary);
        TotalEmployerCost = records.Sum(r => r.TotalEmployerCost);
    }

    [RelayCommand]
    private async Task CalculatePayrollAsync()
    {
        if (CurrentParameter == null)
        {
            _dialogService.ShowError($"{SelectedYear} yili icin bordro parametreleri tanimlanmamis!");
            return;
        }

        if (!_dialogService.Confirm($"{SelectedYear}/{SelectedMonth:D2} donemi icin tum calisanlarin bordrosu hesaplanacak. Devam edilsin mi?"))
            return;

        IsLoading = true;
        try
        {
            var employees = await _context.Employees
                .Where(e => e.TerminationDate == null)
                .ToListAsync();

            foreach (var employee in employees)
            {
                var existingRecord = await _context.PayrollRecords
                    .FirstOrDefaultAsync(p => p.EmployeeId == employee.Id && 
                                             p.Year == SelectedYear && 
                                             p.Month == SelectedMonth);

                if (existingRecord != null && existingRecord.Status != PayrollStatus.Draft)
                    continue;

                var record = existingRecord ?? new PayrollRecord
                {
                    EmployeeId = employee.Id,
                    Year = SelectedYear,
                    Month = SelectedMonth
                };

                CalculatePayroll(record, employee, CurrentParameter);

                if (existingRecord == null)
                    await _context.PayrollRecords.AddAsync(record);
            }

            await _context.SaveChangesAsync();
            await LoadPayrollRecordsAsync();
            
            _dialogService.ShowInfo($"{employees.Count} calisanin bordrosu hesaplandi.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Bordro hesaplanirken hata: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CalculatePayroll(PayrollRecord record, Employee employee, PayrollParameter param)
    {
        record.WorkingDays = 30;
        record.GrossSalary = employee.GrossSalary;
        record.TotalGross = record.GrossSalary + record.OvertimePay + record.BonusPay + record.OtherEarnings;

        // SGK isci payi (%14)
        var sgkBase = Math.Min(record.TotalGross, param.SgkCeiling);
        record.SgkWorkerDeduction = sgkBase * param.SgkEmployeeRate / 100;

        // Issizlik isci payi (%1)
        record.SgkUnemploymentWorker = sgkBase * param.UnemploymentEmployeeRate / 100;

        // Gelir vergisi matrahi
        record.IncomeTaxBase = record.TotalGross - record.SgkWorkerDeduction - record.SgkUnemploymentWorker;

        // Asgari ucret istisnasi
        if (employee.IsMinimumWageExempt || record.TotalGross <= param.MinimumWage)
        {
            var minWageTaxBase = param.MinimumWage - (param.MinimumWage * (param.SgkEmployeeRate + param.UnemploymentEmployeeRate) / 100);
            record.MinimumWageExemption = minWageTaxBase * 0.15m; // %15 oran
        }

        // Gelir vergisi (basit %15 hesaplama - gercekte kumulatif matrah gerekir)
        record.IncomeTax = Math.Max(0, record.IncomeTaxBase * 0.15m - record.MinimumWageExemption);

        // Damga vergisi
        record.StampTax = record.TotalGross * param.StampTaxRate / 100;

        // Toplam kesinti
        record.TotalDeductions = record.SgkWorkerDeduction + record.SgkUnemploymentWorker + 
                                record.IncomeTax + record.StampTax;

        // Net ucret
        record.NetSalary = record.TotalGross - record.TotalDeductions;

        // Isveren paylari
        record.SgkEmployerCost = sgkBase * param.SgkEmployerRate / 100;
        record.SgkUnemploymentEmployer = sgkBase * param.UnemploymentEmployerRate / 100;

        // Toplam isveren maliyeti
        record.TotalEmployerCost = record.TotalGross + record.SgkEmployerCost + record.SgkUnemploymentEmployer;

        record.Status = PayrollStatus.Calculated;
        record.PaymentDate = new DateTime(SelectedYear, SelectedMonth, 1).AddMonths(1).AddDays(-1);
    }

    [RelayCommand]
    private async Task ApprovePayrollAsync()
    {
        var draftRecords = PayrollRecords.Where(p => p.Status == PayrollStatus.Calculated).ToList();
        
        if (!draftRecords.Any())
        {
            _dialogService.ShowWarning("Onaylanacak bordro kaydi bulunamadi.");
            return;
        }

        if (!_dialogService.Confirm($"{draftRecords.Count} bordro kaydi onaylanacak. Devam edilsin mi?"))
            return;

        foreach (var record in draftRecords)
        {
            record.Status = PayrollStatus.Approved;
        }

        await _context.SaveChangesAsync();
        _dialogService.ShowInfo($"{draftRecords.Count} bordro onaylandi.");
    }

    partial void OnSelectedYearChanged(int value)
    {
        _ = LoadPayrollParameterAsync();
        _ = LoadPayrollRecordsAsync();
    }

    partial void OnSelectedMonthChanged(int value)
    {
        _ = LoadPayrollRecordsAsync();
    }
}
