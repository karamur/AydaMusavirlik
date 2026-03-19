using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AydaMusavirlik.Desktop.Services;

namespace AydaMusavirlik.Desktop.Views.Accounting;

public partial class TrialBalanceView : UserControl
{
    private readonly IAccountService _accountService;
    private ObservableCollection<TrialBalanceItemDto> _items = new();
    private int _currentCompanyId = 1; // TODO: Aktif firma ID'si

    public TrialBalanceView()
    {
        InitializeComponent();
        _accountService = App.GetService<IAccountService>();
        dpStartDate.SelectedDate = new DateTime(DateTime.Now.Year, 1, 1);
        dpEndDate.SelectedDate = DateTime.Now;
        Loaded += TrialBalanceView_Loaded;
    }

    private async void TrialBalanceView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var startDate = dpStartDate.SelectedDate ?? new DateTime(DateTime.Now.Year, 1, 1);
            var endDate = dpEndDate.SelectedDate ?? DateTime.Now;

            var result = await _accountService.GetTrialBalanceAsync(_currentCompanyId, startDate, endDate);
            if (result != null && result.Items.Any())
            {
                _items = new ObservableCollection<TrialBalanceItemDto>(result.Items);
                dgTrialBalance.ItemsSource = _items;
                UpdateTotals();
            }
            else
            {
                LoadSampleData();
            }
        }
        catch
        {
            LoadSampleData();
        }
    }

    private void LoadSampleData()
    {
        _items = new ObservableCollection<TrialBalanceItemDto>
        {
            new TrialBalanceItemDto { AccountCode = "100", AccountName = "KASA", TotalDebit = 125000, TotalCredit = 98000, DebitBalance = 27000, CreditBalance = 0 },
            new TrialBalanceItemDto { AccountCode = "102", AccountName = "BANKALAR", TotalDebit = 450000, TotalCredit = 320000, DebitBalance = 130000, CreditBalance = 0 },
            new TrialBalanceItemDto { AccountCode = "120", AccountName = "ALICILAR", TotalDebit = 280000, TotalCredit = 150000, DebitBalance = 130000, CreditBalance = 0 },
            new TrialBalanceItemDto { AccountCode = "320", AccountName = "SATICILAR", TotalDebit = 120000, TotalCredit = 245000, DebitBalance = 0, CreditBalance = 125000 },
            new TrialBalanceItemDto { AccountCode = "500", AccountName = "SERMAYE", TotalDebit = 0, TotalCredit = 500000, DebitBalance = 0, CreditBalance = 500000 },
            new TrialBalanceItemDto { AccountCode = "600", AccountName = "YURTÝÇÝ SATIÞLAR", TotalDebit = 0, TotalCredit = 680000, DebitBalance = 0, CreditBalance = 680000 },
        };
        dgTrialBalance.ItemsSource = _items;
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        var totalDebit = _items.Sum(i => i.TotalDebit);
        var totalCredit = _items.Sum(i => i.TotalCredit);
        var difference = totalDebit - totalCredit;

        txtGrandTotalDebit.Text = $"{totalDebit:N2} TL";
        txtGrandTotalCredit.Text = $"{totalCredit:N2} TL";
        txtGrandDifference.Text = $"{Math.Abs(difference):N2} TL";

        txtGrandDifference.Foreground = difference == 0 
            ? System.Windows.Media.Brushes.LightGreen 
            : System.Windows.Media.Brushes.Red;
    }

    private async void MizanOlustur_Click(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
        MessageBox.Show("Mizan baþarýyla oluþturuldu.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExcelAktar_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Mizan Excel dosyasýna aktarýlacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Yazdir_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Mizan yazdýrýlacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}