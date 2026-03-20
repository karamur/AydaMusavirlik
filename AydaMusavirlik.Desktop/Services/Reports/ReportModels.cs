namespace AydaMusavirlik.Desktop.Services;

/// <summary>
/// Bilanço raporu modeli
/// </summary>
public class BalanceSheetReport
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string Period { get; set; } = string.Empty;

    // VARLIKLAR (AKTÝF)
    public BalanceSheetSection DonenVarliklar { get; set; } = new() { Title = "I. DÖNEN VARLIKLAR" };
    public BalanceSheetSection DuranVarliklar { get; set; } = new() { Title = "II. DURAN VARLIKLAR" };

    // KAYNAKLAR (PASÝF)
    public BalanceSheetSection KisaVadeliYabanciKaynaklar { get; set; } = new() { Title = "I. KISA VADELÝ YABANCI KAYNAKLAR" };
    public BalanceSheetSection UzunVadeliYabanciKaynaklar { get; set; } = new() { Title = "II. UZUN VADELÝ YABANCI KAYNAKLAR" };
    public BalanceSheetSection OzKaynaklar { get; set; } = new() { Title = "III. ÖZ KAYNAKLAR" };

    public decimal ToplamAktif => DonenVarliklar.Total + DuranVarliklar.Total;
    public decimal ToplamPasif => KisaVadeliYabanciKaynaklar.Total + UzunVadeliYabanciKaynaklar.Total + OzKaynaklar.Total;
    public bool IsBalanced => Math.Abs(ToplamAktif - ToplamPasif) < 0.01m;
}

public class BalanceSheetSection
{
    public string Title { get; set; } = string.Empty;
    public List<BalanceSheetItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Amount);
}

public class BalanceSheetItem
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? PreviousPeriodAmount { get; set; }
    public int Level { get; set; }
    public bool IsHeader { get; set; }
}

/// <summary>
/// Gelir Tablosu raporu modeli
/// </summary>
public class IncomeStatementReport
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Period { get; set; } = string.Empty;

    public List<IncomeStatementItem> Items { get; set; } = new();

    // Özet Hesaplamalar
    public decimal BrutSatislar => GetItemAmount("60");
    public decimal SatisIndirimleri => GetItemAmount("61");
    public decimal NetSatislar => BrutSatislar - SatisIndirimleri;
    public decimal SatislarinMaliyeti => GetItemAmount("62");
    public decimal BrutSatisKari => NetSatislar - SatislarinMaliyeti;
    public decimal FaaliyetGiderleri => GetItemAmount("63") + GetItemAmount("64") + GetItemAmount("65");
    public decimal FaaliyetKari => BrutSatisKari - FaaliyetGiderleri;
    public decimal DigerGelirler => GetItemAmount("64");
    public decimal DigerGiderler => GetItemAmount("65") + GetItemAmount("66");
    public decimal FinansmanGiderleri => GetItemAmount("67");
    public decimal DonemKariZarari => FaaliyetKari + DigerGelirler - DigerGiderler - FinansmanGiderleri;

    private decimal GetItemAmount(string code) => Items.Where(i => i.Code.StartsWith(code)).Sum(i => i.Amount);
}

public class IncomeStatementItem
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? PreviousPeriodAmount { get; set; }
    public int Level { get; set; }
    public bool IsHeader { get; set; }
    public bool IsTotal { get; set; }
}

/// <summary>
/// Nakit Akýþ Tablosu
/// </summary>
public class CashFlowReport
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public CashFlowSection IsletmeFaaliyetleri { get; set; } = new() { Title = "A. ÝÞLETME FAALÝYETLERÝNDEN NAKÝT AKIÞLARI" };
    public CashFlowSection YatirimFaaliyetleri { get; set; } = new() { Title = "B. YATIRIM FAALÝYETLERÝNDEN NAKÝT AKIÞLARI" };
    public CashFlowSection FinansmanFaaliyetleri { get; set; } = new() { Title = "C. FÝNANSMAN FAALÝYETLERÝNDEN NAKÝT AKIÞLARI" };

    public decimal NakitDegisimi => IsletmeFaaliyetleri.Total + YatirimFaaliyetleri.Total + FinansmanFaaliyetleri.Total;
    public decimal DonemBasiNakit { get; set; }
    public decimal DonemSonuNakit => DonemBasiNakit + NakitDegisimi;
}

public class CashFlowSection
{
    public string Title { get; set; } = string.Empty;
    public List<CashFlowItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Amount);
}

public class CashFlowItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsSubtotal { get; set; }
}