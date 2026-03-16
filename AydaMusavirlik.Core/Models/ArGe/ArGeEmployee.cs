using AydaMusavirlik.Core.Models.Payroll;

namespace AydaMusavirlik.Core.Models.ArGe;

/// <summary>
/// AR-GE personeli atama
/// </summary>
public class ArGeEmployee : Common.BaseEntity
{
    public int ArGeProjectId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime AssignmentDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal AllocationPercentage { get; set; } = 100;  // Projeye ayrýlan süre %
    public ArGeRole Role { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public virtual ArGeProject Project { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}

public enum ArGeRole
{
    ProjectManager = 1,    // Proje Yöneticisi
    Researcher = 2,        // Araþtýrmacý
    Technician = 3,        // Teknisyen
    SupportStaff = 4       // Destek Personeli
}

/// <summary>
/// AR-GE harcamalarý
/// </summary>
public class ArGeExpense : Common.BaseEntity
{
    public int ArGeProjectId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ArGeExpenseType ExpenseType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? DocumentNumber { get; set; }
    public string? VendorName { get; set; }
    public bool IsEligibleForIncentive { get; set; } = true;  // Teþvik kapsamýnda mý?
    public string? Notes { get; set; }

    // Navigation
    public virtual ArGeProject Project { get; set; } = null!;
}

public enum ArGeExpenseType
{
    PersonelGideri = 1,          // Personel gideri
    MalzemeGideri = 2,           // Malzeme gideri
    HizmetAlimi = 3,             // Hizmet alýmý
    AmortismanGideri = 4,        // Amortisman gideri
    GeliþtirmeGideri = 5,        // Genel gider
    SeyahatGideri = 6,           // Seyahat gideri
    PatentGideri = 7,            // Patent/Lisans gideri
    DanismanlikGideri = 8        // Danýþmanlýk gideri
}
