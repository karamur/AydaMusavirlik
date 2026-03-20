namespace AydaMusavirlik.Desktop.Services.Reports;

/// <summary>
/// Mali Analiz ve Oran Hesaplama Servisi
/// </summary>
public interface IFinancialAnalysisService
{
    LiquidityRatios CalculateLiquidityRatios(BalanceSheetReport balance);
    ProfitabilityRatios CalculateProfitabilityRatios(BalanceSheetReport balance, IncomeStatementReport income);
    LeverageRatios CalculateLeverageRatios(BalanceSheetReport balance);
    ActivityRatios CalculateActivityRatios(BalanceSheetReport balance, IncomeStatementReport income);
    FinancialHealthScore CalculateHealthScore(BalanceSheetReport balance, IncomeStatementReport income);
}

public class FinancialAnalysisService : IFinancialAnalysisService
{
    /// <summary>
    /// Likidite Oranlarý - Kýsa vadeli borç ödeme gücü
    /// </summary>
    public LiquidityRatios CalculateLiquidityRatios(BalanceSheetReport balance)
    {
        var donenVarliklar = balance.DonenVarliklar.Total;
        var stoklar = balance.DonenVarliklar.Items.FirstOrDefault(i => i.Code.StartsWith("15"))?.Amount ?? 0;
        var hazirDegerler = balance.DonenVarliklar.Items.FirstOrDefault(i => i.Code.StartsWith("10"))?.Amount ?? 0;
        var kvyk = balance.KisaVadeliYabanciKaynaklar.Total;

        return new LiquidityRatios
        {
            CariOran = kvyk > 0 ? donenVarliklar / kvyk : 0,
            AsitTestOrani = kvyk > 0 ? (donenVarliklar - stoklar) / kvyk : 0,
            NakitOrani = kvyk > 0 ? hazirDegerler / kvyk : 0,
            NetIsletmeSermayesi = donenVarliklar - kvyk
        };
    }

    /// <summary>
    /// Karlýlýk Oranlarý
    /// </summary>
    public ProfitabilityRatios CalculateProfitabilityRatios(BalanceSheetReport balance, IncomeStatementReport income)
    {
        var netSatislar = income.NetSatislar;
        var brutKar = income.BrutSatisKari;
        var netKar = income.DonemKariZarari;
        var ozkaynaklar = balance.OzKaynaklar.Total;
        var toplamAktif = balance.ToplamAktif;

        return new ProfitabilityRatios
        {
            BrutKarMarji = netSatislar > 0 ? (brutKar / netSatislar) * 100 : 0,
            NetKarMarji = netSatislar > 0 ? (netKar / netSatislar) * 100 : 0,
            OzkaynakKarliligi = ozkaynaklar > 0 ? (netKar / ozkaynaklar) * 100 : 0,
            AktifKarliligi = toplamAktif > 0 ? (netKar / toplamAktif) * 100 : 0
        };
    }

    /// <summary>
    /// Kaldýraç/Borçluluk Oranlarý
    /// </summary>
    public LeverageRatios CalculateLeverageRatios(BalanceSheetReport balance)
    {
        var toplamBorc = balance.KisaVadeliYabanciKaynaklar.Total + balance.UzunVadeliYabanciKaynaklar.Total;
        var ozkaynaklar = balance.OzKaynaklar.Total;
        var toplamAktif = balance.ToplamAktif;

        return new LeverageRatios
        {
            BorcOzkaynakOrani = ozkaynaklar > 0 ? toplamBorc / ozkaynaklar : 0,
            ToplamBorcOrani = toplamAktif > 0 ? (toplamBorc / toplamAktif) * 100 : 0,
            OzkaynakOrani = toplamAktif > 0 ? (ozkaynaklar / toplamAktif) * 100 : 0,
            FinansalKaldirac = ozkaynaklar > 0 ? toplamAktif / ozkaynaklar : 0
        };
    }

    /// <summary>
    /// Faaliyet/Verimlilik Oranlarý
    /// </summary>
    public ActivityRatios CalculateActivityRatios(BalanceSheetReport balance, IncomeStatementReport income)
    {
        var satislar = income.NetSatislar;
        var stoklar = balance.DonenVarliklar.Items.FirstOrDefault(i => i.Code.StartsWith("15"))?.Amount ?? 0;
        var alacaklar = balance.DonenVarliklar.Items.FirstOrDefault(i => i.Code.StartsWith("12"))?.Amount ?? 0;
        var toplamAktif = balance.ToplamAktif;

        return new ActivityRatios
        {
            StokDevirHizi = stoklar > 0 ? income.SatislarinMaliyeti / stoklar : 0,
            StokDevirSuresi = stoklar > 0 ? 365 / (income.SatislarinMaliyeti / stoklar) : 0,
            AlacakDevirHizi = alacaklar > 0 ? satislar / alacaklar : 0,
            AlacakTahsilSuresi = alacaklar > 0 ? 365 / (satislar / alacaklar) : 0,
            AktifDevirHizi = toplamAktif > 0 ? satislar / toplamAktif : 0
        };
    }

    /// <summary>
    /// Genel Mali Saðlýk Puaný (0-100)
    /// </summary>
    public FinancialHealthScore CalculateHealthScore(BalanceSheetReport balance, IncomeStatementReport income)
    {
        var liquidity = CalculateLiquidityRatios(balance);
        var profitability = CalculateProfitabilityRatios(balance, income);
        var leverage = CalculateLeverageRatios(balance);
        var activity = CalculateActivityRatios(balance, income);

        var score = new FinancialHealthScore();

        // Likidite Puaný (25 puan)
        score.LiquidityScore = CalculateLiquidityScore(liquidity);

        // Karlýlýk Puaný (25 puan)
        score.ProfitabilityScore = CalculateProfitabilityScore(profitability);

        // Borçluluk Puaný (25 puan)
        score.LeverageScore = CalculateLeverageScore(leverage);

        // Verimlilik Puaný (25 puan)
        score.ActivityScore = CalculateActivityScore(activity);

        score.TotalScore = score.LiquidityScore + score.ProfitabilityScore + score.LeverageScore + score.ActivityScore;
        score.Grade = GetGrade(score.TotalScore);
        score.Interpretation = GetInterpretation(score.TotalScore);

        return score;
    }

    private decimal CalculateLiquidityScore(LiquidityRatios ratios)
    {
        decimal score = 0;

        // Cari Oran: Ýdeal 1.5-2.5
        if (ratios.CariOran >= 1.5m && ratios.CariOran <= 2.5m) score += 10;
        else if (ratios.CariOran >= 1.0m) score += 5;

        // Asit Test: Ýdeal > 1
        if (ratios.AsitTestOrani >= 1.0m) score += 10;
        else if (ratios.AsitTestOrani >= 0.5m) score += 5;

        // Nakit Oran: Ýdeal > 0.2
        if (ratios.NakitOrani >= 0.2m) score += 5;

        return Math.Min(25, score);
    }

    private decimal CalculateProfitabilityScore(ProfitabilityRatios ratios)
    {
        decimal score = 0;

        // Net Kar Marjý: > 10% mükemmel
        if (ratios.NetKarMarji >= 10) score += 10;
        else if (ratios.NetKarMarji >= 5) score += 7;
        else if (ratios.NetKarMarji > 0) score += 3;

        // Özkaynak Karlýlýðý: > 15% mükemmel
        if (ratios.OzkaynakKarliligi >= 15) score += 10;
        else if (ratios.OzkaynakKarliligi >= 8) score += 6;
        else if (ratios.OzkaynakKarliligi > 0) score += 3;

        // Aktif Karlýlýðý
        if (ratios.AktifKarliligi >= 5) score += 5;
        else if (ratios.AktifKarliligi > 0) score += 2;

        return Math.Min(25, score);
    }

    private decimal CalculateLeverageScore(LeverageRatios ratios)
    {
        decimal score = 25;

        // Borç/Özkaynak < 1 ideal
        if (ratios.BorcOzkaynakOrani > 2) score -= 15;
        else if (ratios.BorcOzkaynakOrani > 1) score -= 8;

        // Özkaynak Oraný > 50% ideal
        if (ratios.OzkaynakOrani < 30) score -= 10;
        else if (ratios.OzkaynakOrani < 50) score -= 5;

        return Math.Max(0, score);
    }

    private decimal CalculateActivityScore(ActivityRatios ratios)
    {
        decimal score = 0;

        // Stok Devir Hýzý > 4 iyi
        if (ratios.StokDevirHizi >= 6) score += 10;
        else if (ratios.StokDevirHizi >= 4) score += 7;
        else if (ratios.StokDevirHizi >= 2) score += 4;

        // Alacak Tahsil Süresi < 60 gün ideal
        if (ratios.AlacakTahsilSuresi <= 30) score += 10;
        else if (ratios.AlacakTahsilSuresi <= 60) score += 6;
        else if (ratios.AlacakTahsilSuresi <= 90) score += 3;

        // Aktif Devir Hýzý
        if (ratios.AktifDevirHizi >= 1.5m) score += 5;
        else if (ratios.AktifDevirHizi >= 1.0m) score += 3;

        return Math.Min(25, score);
    }

    private string GetGrade(decimal score)
    {
        return score switch
        {
            >= 90 => "A+",
            >= 80 => "A",
            >= 70 => "B+",
            >= 60 => "B",
            >= 50 => "C",
            >= 40 => "D",
            _ => "F"
        };
    }

    private string GetInterpretation(decimal score)
    {
        return score switch
        {
            >= 80 => "Mükemmel mali saðlýk. Þirket güçlü bir finansal yapýya sahip.",
            >= 60 => "Ýyi mali saðlýk. Bazý alanlarda iyileþtirme yapýlabilir.",
            >= 40 => "Orta düzeyde mali saðlýk. Dikkat edilmesi gereken alanlar var.",
            >= 20 => "Zayýf mali saðlýk. Acil önlemler gerekli.",
            _ => "Kritik durum. Finansal yapýda ciddi sorunlar mevcut."
        };
    }
}

#region Oran Model Sýnýflarý

public class LiquidityRatios
{
    public decimal CariOran { get; set; }
    public decimal AsitTestOrani { get; set; }
    public decimal NakitOrani { get; set; }
    public decimal NetIsletmeSermayesi { get; set; }

    public string CariOranYorum => CariOran switch
    {
        >= 2 => "Mükemmel",
        >= 1.5m => "Ýyi",
        >= 1 => "Kabul edilebilir",
        _ => "Dikkat"
    };
}

public class ProfitabilityRatios
{
    public decimal BrutKarMarji { get; set; }
    public decimal NetKarMarji { get; set; }
    public decimal OzkaynakKarliligi { get; set; }
    public decimal AktifKarliligi { get; set; }
}

public class LeverageRatios
{
    public decimal BorcOzkaynakOrani { get; set; }
    public decimal ToplamBorcOrani { get; set; }
    public decimal OzkaynakOrani { get; set; }
    public decimal FinansalKaldirac { get; set; }
}

public class ActivityRatios
{
    public decimal StokDevirHizi { get; set; }
    public decimal StokDevirSuresi { get; set; }
    public decimal AlacakDevirHizi { get; set; }
    public decimal AlacakTahsilSuresi { get; set; }
    public decimal AktifDevirHizi { get; set; }
}

public class FinancialHealthScore
{
    public decimal LiquidityScore { get; set; }
    public decimal ProfitabilityScore { get; set; }
    public decimal LeverageScore { get; set; }
    public decimal ActivityScore { get; set; }
    public decimal TotalScore { get; set; }
    public string Grade { get; set; } = string.Empty;
    public string Interpretation { get; set; } = string.Empty;
}

#endregion