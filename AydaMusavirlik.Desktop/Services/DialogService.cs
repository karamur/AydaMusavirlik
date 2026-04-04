using System.Windows;

namespace AydaMusavirlik.Desktop.Services;

/// <summary>
/// WPF için dialog gösterme servisi implementasyonu
/// </summary>
public class DialogService : IDialogService
{
    public void ShowMessage(string message, string title = "Bilgi")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowInfo(string message, string title = "Bilgi")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "Hata")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message, string title = "Uyarı")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public bool ShowConfirmation(string message, string title = "Onay")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public bool Confirm(string message, string title = "Onay")
    {
        return ShowConfirmation(message, title);
    }

    public string? ShowInputDialog(string message, string title = "Giriş", string defaultValue = "")
    {
        // Basit input dialog - MessageBox ile null döner, gerekirse özel window oluşturulabilir
        return null;
    }
}
