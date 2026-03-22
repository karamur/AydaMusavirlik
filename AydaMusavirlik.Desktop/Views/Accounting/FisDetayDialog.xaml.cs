using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AydaMusavirlik.Core.Models.Accounting;

namespace AydaMusavirlik.Desktop.Views.Accounting;

public partial class FisDetayDialog : Window
{
    private readonly int _companyId;
    private readonly int? _fisId;
    private ObservableCollection<FisSatirItem> _satirlar = new();

    public FisDetayDialog(int companyId, int? fisId = null)
    {
        InitializeComponent();
        _companyId = companyId;
        _fisId = fisId;

        Loaded += FisDetayDialog_Loaded;
    }

    private void FisDetayDialog_Loaded(object sender, RoutedEventArgs e)
    {
        // Fis turleri
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Mahsup Fisi", Tag = RecordType.MahsupFisi });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Tahsilat Fisi", Tag = RecordType.TahsilatFisi });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Odeme Fisi", Tag = RecordType.OdemeFisi });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Acilis Fisi", Tag = RecordType.AcilisFisi });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Satis Faturasi", Tag = RecordType.SatisFaturasi });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Alis Faturasi", Tag = RecordType.AlisFaturasi });
        cmbFisTuru.Items.Add(new ComboBoxItem { Content = "Dekont Fisi", Tag = RecordType.DekontFisi });
        cmbFisTuru.SelectedIndex = 0;

        // Durumlar
        cmbDurum.Items.Add(new ComboBoxItem { Content = "Taslak", Tag = RecordStatus.Draft });
        cmbDurum.Items.Add(new ComboBoxItem { Content = "Onaylandi", Tag = RecordStatus.Approved });
        cmbDurum.Items.Add(new ComboBoxItem { Content = "Deftere Islendi", Tag = RecordStatus.Posted });
        cmbDurum.SelectedIndex = 0;

        dpTarih.SelectedDate = DateTime.Now;

        if (_fisId.HasValue)
        {
            // Duzenleme modu
            txtBaslik.Text = "Muhasebe Fisi Duzenleme";
            LoadFisDetay();
        }
        else
        {
            // Yeni fis
            txtBaslik.Text = "Yeni Muhasebe Fisi";
            txtFisNo.Text = GenerateNewFisNo();

            // Bos satirlar ekle
            for (int i = 1; i <= 3; i++)
            {
                _satirlar.Add(new FisSatirItem { Sira = i });
            }
        }

        dgSatirlar.ItemsSource = _satirlar;
        dgSatirlar.CellEditEnding += DgSatirlar_CellEditEnding;
    }

    private void LoadFisDetay()
    {
        // Ornek veri (gercek uygulamada servisden cekilecek)
        txtFisNo.Text = $"{DateTime.Now:yyyyMM}-0001";
        dpTarih.SelectedDate = DateTime.Now.AddDays(-5);
        txtAciklama.Text = "Personel maas tahakkuku";
        cmbFisTuru.SelectedIndex = 0;
        cmbDurum.SelectedIndex = 1;

        _satirlar.Add(new FisSatirItem 
        { 
            Sira = 1, 
            HesapKodu = "770", 
            HesapAdi = "Genel Yonetim Giderleri",
            Aciklama = "Ocak 2025 maas",
            Borc = 100000,
            Alacak = 0
        });
        _satirlar.Add(new FisSatirItem 
        { 
            Sira = 2, 
            HesapKodu = "361", 
            HesapAdi = "Odenecek SGK Primleri",
            Aciklama = "Ocak 2025 SGK",
            Borc = 25000,
            Alacak = 0
        });
        _satirlar.Add(new FisSatirItem 
        { 
            Sira = 3, 
            HesapKodu = "335", 
            HesapAdi = "Personele Borclar",
            Aciklama = "Ocak 2025 net odeme",
            Borc = 0,
            Alacak = 75000
        });
        _satirlar.Add(new FisSatirItem 
        { 
            Sira = 4, 
            HesapKodu = "360", 
            HesapAdi = "Odenecek Vergi ve Fonlar",
            Aciklama = "Ocak 2025 vergi",
            Borc = 0,
            Alacak = 50000
        });

        UpdateTotals();
    }

    private string GenerateNewFisNo()
    {
        return $"{DateTime.Now:yyyyMM}-{DateTime.Now.Ticks % 10000:D4}";
    }

    private void BtnSatirEkle_Click(object sender, RoutedEventArgs e)
    {
        var nextSira = _satirlar.Any() ? _satirlar.Max(s => s.Sira) + 1 : 1;
        _satirlar.Add(new FisSatirItem { Sira = nextSira });
    }

    private void BtnSatirSil_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is FisSatirItem satir)
        {
            _satirlar.Remove(satir);
            ReorderSatirlar();
            UpdateTotals();
        }
    }

    private void ReorderSatirlar()
    {
        int sira = 1;
        foreach (var satir in _satirlar)
        {
            satir.Sira = sira++;
        }
    }

    private void DgSatirlar_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        // Kisa bir gecikme ile toplami guncelle
        Dispatcher.BeginInvoke(new Action(UpdateTotals), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void UpdateTotals()
    {
        var toplamBorc = _satirlar.Sum(s => s.Borc);
        var toplamAlacak = _satirlar.Sum(s => s.Alacak);
        var fark = toplamBorc - toplamAlacak;

        txtToplamBorc.Text = toplamBorc.ToString("N2");
        txtToplamAlacak.Text = toplamAlacak.ToString("N2");
        txtFark.Text = fark.ToString("N2");

        if (Math.Abs(fark) > 0.01m)
        {
            txtFark.Foreground = System.Windows.Media.Brushes.Red;
            txtDengeUyari.Text = "?? Borc ve Alacak eslesmeli! Fark: " + fark.ToString("N2") + " TL";
        }
        else
        {
            txtFark.Foreground = System.Windows.Media.Brushes.LightGreen;
            txtDengeUyari.Text = "? Fis dengeli";
        }
    }

    private bool ValidateFis()
    {
        if (string.IsNullOrWhiteSpace(txtAciklama.Text))
        {
            MessageBox.Show("Aciklama giriniz.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!dpTarih.SelectedDate.HasValue)
        {
            MessageBox.Show("Tarih seciniz.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!_satirlar.Any(s => s.Borc > 0 || s.Alacak > 0))
        {
            MessageBox.Show("En az bir satir giriniz.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        var toplamBorc = _satirlar.Sum(s => s.Borc);
        var toplamAlacak = _satirlar.Sum(s => s.Alacak);
        if (Math.Abs(toplamBorc - toplamAlacak) > 0.01m)
        {
            MessageBox.Show("Borc ve Alacak toplamlari esit olmali!", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void BtnKaydet_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateFis()) return;

        // Taslak olarak kaydet
        SaveFis(RecordStatus.Draft);
    }

    private void BtnOnayla_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateFis()) return;

        // Onayli olarak kaydet
        SaveFis(RecordStatus.Approved);
    }

    private void SaveFis(RecordStatus status)
    {
        try
        {
            // TODO: Veritabanina kaydet
            var statusText = status == RecordStatus.Approved ? "onaylandi ve" : "";
            MessageBox.Show($"Fis {statusText} kaydedildi.", "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnIptal_Click(object sender, RoutedEventArgs e)
    {
        if (_satirlar.Any(s => s.Borc > 0 || s.Alacak > 0))
        {
            if (MessageBox.Show("Kaydedilmemis degisiklikler kaybolacak. Iptal etmek istediginizden emin misiniz?",
                "Iptal Onay", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
        }

        DialogResult = false;
        Close();
    }
}

public class FisSatirItem : System.ComponentModel.INotifyPropertyChanged
{
    private int _sira;
    private string _hesapKodu = string.Empty;
    private string _hesapAdi = string.Empty;
    private string _aciklama = string.Empty;
    private decimal _borc;
    private decimal _alacak;

    public int Sira
    {
        get => _sira;
        set { _sira = value; OnPropertyChanged(nameof(Sira)); }
    }

    public string HesapKodu
    {
        get => _hesapKodu;
        set { _hesapKodu = value; OnPropertyChanged(nameof(HesapKodu)); }
    }

    public string HesapAdi
    {
        get => _hesapAdi;
        set { _hesapAdi = value; OnPropertyChanged(nameof(HesapAdi)); }
    }

    public string Aciklama
    {
        get => _aciklama;
        set { _aciklama = value; OnPropertyChanged(nameof(Aciklama)); }
    }

    public decimal Borc
    {
        get => _borc;
        set { _borc = value; OnPropertyChanged(nameof(Borc)); }
    }

    public decimal Alacak
    {
        get => _alacak;
        set { _alacak = value; OnPropertyChanged(nameof(Alacak)); }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
}