using System.Diagnostics;
using System.Windows;

namespace AydaMusavirlik.Desktop.Views.Payroll;

public partial class ShareOptionsWindow : Window
{
    private readonly string _filePath;
    private readonly string _formNo;

    public ShareOptionsWindow(string filePath, string formNo)
    {
        InitializeComponent();
        _filePath = filePath;
        _formNo = formNo;
        txtFormNo.Text = formNo;
    }

    private void BtnWhatsApp_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // WhatsApp Web ile paylaþ
            var message = Uri.EscapeDataString($"Ýzin Formu: {_formNo}\nDosya ekte sunulmuþtur.");
            var url = $"https://web.whatsapp.com/send?text={message}";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

            // Dosyayý panoya kopyala (kullanýcý yapýþtýrabilsin)
            Clipboard.SetText(_filePath);
            MessageBox.Show("WhatsApp Web açýldý.\nDosya yolu panoya kopyalandý, sohbete yapýþtýrabilirsiniz.", 
                "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnTelegram_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var message = Uri.EscapeDataString($"Ýzin Formu: {_formNo}");
            var url = $"https://t.me/share/url?url={message}";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

            Clipboard.SetText(_filePath);
            MessageBox.Show("Telegram açýldý.\nDosya yolu panoya kopyalandý.", 
                "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnKopyala_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_filePath);
        MessageBox.Show($"Dosya yolu panoya kopyalandý:\n{_filePath}", 
            "Kopyalandý", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnKlasorAc_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var directory = System.IO.Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{_filePath}\"") { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnWindowsPaylas_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Windows varsayýlan paylaþým
            Process.Start(new ProcessStartInfo(_filePath) { UseShellExecute = true, Verb = "share" });
        }
        catch
        {
            // Paylaþým desteklenmiyorsa dosyayý aç
            try
            {
                Process.Start(new ProcessStartInfo(_filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void BtnKapat_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}