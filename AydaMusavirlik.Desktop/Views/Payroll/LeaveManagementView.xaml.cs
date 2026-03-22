using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace AydaMusavirlik.Desktop.Views.Payroll;

public partial class LeaveManagementView : UserControl
{
    private ObservableCollection<IzinTalebiViewModel> _bekleyenTalepler = new();
    private ObservableCollection<IzinTalebiViewModel> _tumTalepler = new();

    public LeaveManagementView()
    {
        InitializeComponent();
        Loaded += LeaveManagementView_Loaded;
    }

    private void LeaveManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        LoadData();
        LoadPersonelList();
    }

    private void LoadData()
    {
        // Demo veri
        _bekleyenTalepler = new ObservableCollection<IzinTalebiViewModel>
        {
            new() { Id = 1, FormNo = "IZN-2025-001", PersonelAdi = "Ahmet Yýlmaz", IzinTuru = "Yýllýk Ýzin", BaslangicTarihi = DateTime.Now.AddDays(5), BitisTarihi = DateTime.Now.AddDays(10), GunSayisi = 6, TalepTarihi = DateTime.Now.AddDays(-2), Durum = "Bekliyor" },
            new() { Id = 2, FormNo = "IZN-2025-002", PersonelAdi = "Mehmet Demir", IzinTuru = "Mazeret Ýzni", BaslangicTarihi = DateTime.Now.AddDays(2), BitisTarihi = DateTime.Now.AddDays(3), GunSayisi = 2, TalepTarihi = DateTime.Now.AddDays(-1), Durum = "Bekliyor" },
            new() { Id = 3, FormNo = "IZN-2025-003", PersonelAdi = "Ayþe Kaya", IzinTuru = "Yýllýk Ýzin", BaslangicTarihi = DateTime.Now.AddDays(15), BitisTarihi = DateTime.Now.AddDays(20), GunSayisi = 6, TalepTarihi = DateTime.Now, Durum = "Bekliyor" },
        };

        _tumTalepler = new ObservableCollection<IzinTalebiViewModel>
        {
            new() { Id = 1, FormNo = "IZN-2025-001", PersonelAdi = "Ahmet Yýlmaz", IzinTuru = "Yýllýk Ýzin", BaslangicTarihi = DateTime.Now.AddDays(5), BitisTarihi = DateTime.Now.AddDays(10), GunSayisi = 6, TalepTarihi = DateTime.Now.AddDays(-2), Durum = "Bekliyor", DurumRenk = "#FF9800" },
            new() { Id = 2, FormNo = "IZN-2025-002", PersonelAdi = "Mehmet Demir", IzinTuru = "Mazeret Ýzni", BaslangicTarihi = DateTime.Now.AddDays(2), BitisTarihi = DateTime.Now.AddDays(3), GunSayisi = 2, TalepTarihi = DateTime.Now.AddDays(-1), Durum = "Bekliyor", DurumRenk = "#FF9800" },
            new() { Id = 4, FormNo = "IZN-2024-025", PersonelAdi = "Ali Yýldýz", IzinTuru = "Yýllýk Ýzin", BaslangicTarihi = DateTime.Now.AddDays(-30), BitisTarihi = DateTime.Now.AddDays(-25), GunSayisi = 6, TalepTarihi = DateTime.Now.AddDays(-35), Durum = "Onaylandý", OnaylayanAdi = "Müdür Bey", DurumRenk = "#4CAF50", Onaylandi = true },
            new() { Id = 5, FormNo = "IZN-2024-024", PersonelAdi = "Fatma Çelik", IzinTuru = "Hastalýk Ýzni", BaslangicTarihi = DateTime.Now.AddDays(-45), BitisTarihi = DateTime.Now.AddDays(-43), GunSayisi = 3, TalepTarihi = DateTime.Now.AddDays(-46), Durum = "Onaylandý", OnaylayanAdi = "Müdür Bey", DurumRenk = "#4CAF50", Onaylandi = true },
            new() { Id = 6, FormNo = "IZN-2024-023", PersonelAdi = "Can Özkan", IzinTuru = "Mazeret Ýzni", BaslangicTarihi = DateTime.Now.AddDays(-60), BitisTarihi = DateTime.Now.AddDays(-60), GunSayisi = 1, TalepTarihi = DateTime.Now.AddDays(-62), Durum = "Reddedildi", OnaylayanAdi = "Müdür Bey", DurumRenk = "#F44336" },
        };

        dgBekleyenTalepler.ItemsSource = _bekleyenTalepler;
        dgTumTalepler.ItemsSource = _tumTalepler;
        txtBekleyenSayisi.Text = _bekleyenTalepler.Count.ToString();
    }

    private void LoadPersonelList()
    {
        cmbPersonel.Items.Clear();
        cmbPersonel.Items.Add("Ahmet Yýlmaz");
        cmbPersonel.Items.Add("Mehmet Demir");
        cmbPersonel.Items.Add("Ayþe Kaya");
        cmbPersonel.Items.Add("Ali Yýldýz");
        cmbPersonel.Items.Add("Fatma Çelik");
        cmbPersonel.SelectedIndex = 0;
    }

    private void CmbDurum_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Filtre uygula
    }

    private void BtnYenile_Click(object sender, RoutedEventArgs e)
    {
        LoadData();
    }

    private void BtnOnayla_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var id = (int)(button?.Tag ?? 0);

        var result = MessageBox.Show(
            "Bu izin talebini onaylamak istediðinize emin misiniz?",
            "Ýzin Onayý",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            var talep = _bekleyenTalepler.FirstOrDefault(t => t.Id == id);
            if (talep != null)
            {
                _bekleyenTalepler.Remove(talep);
                talep.Durum = "Onaylandý";
                talep.DurumRenk = "#4CAF50";
                talep.OnaylayanAdi = "Yönetici";
                talep.Onaylandi = true;

                var existing = _tumTalepler.FirstOrDefault(t => t.Id == id);
                if (existing != null)
                {
                    existing.Durum = "Onaylandý";
                    existing.DurumRenk = "#4CAF50";
                    existing.OnaylayanAdi = "Yönetici";
                    existing.Onaylandi = true;
                }
            }

            txtBekleyenSayisi.Text = _bekleyenTalepler.Count.ToString();
            MessageBox.Show("Ýzin talebi onaylandý. Form yazdýrýlabilir.", "Baþarýlý", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void BtnReddet_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var id = (int)(button?.Tag ?? 0);

        var result = MessageBox.Show(
            "Bu izin talebini reddetmek istediðinize emin misiniz?",
            "Ýzin Reddi",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            var talep = _bekleyenTalepler.FirstOrDefault(t => t.Id == id);
            if (talep != null)
            {
                _bekleyenTalepler.Remove(talep);
                talep.Durum = "Reddedildi";
                talep.DurumRenk = "#F44336";

                var existing = _tumTalepler.FirstOrDefault(t => t.Id == id);
                if (existing != null)
                {
                    existing.Durum = "Reddedildi";
                    existing.DurumRenk = "#F44336";
                }
            }

            txtBekleyenSayisi.Text = _bekleyenTalepler.Count.ToString();
            MessageBox.Show("Ýzin talebi reddedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void BtnDetay_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var id = (int)(button?.Tag ?? 0);

        var talep = _tumTalepler.FirstOrDefault(t => t.Id == id) ?? _bekleyenTalepler.FirstOrDefault(t => t.Id == id);
        if (talep != null)
        {
            var detailWindow = new LeaveDetailWindow(talep);
            detailWindow.Owner = Window.GetWindow(this);
            detailWindow.ShowDialog();
        }
    }

    private void BtnYazdir_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var id = (int)(button?.Tag ?? 0);

        var talep = _tumTalepler.FirstOrDefault(t => t.Id == id);
        if (talep != null)
        {
            var printWindow = new LeavePrintPreviewWindow(talep);
            printWindow.Owner = Window.GetWindow(this);
            printWindow.ShowDialog();
        }
    }

    private void BtnMailGonder_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var id = (int)(button?.Tag ?? 0);

        var talep = _tumTalepler.FirstOrDefault(t => t.Id == id);
        if (talep != null)
        {
            var result = MessageBox.Show(
                $"Ýzin formu ({talep.FormNo}) personelin e-posta adresine gönderilsin mi?",
                "E-posta Gönder",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("E-posta baþarýyla gönderildi.", "Baþarýlý", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private void BtnPdfIndir_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var id = (int)(button?.Tag ?? 0);

        var talep = _tumTalepler.FirstOrDefault(t => t.Id == id);
        if (talep != null)
        {
            var saveDialog = new SaveFileDialog
            {
                FileName = $"IzinFormu_{talep.FormNo}.pdf",
                Filter = "PDF Dosyasý|*.pdf",
                Title = "Ýzin Formu PDF Kaydet"
            };

            if (saveDialog.ShowDialog() == true)
            {
                // PDF oluþtur ve kaydet
                MessageBox.Show($"PDF kaydedildi: {saveDialog.FileName}", "Baþarýlý", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private void CmbPersonel_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbPersonel.SelectedItem == null) return;

        var personelAdi = cmbPersonel.SelectedItem.ToString();

        // Demo veri
        txtToplamHakedilen.Text = "20 gün";
        txtKullanilan.Text = "8 gün";
        txtKalan.Text = "12 gün";

        var personelIzinleri = _tumTalepler
            .Where(t => t.PersonelAdi == personelAdi)
            .Select(t => new
            {
                t.FormNo,
                t.IzinTuru,
                TarihAraligi = $"{t.BaslangicTarihi:dd.MM.yyyy} - {t.BitisTarihi:dd.MM.yyyy}",
                t.GunSayisi,
                t.Durum
            }).ToList();

        dgPersonelIzinleri.ItemsSource = personelIzinleri;
    }
}

public class IzinTalebiViewModel
{
    public int Id { get; set; }
    public string FormNo { get; set; } = "";
    public string PersonelAdi { get; set; } = "";
    public string IzinTuru { get; set; } = "";
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int GunSayisi { get; set; }
    public DateTime TalepTarihi { get; set; }
    public string Durum { get; set; } = "";
    public string DurumRenk { get; set; } = "#9E9E9E";
    public string? OnaylayanAdi { get; set; }
    public bool Onaylandi { get; set; }
}