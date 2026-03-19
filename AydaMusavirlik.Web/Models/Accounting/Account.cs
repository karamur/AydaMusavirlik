using AydaMusavirlik.Models.Common;

namespace AydaMusavirlik.Models.Accounting;

/// <summary>
/// Muhasebe hesabý (Hesap Planý)
/// </summary>
public class Account : SoftDeleteEntity
{
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;           // Hesap kodu (100, 101, 102...)
    public string Name { get; set; } = string.Empty;           // Hesap adý
    public int? ParentId { get; set; }                         // Üst hesap
    public AccountType AccountType { get; set; }
    public AccountNature Nature { get; set; }                   // Borç/Alacak yapýsý
    public int Level { get; set; }                              // Hesap seviyesi (1,2,3...)
    public bool IsHeader { get; set; }                          // Grup hesabý mý?
    public bool AllowPosting { get; set; } = true;              // Kayýt yapýlabilir mi?
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual Account? Parent { get; set; }
    public virtual ICollection<Account> Children { get; set; } = new List<Account>();
    public virtual ICollection<AccountingEntry> Entries { get; set; } = new List<AccountingEntry>();
}

public enum AccountType
{
    Aktif = 1,           // Varlýklar (1-2)
    Pasif = 2,           // Kaynaklar (3-4-5)
    Gelir = 3,           // Gelirler (6)
    Gider = 4,           // Giderler (7)
    Maliyet = 5,         // Maliyet (7)
    Nazim = 6            // Nazým Hesaplar (9)
}

public enum AccountNature
{
    Debit = 1,           // Borç
    Credit = 2           // Alacak
}
