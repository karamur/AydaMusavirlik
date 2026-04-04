using CommunityToolkit.Mvvm.ComponentModel;
using AydaMusavirlik.Data;

namespace AydaMusavirlik.Desktop.ViewModels;

/// <summary>
/// Ar-Ge projeleri ViewModel
/// </summary>
public partial class ArGeViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private string _title = "Ar-Ge Projeleri";

    [ObservableProperty]
    private bool _isLoading;

    public ArGeViewModel(AppDbContext context)
    {
        _context = context;
    }
}
