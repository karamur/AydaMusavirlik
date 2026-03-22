using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Core.Models.Payroll;

/// <summary>
/// Çalýþan bilgileri
/// </summary>
public class Employee : SoftDeleteEntity
{
    public int CompanyId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public int NumberOfChildren { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IbanNumber { get; set; }
    public string? SgkNumber { get; set; }

    // SGK Bilgileri
    public int? SgkBelgeTuruId { get; set; }                  // SGK Belge Turu (01, 02, vb.)
    public string? SgkIsyeriSicilNo { get; set; }             // Isyeri sicil no
    public DateTime? SgkIseGirisTarihi { get; set; }          // SGK ise giris

    // Ýþ bilgileri
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public WorkType WorkType { get; set; }

    // Ücret bilgileri
    public decimal GrossSalary { get; set; }
    public SalaryType SalaryType { get; set; }
    public bool IsMinimumWageExempt { get; set; }             // Asgari ucret istisnasi
    public bool IsDisabled { get; set; }
    public int? DisabilityDegree { get; set; }

    // Ek Bilgiler
    public bool SendikaUyesi { get; set; }
    public decimal? SendikaAidatOrani { get; set; }
    public bool IcraKesintisiVar { get; set; }
    public decimal? IcraKesintisiTutari { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    // Navigation
    public virtual Company Company { get; set; } = null!;
    public virtual SgkBelgeTuru? SgkBelgeTuru { get; set; }
    public virtual ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();
    public virtual ICollection<LeaveRecord> LeaveRecords { get; set; } = new List<LeaveRecord>();
    public virtual ICollection<Puantaj> Puantajlar { get; set; } = new List<Puantaj>();
}

public enum Gender
{
    Male = 1,
    Female = 2
}

public enum MaritalStatus
{
    Single = 1,       // Bekar
    Married = 2,      // Evli
    Divorced = 3,     // Bosanmis
    Widowed = 4       // Dul
}

public enum EmploymentType
{
    Permanent = 1,        // Daimi
    Contract = 2,         // Sozlesmeli
    PartTime = 3,         // Yari zamanli
    Intern = 4            // Stajyer
}

public enum WorkType
{
    Office = 1,           // Ofis
    Field = 2,            // Saha
    Remote = 3,           // Uzaktan
    Hybrid = 4            // Hibrit
}

public enum SalaryType
{
    Monthly = 1,          // Aylik
    Daily = 2,            // Gunluk
    Hourly = 3            // Saatlik
}
