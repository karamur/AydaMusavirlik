// Önerilen Mali Analiz Servisleri
public interface IFinancialAnalysisService
{
    // Oran Analizleri
    Task<LiquidityRatios> CalculateLiquidityRatiosAsync(int companyId, int fiscalYear);
    Task<ProfitabilityRatios> CalculateProfitabilityRatiosAsync(int companyId, int fiscalYear);
    Task<LeverageRatios> CalculateLeverageRatiosAsync(int companyId, int fiscalYear);
    Task<ActivityRatios> CalculateActivityRatiosAsync(int companyId, int fiscalYear);
    
    // Trend Analizleri
    Task<TrendAnalysis> AnalyzeTrendAsync(int companyId, int startYear, int endYear);
    Task<ComparativeAnalysis> ComparePeriodsAsync(int companyId, params int[] periods);
    
    // Sektör Karþýlaþtýrmasý
    Task<IndustryBenchmark> CompareWithIndustryAsync(int companyId, string industryCode);
}

public interface IReportGeneratorService
{
    // Yasal Raporlar
    Task<Report> GenerateBalanceSheetAsync(int companyId, DateTime date);
    Task<Report> GenerateIncomeStatementAsync(int companyId, DateTime startDate, DateTime endDate);
    Task<Report> GenerateCashFlowStatementAsync(int companyId, DateTime startDate, DateTime endDate);
    
    // E-Defter ve Beyannameler
    Task<EDefterPackage> GenerateEDefterAsync(int companyId, int year, int month);
    Task<Declaration> GenerateKDVDeclarationAsync(int companyId, int year, int month);
    Task<Declaration> GenerateMuhtasarAsync(int companyId, int year, int month);
    
    // Özel Raporlar
    Task<Report> GenerateCustomReportAsync(ReportDefinition definition);
}