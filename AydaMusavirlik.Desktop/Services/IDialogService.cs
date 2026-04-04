namespace AydaMusavirlik.Desktop.Services;

/// <summary>
/// Dialog gösterme servisi için interface
/// </summary>
public interface IDialogService
{
    void ShowMessage(string message, string title = "Bilgi");
    void ShowInfo(string message, string title = "Bilgi");
    void ShowError(string message, string title = "Hata");
    void ShowWarning(string message, string title = "Uyarı");
    bool ShowConfirmation(string message, string title = "Onay");
    bool Confirm(string message, string title = "Onay");
    string? ShowInputDialog(string message, string title = "Giriş", string defaultValue = "");
}
