using System.Windows;
using System.Windows.Controls;
using AydaMusavirlik.Core.Models.Common;
using AydaMusavirlik.Desktop.Services;

namespace AydaMusavirlik.Desktop.Views.Companies;

public partial class CompanyListView : UserControl
{
    public event EventHandler<Company>? CompanySelected;
    private readonly ICompanyService _companyService;
    private List<CompanyDto> _companies = new();

    public CompanyListView()
    {
        InitializeComponent();
        _companyService = App.GetService<ICompanyService>();
        Loaded += CompanyListView_Loaded;
    }

    private async void CompanyListView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var companies = await _companyService.GetAllAsync();
            _companies = companies.ToList();
            dgCompanies.ItemsSource = _companies;
            txtRecordCount.Text = $"Toplam: {_companies.Count} kayýt";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Veriler yüklenirken hata oluþtu: {ex.Message}", "Hata", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void YeniFirma_Click(object sender, RoutedEventArgs e)
    {
        var editWindow = new CompanyEditWindow();
        if (editWindow.ShowDialog() == true)
        {
            _ = LoadDataAsync();
        }
    }

    private async void Yenile_Click(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private void Duzenle_Click(object sender, RoutedEventArgs e)
    {
        var company = (sender as Button)?.DataContext as CompanyDto;
        if (company != null)
        {
            var editWindow = new CompanyEditWindow(company);
            if (editWindow.ShowDialog() == true)
            {
                _ = LoadDataAsync();
            }
        }
    }

    private void Sec_Click(object sender, RoutedEventArgs e)
    {
        var companyDto = (sender as Button)?.DataContext as CompanyDto;
        if (companyDto != null)
        {
            var company = new Company
            {
                Id = companyDto.Id,
                Name = companyDto.Name,
                TaxNumber = companyDto.TaxNumber,
                TaxOffice = companyDto.TaxOffice,
                City = companyDto.City
            };
            CompanySelected?.Invoke(this, company);
        }
    }

    private async void Sil_Click(object sender, RoutedEventArgs e)
    {
        var company = (sender as Button)?.DataContext as CompanyDto;
        if (company != null)
        {
            if (MessageBox.Show($"'{company.Name}' firmasýný silmek istediðinize emin misiniz?", 
                "Silme Onayý", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var result = await _companyService.DeleteAsync(company.Id);
                if (result)
                {
                    await LoadDataAsync();
                    MessageBox.Show("Firma baþarýyla silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Firma silinirken hata oluþtu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}