namespace AydaMusavirlik.Desktop.Services;

/// <summary>
/// Navigasyon servisi implementasyonu
/// </summary>
public class NavigationService : INavigationService
{
    private readonly Stack<string> _navigationStack = new();

    public bool CanGoBack => _navigationStack.Count > 1;

    public void NavigateTo(string viewName)
    {
        _navigationStack.Push(viewName);
    }

    public void NavigateTo(string viewName, object parameter)
    {
        _navigationStack.Push(viewName);
    }

    public void GoBack()
    {
        if (CanGoBack)
        {
            _navigationStack.Pop();
        }
    }
}
