using System.Windows;

namespace AydaMusavirlik.Desktop.Views.Payroll;

public partial class EmailSendWindow : Window
{
    private readonly IzinTalebiViewModel _talep;

    public EmailSendWindow(IzinTalebiViewModel talep)
    {
        InitializeComponent();
        _talep = talep;
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

Ýzin formunuz ekte sunulmuþtur.

Ýyi tatiller dileriz.

Ayda Müþavirlik
Ýnsan Kaynaklarý";

        txtFormNo.Text = $"{_talep.FormNo}.pdf";
    }

    private void BtnGonder_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtAlici.Text))
        {
            MessageBox.Show("Lütfen alýcý e-posta adresini girin.", "Uyarý", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // E-posta gönderme (gerçek uygulamada SMTP servisi kullanýlacak)
        MessageBox.Show($"E-posta baþarýyla gönderildi.\n\nAlýcý: {txtAlici.Text}", "Baþarýlý", MessageBoxButton.OK, MessageBoxImage.Information);

        DialogResult = true;
        Close();
    }

    private void BtnIptal_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}