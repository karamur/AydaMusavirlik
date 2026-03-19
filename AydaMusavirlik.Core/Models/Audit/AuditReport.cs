using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Core.Models.Audit;

/// <summary>
/// Denetim Raporu
/// </summary>
public class AuditReport : SoftDeleteEntity
{
    public int CompanyId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public AuditType AuditType { get; set; }
    public DateTime AuditStartDate { get; set; }
    public DateTime? AuditEndDate { get; set; }
    public DateTime ReportDate { get; set; }
    public string? Auditor { get; set; }
    public string? Findings { get; set; }
    public string? Recommendations { get; set; }
    public AuditStatus Status { get; set; } = AuditStatus.Draft;
    public string? Notes { get; set; }

    // Navigation
    public virtual Company Company { get; set; } = null!;
}

public enum AuditType
{
    BagimsizDenetim = 1,     // Baðýmsýz Denetim
    IcDenetim = 2,           // Ýç Denetim
    VergiDenetimi = 3,       // Vergi Denetimi
    OzelDenetim = 4          // Özel Denetim
}

public enum AuditStatus
{
    Draft = 1,       // Taslak
    InProgress = 2,  // Devam Ediyor
    Completed = 3,   // Tamamlandý
    Approved = 4     // Onaylandý
}
