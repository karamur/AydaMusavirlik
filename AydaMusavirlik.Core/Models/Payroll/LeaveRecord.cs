namespace AydaMusavirlik.Core.Models.Payroll;

/// <summary>
/// Ýzin kaydý
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

public enum LeaveType
{
    YillikIzin = 1,              // Yýllýk ücretli izin
    UcretsizIzin = 2,            // Ücretsiz izin
    HastalikIzni = 3,            // Hastalýk izni
    DoðumIzni = 4,               // Doðum izni
    BabalikIzni = 5,             // Babalýk izni
    EvlilikIzni = 6,             // Evlilik izni
    OlumIzni = 7,                // Ölüm izni
    MazeretIzni = 8              // Mazeret izni
}

public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

/// <summary>
/// Bordro parametreleri (SGK, vergi oranlarý vb.)
/// </summary>
public class PayrollParameter : Common.BaseEntity
{
    public int Year { get; set; }
    public decimal MinimumWage { get; set; }                    // Asgari ücret
    public decimal SgkEmployeeRate { get; set; } = 14m;         // SGK iþçi payý %
    public decimal SgkEmployerRate { get; set; } = 20.5m;       // SGK iþveren payý %
    public decimal UnemploymentEmployeeRate { get; set; } = 1m; // Ýþsizlik iþçi %
    public decimal UnemploymentEmployerRate { get; set; } = 2m; // Ýþsizlik iþveren %
    public decimal StampTaxRate { get; set; } = 0.759m;         // Damga vergisi %
    public decimal SgkCeiling { get; set; }                     // SGK tavan
    
    // Gelir vergisi dilimleri (JSON olarak saklanabilir)
    public string? IncomeTaxBrackets { get; set; }
}
