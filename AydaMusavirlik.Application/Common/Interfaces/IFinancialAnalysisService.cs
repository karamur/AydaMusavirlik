using AydaMusavirlik.Core.Models.FinancialAnalysis;

namespace AydaMusavirlik.Application.Common.Interfaces;

/// <summary>
/// Mali Analiz Servisi Interface
/// </summary>
public interface IFinancialAnalysisService
{
    /// <summary>
    /// Rasyo analizi yapar
    /// </summary>
    Task<RasyoAnaliziResult> CalculateRasyoAnaliziAsync(int companyId, int yil, CancellationToken cancellationToken = default);

    /// <summary>
    /// DuPont analizi yapar
    /// </summary>
    Task<DuPontAnaliziResult> CalculateDuPontAnaliziAsync(int companyId, int yil, CancellationToken cancellationToken = default);

    /// <summary>
    /// SWOT analizi yapar
    /// </summary>
    Task<SwotAnaliziResult> CalculateSwotAnaliziAsync(int companyId, int yil, CancellationToken cancellationToken = default);

    /// <summary>
    /// Trend analizi yapar
    /// </summary>
    Task<TrendAnaliziResult> CalculateTrendAnaliziAsync(int companyId, int baslangicYil, int bitisYil, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mali saglik puani hesaplar (0-100)
    /// </summary>
    Task<int> CalculateMaliSaglikPuaniAsync(int companyId, int yil, CancellationToken cancellationToken = default);
}