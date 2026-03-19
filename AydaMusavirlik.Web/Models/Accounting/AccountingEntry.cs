using AydaMusavirlik.Models.Common;

namespace AydaMusavirlik.Models.Accounting;

/// <summary>
/// Muhasebe kaydý satýrý (Yevmiye maddesi)
/// </summary>
public class AccountingEntry : BaseEntity
{
    public int AccountingRecordId { get; set; }
    public int AccountId { get; set; }
    public int LineNumber { get; set; }
    public decimal Debit { get; set; }                  // Borç
    public decimal Credit { get; set; }                 // Alacak
    public string? Description { get; set; }
    public string? CostCenterCode { get; set; }         // Masraf merkezi
    public string? ProjectCode { get; set; }            // Proje kodu

    // Navigation
    public virtual AccountingRecord AccountingRecord { get; set; } = null!;
    public virtual Account Account { get; set; } = null!;
}
