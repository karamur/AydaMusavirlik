namespace AydaMusavirlik.Models.Financial;

/// <summary>
/// Finansal Oran Modeli
/// </summary>
public class FinancialRatio
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? PreviousValue { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RatioStatus Status { get; set; }
    public decimal? TargetValue { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public enum RatioStatus
{
    Excellent,
    Good,
    Warning,
    Critical
}

/// <summary>
/// Gelir Tablosu
/// </summary>
public class IncomeStatement
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit => Revenue - CostOfGoodsSold;
    public decimal OperatingExpenses { get; set; }
    public decimal OperatingIncome => GrossProfit - OperatingExpenses;
    public decimal InterestExpense { get; set; }
    public decimal TaxExpense { get; set; }
    public decimal NetIncome => OperatingIncome - InterestExpense - TaxExpense;
    public decimal GrossProfitMargin => Revenue != 0 ? (GrossProfit / Revenue) * 100 : 0;
    public decimal NetProfitMargin => Revenue != 0 ? (NetIncome / Revenue) * 100 : 0;
}

/// <summary>
/// Bilanço
/// </summary>
public class BalanceSheet
{
    public int Year { get; set; }
    public int Month { get; set; }

    // Dönen Varlýklar
    public decimal Cash { get; set; }
    public decimal AccountsReceivable { get; set; }
    public decimal Inventory { get; set; }
    public decimal OtherCurrentAssets { get; set; }
    public decimal CurrentAssets => Cash + AccountsReceivable + Inventory + OtherCurrentAssets;

    // Duran Varlýklar
    public decimal FixedAssets { get; set; }
    public decimal IntangibleAssets { get; set; }
    public decimal NonCurrentAssets => FixedAssets + IntangibleAssets;

    public decimal TotalAssets => CurrentAssets + NonCurrentAssets;

    // Kýsa Vadeli Borçlar
    public decimal AccountsPayable { get; set; }
    public decimal ShortTermDebt { get; set; }
    public decimal OtherCurrentLiabilities { get; set; }
    public decimal CurrentLiabilities => AccountsPayable + ShortTermDebt + OtherCurrentLiabilities;

    // Uzun Vadeli Borçlar
    public decimal LongTermDebt { get; set; }
    public decimal NonCurrentLiabilities => LongTermDebt;

    public decimal TotalLiabilities => CurrentLiabilities + NonCurrentLiabilities;

    // Öz Sermaye
    public decimal ShareCapital { get; set; }
    public decimal RetainedEarnings { get; set; }
    public decimal Equity => ShareCapital + RetainedEarnings;
}

/// <summary>
/// Nakit Akýþý
/// </summary>
public class CashFlow
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal OperatingCashFlow { get; set; }
    public decimal InvestingCashFlow { get; set; }
    public decimal FinancingCashFlow { get; set; }
    public decimal NetCashFlow => OperatingCashFlow + InvestingCashFlow + FinancingCashFlow;
    public decimal BeginningCash { get; set; }
    public decimal EndingCash => BeginningCash + NetCashFlow;
}

/// <summary>
/// Trend Verisi
/// </summary>
public class TrendData
{
    public string Period { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Category { get; set; } = string.Empty;
}
