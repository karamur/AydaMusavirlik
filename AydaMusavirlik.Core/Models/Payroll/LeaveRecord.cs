namespace AydaMusavirlik.Core.Models.Payroll;

/// <summary>
/// Izin kaydi (Eski model - geriye uyumluluk icin)
/// </summary>
public class LeaveRecord : Common.BaseEntity
{
    public int EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? Reason { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public virtual Employee Employee { get; set; } = null!;
}

/// <summary>
/// Bordro parametreleri (SGK, vergi oranlari vb.)
/// </summary>
public class PayrollParameter : Common.BaseEntity
{
    public int Year { get; set; }
    public decimal MinimumWage { get; set; }
    public decimal SgkEmployeeRate { get; set; } = 14m;
    public decimal SgkEmployerRate { get; set; } = 20.5m;
    public decimal UnemploymentEmployeeRate { get; set; } = 1m;
    public decimal UnemploymentEmployerRate { get; set; } = 2m;
    public decimal StampTaxRate { get; set; } = 0.759m;
    public decimal SgkCeiling { get; set; }
    
    public string? IncomeTaxBrackets { get; set; }
}
