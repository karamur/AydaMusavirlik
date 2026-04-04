using CommunityToolkit.Mvvm.ComponentModel;
using AydaMusavirlik.Data;

namespace AydaMusavirlik.Desktop.ViewModels;

/// <summary>
/// Finansal analiz ViewModel
/// </summary>
public partial class FinancialAnalysisViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private string _title = "Finansal Analiz";

    [ObservableProperty]
    private bool _isLoading;

    public FinancialAnalysisViewModel(AppDbContext context)
    {
        _context = context;
    }
}
