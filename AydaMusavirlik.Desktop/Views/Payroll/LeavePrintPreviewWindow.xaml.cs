using System.Windows;
using System.Windows.Controls;
using System.Printing;
using System.Windows.Media;
using System.Diagnostics;
using Microsoft.Win32;
using AydaMusavirlik.Infrastructure.Services;

namespace AydaMusavirlik.Desktop.Views.Payroll;

public partial class LeavePrintPreviewWindow : Window
{
    private readonly IzinTalebiViewModel _talep;
    private readonly ILeaveFormPdfService _pdfService;

    public LeavePrintPreviewWindow(IzinTalebiViewModel talep)
    {
        InitializeComponent();
        _talep = talep;
        _pdfService = new LeaveFormPdfService();
        LoadData();
    }

    private LeaveFormPdfModel CreatePdfModel()
    {
        return new LeaveFormPdfModel
        {
            FormNo = _talep.FormNo,
            FirmaAdi = "Ayda Müþavirlik A.Þ.",
            FirmaAdres = "Merkez Mah. Ýþ Cad. No:1 Ýstanbul",
            PersonelAdi = _talep.PersonelAdi,
            TcKimlikNo = "12345678901",
            SicilNo = "P001",
            Departman = "Muhasebe",
            Pozisyon = "Uzman",
            IseGirisTarihi = new DateTime(2020, 3, 15),
            IzinTuru = _talep.IzinTuru,
            BaslangicTarihi = _talep.BaslangicTarihi,
            BitisTarihi = _talep.BitisTarihi,
            GunSayisi = _talep.GunSayisi,
            Aciklama = "Ailevi nedenlerle izin talep ediyorum.",
            ToplamHakedilen = 20,
            Kullanilan = 8,
            Kalan = 12,
            OnayDurumu = _talep.Durum,
            OnaylayanAdi = _talep.OnaylayanAdi,
            OnayTarihi = _talep.Onaylandi ? DateTime.Now.AddDays(-1) : null
        };
    }

    private void LoadData()
    {
        txtFormNo.Text = $"Form No: {_talep.FormNo}";
        txtTarih.Text = DateTime.Now.ToString("dd.MM.yyyy");
        txtPersonelAdi.Text = _talep.PersonelAdi;
        txtTcKimlik.Text = "12345678901";
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
                var model = CreatePdfModel();
                var pdfBytes = _pdfService.GenerateLeaveFormPdf(model);
                System.IO.File.WriteAllBytes(saveDialog.FileName, pdfBytes);
                
                var result = MessageBox.Show(
                    $"PDF baþarýyla kaydedildi:\n{saveDialog.FileName}\n\nDosyayý açmak ister misiniz?", 
                    "Baþarýlý", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Information);
                
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF kaydetme hatasý: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void BtnMailGonder_Click(object sender, RoutedEventArgs e)
    {
        var model = CreatePdfModel();
        var pdfBytes = _pdfService.GenerateLeaveFormPdf(model);
        
        var emailWindow = new EmailSendWindow(_talep, pdfBytes);
        emailWindow.Owner = this;
        emailWindow.ShowDialog();
    }

    private void BtnPaylas_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Geçici PDF oluþtur
            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"IzinFormu_{_talep.FormNo}.pdf");
            var model = CreatePdfModel();
            var pdfBytes = _pdfService.GenerateLeaveFormPdf(model);
            System.IO.File.WriteAllBytes(tempPath, pdfBytes);
            
            // Paylaþým seçenekleri
            var shareWindow = new ShareOptionsWindow(tempPath, _talep.FormNo);
            shareWindow.Owner = this;
            shareWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Paylaþým hatasý: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnKapat_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}