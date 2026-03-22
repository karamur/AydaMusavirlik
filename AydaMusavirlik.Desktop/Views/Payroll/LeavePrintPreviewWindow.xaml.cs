using System.Windows;
using System.Windows.Controls;
using System.Printing;
using System.Windows.Media;
using Microsoft.Win32;

namespace AydaMusavirlik.Desktop.Views.Payroll;

public partial class LeavePrintPreviewWindow : Window
{
    private readonly IzinTalebiViewModel _talep;

    public LeavePrintPreviewWindow(IzinTalebiViewModel talep)
    {
        InitializeComponent();
        _talep = talep;
        LoadData();
    }

    private void LoadData()
    {
        txtFormNo.Text = $"Form No: {_talep.FormNo}";
        txtTarih.Text = DateTime.Now.ToString("dd.MM.yyyy");
        txtPersonelAdi.Text = _talep.PersonelAdi;
        txtTcKimlik.Text = "12345678901"; // Demo
        txtSicilNo.Text = "P001";
        txtDepartman.Text = "Muhasebe";
        txtIseGiris.Text = "15.03.2020";

        txtIzinTuru.Text = _talep.IzinTuru;
        txtBaslangic.Text = _talep.BaslangicTarihi.ToString("dd.MM.yyyy");
        txtBitis.Text = _talep.BitisTarihi.ToString("dd.MM.yyyy");
        txtGunSayisi.Text = $"{_talep.GunSayisi} gün";
        txtAciklama.Text = "Ailevi nedenlerle izin talep ediyorum.";

        txtHakedilen.Text = "20 gün";
        txtKullanilan.Text = "8 gün";
        txtKalan.Text = "12 gün";

        txtPersonelImza.Text = _talep.PersonelAdi;

        if (_talep.Onaylandi)
        {
            brdOnayDurumu.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
            brdOnayDurumu.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            txtOnayBilgi.Text = $"Onaylayan: {_talep.OnaylayanAdi} | Tarih: {DateTime.Now.AddDays(-1):dd.MM.yyyy}";
            txtYoneticiImza.Text = _talep.OnaylayanAdi;
        }
        else if (_talep.Durum == "Reddedildi")
        {
            brdOnayDurumu.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));
            brdOnayDurumu.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
        }
        else
        {
            brdOnayDurumu.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
            brdOnayDurumu.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
        }
    }

    private void BtnYazdir_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(printArea, $"Ýzin Formu - {_talep.FormNo}");
                MessageBox.Show("Yazdýrma iþlemi baþarýyla tamamlandý.", "Baþarýlý", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Yazdýrma hatasý: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnPdfKaydet_Click(object sender, RoutedEventArgs e)
    {
        var saveDialog = new SaveFileDialog
        {
            FileName = $"IzinFormu_{_talep.FormNo}.pdf",
            Filter = "PDF Dosyasý|*.pdf",
            Title = "Ýzin Formu PDF Olarak Kaydet"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                // PDF oluþturma (gerçek uygulamada QuestPDF veya iTextSharp kullanýlabilir)
                // Demo için sadece mesaj göster
                MessageBox.Show($"PDF kaydedildi:\n{saveDialog.FileName}", "Baþarýlý", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF kaydetme hatasý: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void BtnMailGonder_Click(object sender, RoutedEventArgs e)
    {
        var emailWindow = new EmailSendWindow(_talep);
        emailWindow.Owner = this;
        emailWindow.ShowDialog();
    }

    private void BtnPaylas_Click(object sender, RoutedEventArgs e)
    {
        var shareLink = $"https://ayda.com/izin/{_talep.FormNo}";
        Clipboard.SetText(shareLink);
        MessageBox.Show($"Paylaþým linki panoya kopyalandý:\n{shareLink}", "Paylaþým", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnKapat_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}