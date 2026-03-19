using AydaMusavirlik.Models.Common;

namespace AydaMusavirlik.Models.Accounting;

/// <summary>
/// Muhasebe kaydý / Fiþ
/// </summary>
public class AccountingRecord : SoftDeleteEntity
{
    public int CompanyId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime DocumentDate { get; set; }
    public RecordType RecordType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Draft;
    public string? Notes { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<AccountingEntry> Entries { get; set; } = new List<AccountingEntry>();
}

public enum RecordType
{
    MahsupFisi = 1,          // Mahsup Fiþi
    TahsilatFisi = 2,        // Tahsilat Fiþi
    OdemeFisi = 3,           // Ödeme Fiþi
    AcilisFisi = 4,          // Açýlýþ Fiþi
    KapanisFisi = 5,         // Kapanýþ Fiþi
    SatisFaturasi = 6,       // Satýþ Faturasý
    AlisFaturasi = 7,        // Alýþ Faturasý
    DekontFisi = 8           // Dekont Fiþi
}

public enum RecordStatus
{
    Draft = 1,       // Taslak
    Approved = 2,    // Onaylandý
    Posted = 3,      // Deftere Ýþlendi
    Cancelled = 4    // Ýptal
}
