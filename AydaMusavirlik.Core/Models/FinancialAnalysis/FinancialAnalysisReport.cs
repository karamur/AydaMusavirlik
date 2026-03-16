using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Core.Models.FinancialAnalysis;

/// <summary>
/// Finansal analiz raporu
/// </summary>
public class FinancialAnalysisReport : SoftDeleteEntity
{
    public int CompanyId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public AnalysisType AnalysisType { get; set; }
    public DateTime AnalysisPeriodStart { get; set; }
    public DateTime AnalysisPeriodEnd { get; set; }
    public DateTime ReportDate { get; set; }
    public string? PreparedBy { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Draft;
    
    // Analiz sonuçlarý (JSON)
    public string? RatioAnalysis { get; set; }           // Oran analizi
    public string? TrendAnalysis { get; set; }           // Trend analizi
    public string? VerticalAnalysis { get; set; }        // Dikey analiz
    public string? HorizontalAnalysis { get; set; }      // Yatay analiz
    
    public string? Summary { get; set; }                 // Özet deðerlendirme
    public string? Recommendations { get; set; }         // Öneriler
    public string? Notes { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<FinancialRatio> Ratios { get; set; } = new List<FinancialRatio>();
}

public enum AnalysisType
{
    LikiditeAnalizi = 1,             // Likidite analizi
    KarlilikAnalizi = 2,             // Karlýlýk analizi
    FinansalYapiAnalizi = 3,         // Finansal yapý analizi
    FaaliyetAnalizi = 4,             // Faaliyet analizi
    BorsaOranAnalizi = 5,            // Borsa oran analizi
    KapsamliAnaliz = 6               // Kapsamlý analiz
}

public enum ReportStatus
{
    Draft = 1,
    InProgress = 2,
    Completed = 3,
    Approved = 4
}

/// <summary>
/// Finansal oran
/// </summary>
public class FinancialRatio : BaseEntity
{
    public int ReportId { get; set; }
    public RatioCategory Category { get; set; }
    public string RatioName { get; set; } = string.Empty;
    public string? Formula { get; set; }
    public decimal Value { get; set; }
    public decimal? PreviousPeriodValue { get; set; }
    public decimal? IndustryAverage { get; set; }
    public RatioEvaluation Evaluation { get; set; }
    public string? Comments { get; set; }

    // Navigation
    public virtual FinancialAnalysisReport Report { get; set; } = null!;
}

public enum RatioCategory
{
    Likidite = 1,          // Likidite oranlarý
    Mali = 2,              // Mali yapý oranlarý
    Faaliyet = 3,          // Faaliyet oranlarý
    Karlilik = 4,          // Karlýlýk oranlarý
    Buyume = 5             // Büyüme oranlarý
}

public enum RatioEvaluation
{
    VeryGood = 1,     // Çok iyi
    Good = 2,         // Ýyi
    Average = 3,      // Orta
    Poor = 4,         // Zayýf
    Critical = 5      // Kritik
}
