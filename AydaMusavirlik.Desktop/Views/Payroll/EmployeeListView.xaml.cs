using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AydaMusavirlik.Desktop.Services;

namespace AydaMusavirlik.Desktop.Views.Payroll;

public partial class EmployeeListView : UserControl
{
    private readonly IEmployeeService _employeeService;
    private ObservableCollection<Services.EmployeeDto> _employees = new();
    private int _currentCompanyId = 1; // TODO: Aktif firma ID'si

    public EmployeeListView()
    {
        InitializeComponent();
        _employeeService = App.GetService<IEmployeeService>();
        Loaded += EmployeeListView_Loaded;
    }

    private async void EmployeeListView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var employees = await _employeeService.GetByCompanyAsync(_currentCompanyId);
            _employees = new ObservableCollection<Services.EmployeeDto>(employees);
            dgEmployees.ItemsSource = _employees;
            UpdateStats();
        }
        catch
        {
            // API'den veri gelmezse örnek veri göster
            _employees = new ObservableCollection<Services.EmployeeDto>
            {
                new Services.EmployeeDto { Id = 1, EmployeeNumber = "001", FirstName = "Ahmet", LastName = "Yýlmaz", FullName = "Ahmet Yýlmaz", TcKimlikNo = "12345678901", Department = "Muhasebe", Position = "Müdür", HireDate = new DateTime(2020, 3, 15), GrossSalary = 45000, IsActive = true },
                new Services.EmployeeDto { Id = 2, EmployeeNumber = "002", FirstName = "Ayþe", LastName = "Kaya", FullName = "Ayþe Kaya", TcKimlikNo = "23456789012", Department = "ÝK", Position = "Uzman", HireDate = new DateTime(2021, 6, 1), GrossSalary = 32000, IsActive = true },
            };
            dgEmployees.ItemsSource = _employees;
            UpdateStats();
        }
    }

    private void UpdateStats()
    {
        var totalSalary = _employees.Sum(e => e.GrossSalary);
        var activeCount = _employees.Count(e => e.IsActive);
        txtRecordCount.Text = $"Toplam: {_employees.Count} personel";
        txtActiveCount.Text = $"Aktif: {activeCount}";
        txtTotalSalary.Text = $"{totalSalary:N2} TL";
    }

    private void YeniPersonel_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Yeni personel ekleme formu açýlacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void Yenile_Click(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private void Duzenle_Click(object sender, RoutedEventArgs e)
    {
        var employee = (sender as Button)?.DataContext as Services.EmployeeDto;
        if (employee != null)
        {
            MessageBox.Show($"{employee.FullName} personeli düzenlenecek.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void Sil_Click(object sender, RoutedEventArgs e)
    {
        var employee = (sender as Button)?.DataContext as Services.EmployeeDto;
        if (employee != null)
        {
            if (MessageBox.Show($"'{employee.FullName}' personelini silmek istediðinize emin misiniz?", 
                "Silme Onayý", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var result = await _employeeService.DeleteAsync(employee.Id);
                if (result)
                {
                    _employees.Remove(employee);
                    UpdateStats();
                }
            }
        }
    }
}