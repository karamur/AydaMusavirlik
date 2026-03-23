using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AydaMusavirlik.Core.Models.Accounting;
using AydaMusavirlik.Desktop.Services;

namespace AydaMusavirlik.Desktop.Views.Accounting;

public partial class VoucherEntryView : UserControl
{
    private ObservableCollection<VoucherEntryItem> _entries = new();
    private readonly IAccountingRecordService _recordService;
    private readonly IAccountService _accountService;
    private List<AccountDto> _accounts = new();
    private int _currentCompanyId = 1; // TODO: Aktif firma

    public VoucherEntryView()
    {
        InitializeComponent();
        _recordService = App.GetService<IAccountingRecordService>();
        _accountService = App.GetService<IAccountService>();
        
        dpDate.SelectedDate = DateTime.Today;
        Loaded += VoucherEntryView_Loaded;
    }

    private async void VoucherEntryView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadAccountsAsync();
        await GenerateNewVoucherNumberAsync();
        InitializeData();
    }

    private async Task LoadAccountsAsync()
    {
        try
        {
            var accounts = await _accountService.GetByCompanyAsync(_currentCompanyId);
            _accounts = accounts.Where(a => a.AllowPosting).ToList();
        }
        catch { }
    }

    private async Task GenerateNewVoucherNumberAsync()
    {
        try
        {
            var type = GetSelectedRecordTypeString();
            var number = await _recordService.GenerateDocumentNumberAsync(_currentCompanyId, type);
            txtVoucherNo.Text = number;
        }
        catch { }
    }

    private string GetSelectedRecordTypeString()
    {
        var selectedIndex = cmbVoucherType.SelectedIndex;
        return selectedIndex switch
        {
            1 => "Tahsil",
            2 => "Tediye",
            _ => "Mahsup"
        };
    }

    private void InitializeData()
    {
        _entries = new ObservableCollection<VoucherEntryItem>
        {
            new VoucherEntryItem { RowNumber = 1 },
            new VoucherEntryItem { RowNumber = 2 },
        };
        
        foreach (var entry in _entries)
        {
            entry.PropertyChanged += Entry_PropertyChanged;
        }
        
        dgEntries.ItemsSource = _entries;
        UpdateTotals();
    }

    private void Entry_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VoucherEntryItem.AccountCode))
        {
            var item = sender as VoucherEntryItem;
            if (item != null)
            {
                var account = _accounts.FirstOrDefault(a => a.Code == item.AccountCode);
                if (account != null)
                {
                    item.AccountName = account.Name;
                    item.AccountId = account.Id;
                }
            }
        }
        else if (e.PropertyName == nameof(VoucherEntryItem.Debit) || 
                 e.PropertyName == nameof(VoucherEntryItem.Credit))
        {
            UpdateTotals();
        }
    }

    private void UpdateTotals()
    {
        var totalDebit = _entries.Sum(e => e.Debit);
        var totalCredit = _entries.Sum(e => e.Credit);
        var difference = totalDebit - totalCredit;

        txtTotalDebit.Text = $"{totalDebit:N2} TL";
        txtTotalCredit.Text = $"{totalCredit:N2} TL";
        txtDifference.Text = $"{Math.Abs(difference):N2} TL";

        txtDifference.Foreground = difference == 0 
            ? System.Windows.Media.Brushes.LightGreen 
            : System.Windows.Media.Brushes.Red;
    }

    private void SatirEkle_Click(object sender, RoutedEventArgs e)
    {
        var newEntry = new VoucherEntryItem { RowNumber = _entries.Count + 1 };
        newEntry.PropertyChanged += Entry_PropertyChanged;
        _entries.Add(newEntry);
    }

    private void SatirSil_Click(object sender, RoutedEventArgs e)
    {
        if (dgEntries.SelectedItem is VoucherEntryItem item)
        {
            item.PropertyChanged -= Entry_PropertyChanged;
            _entries.Remove(item);
            for (int i = 0; i < _entries.Count; i++)
                _entries[i].RowNumber = i + 1;
            UpdateTotals();
        }
    }

    private async void Kaydet_Click(object sender, RoutedEventArgs e)
    {
        UpdateTotals();
        
        var totalDebit = _entries.Sum(e => e.Debit);
        var totalCredit = _entries.Sum(e => e.Credit);

        if (totalDebit != totalCredit)
        {
            MessageBox.Show("Borc ve Alacak toplamlari esit olmalidir!", "Uyari", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Bos satirlari kontrol et
        var validEntries = _entries.Where(e => e.AccountId > 0 && (e.Debit > 0 || e.Credit > 0)).ToList();
        if (validEntries.Count < 2)
        {
            MessageBox.Show("En az 2 gecerli satir olmalidir!", "Uyari", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dto = new CreateAccountingRecordDto
            {
                CompanyId = _currentCompanyId,
                Date = dpDate.SelectedDate ?? DateTime.Today,
                RecordType = GetSelectedRecordTypeString(),
                Description = txtDescription.Text,
                Entries = validEntries.Select(e => new CreateAccountingEntryDto
                {
                    AccountId = e.AccountId,
                    Description = e.Description,
                    Debit = e.Debit,
                    Credit = e.Credit
                }).ToList()
            };

            var result = await _recordService.CreateAsync(dto);
            
            if (result != null)
            {
                MessageBox.Show($"Fis basariyla kaydedildi.\nFis No: {result.DocumentNumber}", 
                    "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Yeni fis icin hazirla
                await GenerateNewVoucherNumberAsync();
                InitializeData();
            }
            else
            {
                MessageBox.Show("Fis kaydedilemedi!", "Hata", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Onayla_Click(object sender, RoutedEventArgs e)
    {
        var totalDebit = _entries.Sum(e => e.Debit);
        var totalCredit = _entries.Sum(e => e.Credit);

        if (totalDebit != totalCredit)
        {
            MessageBox.Show("Borc ve Alacak toplamlari esit olmalidir!", "Uyari", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MessageBox.Show("Fis onaylandi ve hesap bakiyeleri guncellendi.", "Basarili", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

public class VoucherEntryItem : INotifyPropertyChanged
{
    private int _rowNumber;
    private int _accountId;
    private string _accountCode = string.Empty;
    private string _accountName = string.Empty;
    private string _description = string.Empty;
    private decimal _debit;
    private decimal _credit;

    public int RowNumber
    {
        get => _rowNumber;
        set { _rowNumber = value; OnPropertyChanged(nameof(RowNumber)); }
    }

    public int AccountId
    {
        get => _accountId;
        set { _accountId = value; OnPropertyChanged(nameof(AccountId)); }
    }

    public string AccountCode
    {
        get => _accountCode;
        set { _accountCode = value; OnPropertyChanged(nameof(AccountCode)); }
    }

    public string AccountName
    {
        get => _accountName;
        set { _accountName = value; OnPropertyChanged(nameof(AccountName)); }
    }

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(nameof(Description)); }
    }

    public decimal Debit
    {
        get => _debit;
        set { _debit = value; OnPropertyChanged(nameof(Debit)); }
    }

    public decimal Credit
    {
        get => _credit;
        set { _credit = value; OnPropertyChanged(nameof(Credit)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}