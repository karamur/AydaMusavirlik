using CommunityToolkit.Mvvm.ComponentModel;
using AydaMusavirlik.Data;

namespace AydaMusavirlik.Desktop.ViewModels;

/// <summary>
/// Şirket kurulumu ViewModel
/// </summary>
public partial class CompanyFormationViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private string _title = "Şirket Kurulumu";

    [ObservableProperty]
    private bool _isLoading;

    public CompanyFormationViewModel(AppDbContext context)
    {
        _context = context;
    }
}
