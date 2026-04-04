using CommunityToolkit.Mvvm.ComponentModel;
using AydaMusavirlik.Data;

namespace AydaMusavirlik.Desktop.ViewModels;

/// <summary>
/// Denetim ViewModel
/// </summary>
public partial class AuditViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private string _title = "Denetim";

    [ObservableProperty]
    private bool _isLoading;

    public AuditViewModel(AppDbContext context)
    {
        _context = context;
    }
}
