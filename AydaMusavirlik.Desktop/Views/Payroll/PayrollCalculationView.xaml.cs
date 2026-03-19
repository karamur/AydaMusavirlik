using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AydaMusavirlik.Desktop.Services;

namespace AydaMusavirlik.Desktop.Views.Payroll;

public partial class PayrollCalculationView : UserControl
{
    private readonly IPayrollService _payrollService;
    private ObservableCollection<PayrollRecordDto> _payrolls = new();
    private int _currentCompanyId = 1; // TODO: Aktif firma ID'si

    public PayrollCalculationView()
    {
        InitializeComponent();
        _payrollService = App.GetService<IPayrollService>();
        Loaded += PayrollCalculationView_Loaded;
    }

    private async void PayrollCalculationView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            
            var payrolls = await _payrollService.GetByPeriodAsync(_currentCompanyId, year, month);
            _payrolls = new ObservableCollection<PayrollRecordDto>(payrolls);
            dgPayrolls.ItemsSource = _payrolls;
            UpdateTotals();
        }
        catch
        {
            // API'den veri gelmezse örnek veri göster
            LoadSampleData();
        }
    }

    private void LoadSampleData()
    {
        _payrolls = new ObservableCollection<PayrollRecordDto>
        {
            new PayrollRecordDto { EmployeeName = "Ahmet Yýlmaz", GrossSalary = 45000, SgkWorkerDeduction = 6300, IncomeTax = 5805, StampTax = 341.55m, TotalDeductions = 12896.55m, NetSalary = 32103.45m, SgkEmployerCost = 9225, TotalEmployerCost = 54225 },
            new PayrollRecordDto { EmployeeName = "Ayþe Kaya", GrossSalary = 32000, SgkWorkerDeduction = 4480, IncomeTax = 4128, StampTax = 242.88m, TotalDeductions = 9170.88m, NetSalary = 22829.12m, SgkEmployerCost = 6560, TotalEmployerCost = 38560 },
            new PayrollRecordDto { EmployeeName = "Mehmet Demir", GrossSalary = 28000, SgkWorkerDeduction = 3920, IncomeTax = 3612, StampTax = 212.52m, TotalDeductions = 8024.52m, NetSalary = 19975.48m, SgkEmployerCost = 5740, TotalEmployerCost = 33740 },
        };
        dgPayrolls.ItemsSource = _payrolls;
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        var totalGross = _payrolls.Sum(p => p.GrossSalary);
        var totalNet = _payrolls.Sum(p => p.NetSalary);
        var totalCost = _payrolls.Sum(p => p.TotalEmployerCost);

        txtTotalGross.Text = $"{totalGross:N2} TL";
        txtTotalNet.Text = $"{totalNet:N2} TL";
        txtTotalCost.Text = $"{totalCost:N2} TL";
    }

    private async void TumunuHesapla_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dto = new CalculateAllPayrollDto
            {
                CompanyId = _currentCompanyId,
                Year = DateTime.Now.Year,
                Month = DateTime.Now.Month,
                WorkingDays = 30
            };

            var result = await _payrollService.CalculateAllAsync(dto);
            if (result != null)
            {
                await LoadDataAsync();
                MessageBox.Show($"{result.EmployeeCount} personelin bordrosu hesaplandý.\n" +
                    $"Toplam Brüt: {result.TotalGross:N2} TL\n" +
                    $"Toplam Net: {result.TotalNet:N2} TL\n" +
                    $"Toplam Maliyet: {result.TotalCost:N2} TL", 
                    "Bordro Hesaplama", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SgkBildirge_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("SGK Aylýk Prim ve Hizmet Belgesi oluþturulacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExcelAktar_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Bordro listesi Excel'e aktarýlacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BordroYazdir_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Bordro yazdýrýlacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

// PayrollItem artýk gerekli deðil - PayrollRecordDto kullanýlýyor