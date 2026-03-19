using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AydaMusavirlik.Desktop.Views.Accounting;

public partial class VoucherEntryView : UserControl
{
    private ObservableCollection<VoucherEntryItem> _entries;

    public VoucherEntryView()
    {
        InitializeComponent();
        dpDate.SelectedDate = DateTime.Today;
        InitializeData();
    }

    private void InitializeData()
    {
        _entries = new ObservableCollection<VoucherEntryItem>
        {
            new VoucherEntryItem { RowNumber = 1, AccountCode = "100", AccountName = "KASA", Description = "Nakit tahsilat", Debit = 5000, Credit = 0 },
            new VoucherEntryItem { RowNumber = 2, AccountCode = "120", AccountName = "ALICILAR", Description = "Musteri tahsilati", Debit = 0, Credit = 5000 },
        };
        dgEntries.ItemsSource = _entries;
        UpdateTotals();
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
        _entries.Add(new VoucherEntryItem { RowNumber = _entries.Count + 1 });
    }

    private void SatirSil_Click(object sender, RoutedEventArgs e)
    {
        if (dgEntries.SelectedItem is VoucherEntryItem item)
        {
            _entries.Remove(item);
            for (int i = 0; i < _entries.Count; i++)
                _entries[i].RowNumber = i + 1;
            UpdateTotals();
        }
    }

    private void Kaydet_Click(object sender, RoutedEventArgs e)
    {
        UpdateTotals();
        var totalDebit = _entries.Sum(e => e.Debit);
        var totalCredit = _entries.Sum(e => e.Credit);

        if (totalDebit != totalCredit)
        {
            MessageBox.Show("Borc ve Alacak toplamlari esit olmalidir!", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        MessageBox.Show("Fis basariyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Onayla_Click(object sender, RoutedEventArgs e)
    {
        var totalDebit = _entries.Sum(e => e.Debit);
        var totalCredit = _entries.Sum(e => e.Credit);

        if (totalDebit != totalCredit)
        {
            MessageBox.Show("Borc ve Alacak toplamlari esit olmalidir!", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (MessageBox.Show("Fis onaylandiktan sonra degistirilemez. Onaylamak istediginize emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            MessageBox.Show("Fis basariyla onaylandi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

public class VoucherEntryItem
{
    public int RowNumber { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}