using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Core.Models.Accounting;

namespace AydaMusavirlik.Desktop.Views.Accounting;

public partial class FisListesiView : UserControl
{
    private readonly ICompanyService _companyService;
    private ObservableCollection<FisListeItem> _fisler = new();

    public FisListesiView()
    {
        InitializeComponent();
        _companyService = App.GetService<ICompanyService>();
        Loaded += FisListesiView_Loaded;
    }

    private async void FisListesiView_Loaded(object sender, RoutedEventArgs e)
    {
        // Tarih araligi
        dpBaslangic.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        dpBitis.SelectedDate = DateTime.Now;

        // Fis turleri
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Tumunu Goster", Tag = 0 });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Mahsup Fisi", Tag = 1 });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Tahsilat Fisi", Tag = 2 });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Odeme Fisi", Tag = 3 });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Acilis Fisi", Tag = 4 });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Satis Faturasi", Tag = 6 });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Alis Faturasi", Tag = 7 });
        cmbFisTuru.SelectedIndex = 0;

        // Durumlar
        cmbDurum.Items.Add(new ComboBoxItem { Content = "Tumunu Goster", Tag = 0 });
        cmbDurum.Items.Add(new ComboBoxItem { Content = "Taslak", Tag = 1 });
        cmbDurum.Items.Add(new ComboBoxItem { Content = "Onaylandi", Tag = 2 });
        cmbDurum.Items.Add(new ComboBoxItem { Content = "Deftere Islendi", Tag = 3 });
        cmbDurum.Items.Add(new ComboBoxItem { Content = "Iptal", Tag = 4 });
        cmbDurum.SelectedIndex = 0;

        // Firma listesi
        try
        {
            var firmalar = await _companyService.GetAllAsync();
            cmbFirma.ItemsSource = firmalar;
            if (firmalar.Any())
            {
                cmbFirma.SelectedIndex = 0;
                await LoadFislerAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Veri yuklenemedi: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadFislerAsync()
    {
        if (cmbFirma.SelectedItem == null) return;

        try
        {
            var firma = (CompanyDto)cmbFirma.SelectedItem;

            // Ornek veriler (gercek uygulamada servisden cekilecek)
            _fisler.Clear();

            var ornekFisler = new List<FisListeItem>
            {
                new FisListeItem
                {
                    Id = 1,
                    DocumentNumber = $"{DateTime.Now:yyyyMM}-0001",
                    DocumentDate = DateTime.Now.AddDays(-5),
                    RecordType = RecordType.MahsupFisi,
                    Description = "Personel maas tahakkuku",
                    TotalDebit = 125000,
                    TotalCredit = 125000,
                    Status = RecordStatus.Approved
                },
                new FisListeItem
                {
                    Id = 2,
                    DocumentNumber = $"{DateTime.Now:yyyyMM}-0002",
                    DocumentDate = DateTime.Now.AddDays(-3),
                    RecordType = RecordType.TahsilatFisi,
                    Description = "Musteri tahsilati - ABC Ltd.",
                    TotalDebit = 45000,
                    TotalCredit = 45000,
                    Status = RecordStatus.Posted
                },
                new FisListeItem
                {
                    Id = 3,
                    DocumentNumber = $"{DateTime.Now:yyyyMM}-0003",
                    DocumentDate = DateTime.Now.AddDays(-2),
                    RecordType = RecordType.OdemeFisi,
                    Description = "Tedarikci odemesi - XYZ A.S.",
                    TotalDebit = 78500,
                    TotalCredit = 78500,
                    Status = RecordStatus.Approved
                },
                new FisListeItem
                {
                    Id = 4,
                    DocumentNumber = $"{DateTime.Now:yyyyMM}-0004",
                    DocumentDate = DateTime.Now.AddDays(-1),
                    RecordType = RecordType.SatisFaturasi,
                    Description = "Satis faturasi - DEF Holding",
                    TotalDebit = 156000,
                    TotalCredit = 156000,
                    Status = RecordStatus.Draft
                },
                new FisListeItem
                {
                    Id = 5,
                    DocumentNumber = $"{DateTime.Now:yyyyMM}-0005",
                    DocumentDate = DateTime.Now,
                    RecordType = RecordType.AlisFaturasi,
                    Description = "Alis faturasi - GHI Ticaret",
                    TotalDebit = 32000,
                    TotalCredit = 32000,
                    Status = RecordStatus.Draft
                }
            };

            foreach (var fis in ornekFisler)
                _fisler.Add(fis);

            dgFisler.ItemsSource = _fisler;
            UpdateSummary();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fisler yuklenemedi: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateSummary()
    {
        var toplamBorc = _fisler.Sum(f => f.TotalDebit);
        var toplamAlacak = _fisler.Sum(f => f.TotalCredit);
        var fark = toplamBorc - toplamAlacak;

        txtKayitSayisi.Text = $"Toplam: {_fisler.Count} kayit";
        txtToplamBorc.Text = $"Toplam Borc: {toplamBorc:N2} TL";
        txtToplamAlacak.Text = $"Toplam Alacak: {toplamAlacak:N2} TL";
        txtFark.Text = $"Fark: {fark:N2} TL";
    }

    private async void BtnFiltrele_Click(object sender, RoutedEventArgs e)
    {
        await LoadFislerAsync();
    }

    private void BtnYeniFis_Click(object sender, RoutedEventArgs e)
    {
        if (cmbFirma.SelectedItem == null)
        {
            MessageBox.Show("Lutfen once firma secin.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var firma = (CompanyDto)cmbFirma.SelectedItem;
        var dialog = new FisDetayDialog(firma.Id, null);
        if (dialog.ShowDialog() == true)
        {
            _ = LoadFislerAsync();
        }
    }

    private void DgFisler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (dgFisler.SelectedItem is FisListeItem fis)
        {
            var firma = (CompanyDto)cmbFirma.SelectedItem;
            var dialog = new FisDetayDialog(firma.Id, fis.Id);
            if (dialog.ShowDialog() == true)
            {
                _ = LoadFislerAsync();
            }
        }
    }

    private void BtnDuzenle_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is FisListeItem fis)
        {
            var firma = (CompanyDto)cmbFirma.SelectedItem;
            var dialog = new FisDetayDialog(firma.Id, fis.Id);
            if (dialog.ShowDialog() == true)
            {
                _ = LoadFislerAsync();
            }
        }
    }

    private void BtnSil_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is FisListeItem fis)
        {
            if (MessageBox.Show($"'{fis.DocumentNumber}' numarali fisi silmek istediginizden emin misiniz?", 
                "Silme Onay", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _fisler.Remove(fis);
                UpdateSummary();
                MessageBox.Show("Fis silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private void BtnExcel_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Excel export ozelligi yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnYazdir_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Yazdir ozelligi yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

public class FisListeItem
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime DocumentDate { get; set; }
    public RecordType RecordType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public RecordStatus Status { get; set; }

    public string RecordTypeText => RecordType switch
    {
        RecordType.MahsupFisi => "Mahsup Fisi",
        RecordType.TahsilatFisi => "Tahsilat Fisi",
        RecordType.OdemeFisi => "Odeme Fisi",
        RecordType.AcilisFisi => "Acilis Fisi",
        RecordType.KapanisFisi => "Kapanis Fisi",
        RecordType.SatisFaturasi => "Satis Faturasi",
        RecordType.AlisFaturasi => "Alis Faturasi",
        RecordType.DekontFisi => "Dekont Fisi",
        _ => "Diger"
    };

    public string StatusText => Status switch
    {
        RecordStatus.Draft => "Taslak",
        RecordStatus.Approved => "Onaylandi",
        RecordStatus.Posted => "Islendi",
        RecordStatus.Cancelled => "Iptal",
        _ => "Bilinmiyor"
    };
}