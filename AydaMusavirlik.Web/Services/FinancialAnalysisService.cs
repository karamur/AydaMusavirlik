using AydaMusavirlik.Models.Financial;

namespace AydaMusavirlik.Services;

/// <summary>
/// Profesyonel Finansal Analiz Servisi
/// </summary>
public class FinancialAnalysisService
{
    private readonly ILogger<FinancialAnalysisService> _logger;

    public FinancialAnalysisService(ILogger<FinancialAnalysisService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Finansal oranlarý hesapla
    /// </summary>
    public List<FinancialRatio> CalculateRatios(BalanceSheet balance, IncomeStatement income)
    {
        var ratios = new List<FinancialRatio>();

        // LÝKÝDÝTE ORANLARI
        ratios.Add(new FinancialRatio
        {
            Name = "Cari Oran",
            Value = balance.CurrentLiabilities != 0 ? Math.Round(balance.CurrentAssets / balance.CurrentLiabilities, 2) : 0,
            Category = "Likidite",
            Description = "Kýsa vadeli borçlarý ödeme kapasitesi",
            TargetValue = 2.0m,
            Status = GetRatioStatus(balance.CurrentAssets / (balance.CurrentLiabilities == 0 ? 1 : balance.CurrentLiabilities), 2.0m, 1.5m, 1.0m)
        });

        ratios.Add(new FinancialRatio
        {
            Name = "Asit-Test (Likidite) Oraný",
            Value = balance.CurrentLiabilities != 0 ? Math.Round((balance.CurrentAssets - balance.Inventory) / balance.CurrentLiabilities, 2) : 0,
            Category = "Likidite",
            Description = "Stok hariç kýsa vadeli borç ödeme kapasitesi",
            TargetValue = 1.0m,
            Status = GetRatioStatus((balance.CurrentAssets - balance.Inventory) / (balance.CurrentLiabilities == 0 ? 1 : balance.CurrentLiabilities), 1.0m, 0.8m, 0.5m)
        });

        ratios.Add(new FinancialRatio
        {
            Name = "Nakit Oraný",
            Value = balance.CurrentLiabilities != 0 ? Math.Round(balance.Cash / balance.CurrentLiabilities, 2) : 0,
            Category = "Likidite",
            Description = "Anlýk nakit ile borç ödeme kapasitesi",
            TargetValue = 0.5m,
            Status = GetRatioStatus(balance.Cash / (balance.CurrentLiabilities == 0 ? 1 : balance.CurrentLiabilities), 0.5m, 0.3m, 0.1m)
        });

        // KARLILIK ORANLARI
        ratios.Add(new FinancialRatio
        {
            Name = "Brüt Kar Marjý",
            Value = Math.Round(income.GrossProfitMargin, 2),
            Category = "Karlýlýk",
            Description = "Satýþlardan elde edilen brüt kar oraný",
            Unit = "%",
            TargetValue = 30m,
            Status = GetRatioStatus(income.GrossProfitMargin, 30m, 20m, 10m)
        });

        ratios.Add(new FinancialRatio
        {
            Name = "Net Kar Marjý",
            Value = Math.Round(income.NetProfitMargin, 2),
            Category = "Karlýlýk",
            Description = "Tüm giderler sonrasý kar oraný",
            Unit = "%",
            TargetValue = 15m,
            Status = GetRatioStatus(income.NetProfitMargin, 15m, 10m, 5m)
        });

        var roa = balance.TotalAssets != 0 ? (income.NetIncome / balance.TotalAssets) * 100 : 0;
        ratios.Add(new FinancialRatio
        {
            Name = "Aktif Karlýlýk (ROA)",
            Value = Math.Round(roa, 2),
            Category = "Karlýlýk",
            Description = "Varlýklarýn ne kadar verimli kullanýldýðý",
            Unit = "%",
            TargetValue = 10m,
            Status = GetRatioStatus(roa, 10m, 5m, 2m)
        });

        var roe = balance.Equity != 0 ? (income.NetIncome / balance.Equity) * 100 : 0;
        ratios.Add(new FinancialRatio
        {
            Name = "Özsermaye Karlýlýðý (ROE)",
            Value = Math.Round(roe, 2),
            Category = "Karlýlýk",
            Description = "Ortaklarýn yatýrým getirisi",
            Unit = "%",
            TargetValue = 15m,
            Status = GetRatioStatus(roe, 15m, 10m, 5m)
        });

        // FAALÝYET ORANLARI
        var receivableTurnover = balance.AccountsReceivable != 0 ? income.Revenue / balance.AccountsReceivable : 0;
        ratios.Add(new FinancialRatio
        {
            Name = "Alacak Devir Hýzý",
            Value = Math.Round(receivableTurnover, 2),
            Category = "Faaliyet",
            Description = "Alacaklarýn yýlda kaç kez tahsil edildiði",
            TargetValue = 12m,
            Status = GetRatioStatus(receivableTurnover, 12m, 8m, 4m)
        });

        var inventoryTurnover = balance.Inventory != 0 ? income.CostOfGoodsSold / balance.Inventory : 0;
        ratios.Add(new FinancialRatio
        {
            Name = "Stok Devir Hýzý",
            Value = Math.Round(inventoryTurnover, 2),
            Category = "Faaliyet",
            Description = "Stoklarýn yýlda kaç kez satýldýðý",
            TargetValue = 6m,
            Status = GetRatioStatus(inventoryTurnover, 6m, 4m, 2m)
        });

        var assetTurnover = balance.TotalAssets != 0 ? income.Revenue / balance.TotalAssets : 0;
        ratios.Add(new FinancialRatio
        {
            Name = "Aktif Devir Hýzý",
            Value = Math.Round(assetTurnover, 2),
            Category = "Faaliyet",
            Description = "Varlýklarýn satýþ üretme verimliliði",
            TargetValue = 1.5m,
            Status = GetRatioStatus(assetTurnover, 1.5m, 1.0m, 0.5m)
        });

        // BORÇLULUK ORANLARI
        var debtRatio = balance.TotalAssets != 0 ? (balance.TotalLiabilities / balance.TotalAssets) * 100 : 0;
        ratios.Add(new FinancialRatio
        {
            Name = "Borç Oraný",
            Value = Math.Round(debtRatio, 2),
            Category = "Borçluluk",
            Description = "Varlýklarýn ne kadarýnýn borçla finanse edildiði",
            Unit = "%",
            TargetValue = 50m,
            Status = GetDebtRatioStatus(debtRatio)
        });

        var debtToEquity = balance.Equity != 0 ? balance.TotalLiabilities / balance.Equity : 0;
        ratios.Add(new FinancialRatio
        {
            Name = "Borç/Özsermaye Oraný",
            Value = Math.Round(debtToEquity, 2),
            Category = "Borçluluk",
            Description = "Her 1 TL özsermayeye karþýlýk borç",
            TargetValue = 1.0m,
            Status = GetDebtRatioStatus(debtToEquity * 100)
        });

        var interestCoverage = income.InterestExpense != 0 ? income.OperatingIncome / income.InterestExpense : 99;
        ratios.Add(new FinancialRatio
        {
            Name = "Faiz Karþýlama Oraný",
            Value = Math.Round(interestCoverage, 2),
            Category = "Borçluluk",
            Description = "Faiz giderlerini karþýlama kapasitesi",
            TargetValue = 3m,
            Status = GetRatioStatus(interestCoverage, 3m, 2m, 1m)
        });

        return ratios;
    }

    private RatioStatus GetRatioStatus(decimal value, decimal excellent, decimal good, decimal warning)
    {
        if (value >= excellent) return RatioStatus.Excellent;
        if (value >= good) return RatioStatus.Good;
        if (value >= warning) return RatioStatus.Warning;
        return RatioStatus.Critical;
    }

    private RatioStatus GetDebtRatioStatus(decimal value)
    {
        if (value <= 40) return RatioStatus.Excellent;
        if (value <= 60) return RatioStatus.Good;
        if (value <= 80) return RatioStatus.Warning;
        return RatioStatus.Critical;
    }

    /// <summary>
    /// Örnek bilanço verisi
    /// </summary>
    public BalanceSheet GetSampleBalanceSheet()
    {
        return new BalanceSheet
        {
            Year = DateTime.Now.Year,
            Month = DateTime.Now.Month,
            Cash = 250000,
            AccountsReceivable = 450000,
            Inventory = 320000,
            OtherCurrentAssets = 80000,
            FixedAssets = 1200000,
            IntangibleAssets = 150000,
            AccountsPayable = 280000,
            ShortTermDebt = 200000,
            OtherCurrentLiabilities = 120000,
            LongTermDebt = 500000,
            ShareCapital = 800000,
            RetainedEarnings = 550000
        };
    }

    /// <summary>
    /// Örnek gelir tablosu verisi
    /// </summary>
    public IncomeStatement GetSampleIncomeStatement()
    {
        return new IncomeStatement
        {
            Year = DateTime.Now.Year,
            Month = DateTime.Now.Month,
            Revenue = 2500000,
            CostOfGoodsSold = 1500000,
            OperatingExpenses = 650000,
            InterestExpense = 45000,
            TaxExpense = 76250
        };
    }

    /// <summary>
    /// Aylýk trend verisi
    /// </summary>
    public List<TrendData> GetMonthlyTrend()
    {
        var data = new List<TrendData>();
        var random = new Random(42);
        var baseRevenue = 180000m;
        var baseExpense = 140000m;

        for (int i = 11; i >= 0; i--)
        {
            var date = DateTime.Now.AddMonths(-i);
            var monthName = date.ToString("MMM yy");

            // Gelir trendi (hafif artýþ)
            var revenue = baseRevenue * (1 + (11 - i) * 0.02m) + (decimal)(random.NextDouble() * 20000 - 10000);
            data.Add(new TrendData { Period = monthName, Value = Math.Round(revenue, 0), Category = "Gelir" });

            // Gider trendi
            var expense = baseExpense * (1 + (11 - i) * 0.015m) + (decimal)(random.NextDouble() * 15000 - 7500);
            data.Add(new TrendData { Period = monthName, Value = Math.Round(expense, 0), Category = "Gider" });

            // Kar trendi
            data.Add(new TrendData { Period = monthName, Value = Math.Round(revenue - expense, 0), Category = "Kar" });
        }

        return data;
    }

    /// <summary>
    /// Gider daðýlýmý
    /// </summary>
    public List<(string Category, decimal Amount, decimal Percentage)> GetExpenseBreakdown()
    {
        var total = 650000m;
        return new List<(string, decimal, decimal)>
        {
            ("Personel Giderleri", 260000, 40),
            ("Kira ve Altyapý", 97500, 15),
            ("Pazarlama", 78000, 12),
            ("Üretim Giderleri", 65000, 10),
            ("Yönetim Giderleri", 58500, 9),
            ("Ar-Ge", 45500, 7),
            ("Diðer Giderler", 45500, 7)
        };
    }

    /// <summary>
    /// Nakit akýþý verisi
    /// </summary>
    public List<CashFlow> GetCashFlowData()
    {
        var data = new List<CashFlow>();
        var beginningCash = 180000m;

        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.Now.AddMonths(-i);
            var cf = new CashFlow
            {
                Year = date.Year,
                Month = date.Month,
                BeginningCash = beginningCash,
                OperatingCashFlow = 85000 + (5 - i) * 5000,
                InvestingCashFlow = -25000 - (i % 3) * 10000,
                FinancingCashFlow = -15000
            };
            data.Add(cf);
            beginningCash = cf.EndingCash;
        }

        return data;
    }
}
