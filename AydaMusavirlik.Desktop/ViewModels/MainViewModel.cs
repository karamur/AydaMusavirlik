using CommunityToolkit.Mvvm.ComponentModel;

namespace AydaMusavirlik.Desktop.ViewModels;

/// <summary>
/// Ana pencere ViewModel
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Ayda Müşavirlik";

    [ObservableProperty]
    private string _currentUser = "Kullanıcı";

    [ObservableProperty]
    private bool _isLoading;

    public MainViewModel()
    {
    }
}
