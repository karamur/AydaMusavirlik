namespace AydaMusavirlik.Desktop.Services;

/// <summary>
/// Navigasyon servisi için interface
/// </summary>
public interface INavigationService
{
    void NavigateTo(string viewName);
    void NavigateTo(string viewName, object parameter);
    bool CanGoBack { get; }
    void GoBack();
}
