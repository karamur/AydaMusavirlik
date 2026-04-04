using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AydaMusavirlik.Core.Models.Common;
using AydaMusavirlik.Data;
using AydaMusavirlik.Desktop.Services;
using Microsoft.EntityFrameworkCore;

namespace AydaMusavirlik.Desktop.ViewModels;

public partial class CompaniesViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Company> _companies = new();

    [ObservableProperty]
    private Company? _selectedCompany;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _title = "Firmalar";

    public CompaniesViewModel(AppDbContext context, IDialogService dialogService)
    {
        _context = context;
        _dialogService = dialogService;
        LoadCompaniesAsync();
    }

    [RelayCommand]
    private async Task LoadCompaniesAsync()
    {
        IsLoading = true;
        try
        {
            var companies = await _context.Companies
                .Include(c => c.Contacts)
                .OrderBy(c => c.Name)
                .ToListAsync();

            Companies = new ObservableCollection<Company>(companies);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Firmalar yuklenirken hata olustu: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadCompaniesAsync();
            return;
        }

        IsLoading = true;
        try
        {
            var searchLower = SearchText.ToLower();
            var companies = await _context.Companies
                .Where(c => c.Name.ToLower().Contains(searchLower) ||
                           (c.TaxNumber != null && c.TaxNumber.Contains(searchLower)) ||
                           (c.City != null && c.City.ToLower().Contains(searchLower)))
                .OrderBy(c => c.Name)
                .ToListAsync();

            Companies = new ObservableCollection<Company>(companies);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Arama sirasinda hata olustu: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddCompanyAsync()
    {
        var newCompany = new Company
        {
            Name = "Yeni Firma",
            CompanyType = CompanyType.LimitedSirketi,
            IsActive = true
        };

        try
        {
            await _context.Companies.AddAsync(newCompany);
            await _context.SaveChangesAsync();
            Companies.Add(newCompany);
            SelectedCompany = newCompany;
            _dialogService.ShowInfo("Yeni firma eklendi. Lutfen bilgileri duzenleyin.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Firma eklenirken hata olustu: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveCompanyAsync()
    {
        if (SelectedCompany == null) return;

        try
        {
            SelectedCompany.UpdatedAt = DateTime.UtcNow;
            _context.Companies.Update(SelectedCompany);
            await _context.SaveChangesAsync();
            _dialogService.ShowInfo("Firma basariyla kaydedildi.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Firma kaydedilirken hata olustu: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteCompanyAsync()
    {
        if (SelectedCompany == null) return;

        if (!_dialogService.Confirm($"'{SelectedCompany.Name}' firmasini silmek istediginize emin misiniz?"))
            return;

        try
        {
            // Soft delete
            SelectedCompany.IsDeleted = true;
            SelectedCompany.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            Companies.Remove(SelectedCompany);
            SelectedCompany = null;
            _dialogService.ShowInfo("Firma silindi.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Firma silinirken hata olustu: {ex.Message}");
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            _ = LoadCompaniesAsync();
        }
    }
}
