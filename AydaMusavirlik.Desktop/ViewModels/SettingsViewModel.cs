using CommunityToolkit.Mvvm.ComponentModel;

namespace AydaMusavirlik.Desktop.ViewModels;

/// <summary>
/// Ayarlar ViewModel
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Ayarlar";

    [ObservableProperty]
    private bool _isLoading;

    public SettingsViewModel()
    {
    }
}
