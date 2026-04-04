using CommunityToolkit.Mvvm.ComponentModel;
using AydaMusavirlik.Data;
using Microsoft.EntityFrameworkCore;

namespace AydaMusavirlik.Desktop.ViewModels;

/// <summary>
/// Dashboard ViewModel
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private string _title = "Dashboard";

    [ObservableProperty]
    private int _companyCount;

    [ObservableProperty]
    private int _employeeCount;

    [ObservableProperty]
    private bool _isLoading;

    public DashboardViewModel(AppDbContext context)
    {
        _context = context;
        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            CompanyCount = await _context.Companies.CountAsync();
            EmployeeCount = await _context.Employees.CountAsync();
        }
        catch
        {
            // Ignore errors in dashboard
        }
        finally
        {
            IsLoading = false;
        }
    }
}
