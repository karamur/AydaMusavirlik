using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Core.Models.ArGe;

/// <summary>
/// AR-GE Projesi
/// </summary>
public class ArGeProject : SoftDeleteEntity
{
    public int CompanyId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public ArGeProjectType ProjectType { get; set; }
    public ArGeProjectStatus Status { get; set; } = ArGeProjectStatus.Planning;
    
    // Bütçe
    public decimal PlannedBudget { get; set; }
    public decimal ActualCost { get; set; }
    
    // Destek bilgileri
    public bool HasIncentive { get; set; }
    public IncentiveType? IncentiveType { get; set; }
    public string? IncentiveCertificateNo { get; set; }
    public DateTime? IncentiveStartDate { get; set; }
    public DateTime? IncentiveEndDate { get; set; }
    
    public string? ResponsiblePerson { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<ArGeEmployee> ArGeEmployees { get; set; } = new List<ArGeEmployee>();
    public virtual ICollection<ArGeExpense> Expenses { get; set; } = new List<ArGeExpense>();
}

public enum ArGeProjectType
{
    ArGe = 1,              // Araþtýrma-Geliþtirme
    Tasarim = 2,           // Tasarým
    YazilimGelistirme = 3  // Yazýlým Geliþtirme
}

public enum ArGeProjectStatus
{
    Planning = 1,      // Planlama
    Active = 2,        // Aktif
    OnHold = 3,        // Beklemede
    Completed = 4,     // Tamamlandý
    Cancelled = 5      // Ýptal
}

public enum IncentiveType
{
    TUBITAKDestegi = 1,        // TÜBÝTAK Desteði
    KOSGEBDestegi = 2,         // KOSGEB Desteði
    TeknokentIndirimi = 3,     // Teknokent Ýndirimi
    ArGeMerkeziIndirimi = 4,   // AR-GE Merkezi Ýndirimi
    TasarimMerkeziIndirimi = 5 // Tasarým Merkezi Ýndirimi
}
