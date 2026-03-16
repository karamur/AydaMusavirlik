namespace AydaMusavirlik.Core.Models.Accounting;

/// <summary>
/// Dönem (Mali Yýl) bilgisi
/// </summary>
public class FiscalPeriod : Common.BaseEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PeriodStatus Status { get; set; } = PeriodStatus.Open;
    public DateTime? ClosedAt { get; set; }
    public string? ClosedBy { get; set; }
}

public enum PeriodStatus
{
    Open = 1,        // Açýk
    Closed = 2,      // Kapalý
    Locked = 3       // Kilitli
}

/// <summary>
/// Mali tablo þablonu
/// </summary>
public class FinancialStatement : Common.BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public StatementType Type { get; set; }
    public string? Template { get; set; }   // JSON þablon
    public bool IsDefault { get; set; }
}

public enum StatementType
{
    Bilanco = 1,                    // Bilanço
    GelirTablosu = 2,               // Gelir Tablosu
    NakitAkis = 3,                  // Nakit Akýþ Tablosu
    OzkaynakDegisim = 4,            // Özkaynak Deðiþim Tablosu
    Mizan = 5                       // Mizan
}
