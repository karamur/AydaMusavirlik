using System.Windows;

namespace AydaMusavirlik.Desktop.Views.Payroll;

public partial class EmailSendWindow : Window
{
    private readonly IzinTalebiViewModel _talep;
    private readonly byte[]? _pdfBytes;

    public EmailSendWindow(IzinTalebiViewModel talep, byte[]? pdfBytes = null)
    {
        InitializeComponent();
        _talep = talep;
        _pdfBytes = pdfBytes;
        LoadDefaults();
    }

    private void LoadDefaults()
    {
        txtAlici.Text = "personel@firma.com"; // Demo
        txtKonu.Text = $"Ýzin Formu - {_talep.FormNo}";
        txtMesaj.Text = $@"Sayýn {_talep.PersonelAdi},

Ýzin talebiniz ({_talep.FormNo}) onaylanmýþtýr.

Ýzin Detaylarý:
- Ýzin Türü: {_talep.IzinTuru}
- Baþlangýç: {_talep.BaslangicTarihi:dd.MM.yyyy}
- Bitiþ: {_talep.BitisTarihi:dd.MM.yyyy}
- Toplam: {_talep.GunSayisi} gün

Ýzin formunuz ekte PDF olarak sunulmuþtur.

Ýyi tatiller dileriz.

Ayda Müþavirlik
Ýnsan Kaynaklarý";

        txtFormNo.Text = $"{_talep.FormNo}.pdf ({(_pdfBytes?.Length ?? 0) / 1024} KB)";
    }

    private void BtnGonder_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtAlici.Text))
        {
            MessageBox.Show("Lütfen alýcý e-posta adresini girin.", "Uyarý", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // E-posta gönderme
        // Gerçek uygulamada Infrastructure.Services.EmailService kullanýlacak
        
        btnGonder.IsEnabled = false;
        btnGonder.Content = "Gönderiliyor...";

        // Simülasyon
        System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ =>
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"E-posta baþarýyla gönderildi.\n\nAlýcý: {txtAlici.Text}\nEk: {_talep.FormNo}.pdf", 
                    "Baþarýlý", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
                
                DialogResult = true;
                Close();
            });
        });
    }

    private void BtnIptal_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}