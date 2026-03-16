namespace AydaMusavirlik.Core.Models.Payroll;

/// <summary>
/// Bordro kaydý
/// </summary>
public class PayrollRecord : Common.BaseEntity
{
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime PaymentDate { get; set; }

    // Çalýþma süreleri
    public int WorkingDays { get; set; }
    public int OvertimeHours { get; set; }
    public int WeekendOvertimeHours { get; set; }
    public int HolidayOvertimeHours { get; set; }

    // Brüt kazançlar
    public decimal GrossSalary { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal BonusPay { get; set; }
    public decimal OtherEarnings { get; set; }
    public decimal TotalGross { get; set; }

    // SGK kesintileri
    public decimal SgkEmployeeShare { get; set; }           // SGK iþçi payý (%14)
    public decimal SgkUnemploymentEmployee { get; set; }    // Ýþsizlik sigortasý iþçi (%1)
    public decimal SgkEmployerShare { get; set; }           // SGK iþveren payý (%20.5)
    public decimal SgkUnemploymentEmployer { get; set; }    // Ýþsizlik sigortasý iþveren (%2)

    // Vergi kesintileri
    public decimal IncomeTaxBase { get; set; }              // Gelir vergisi matrahý
    public decimal IncomeTax { get; set; }                  // Gelir vergisi
    public decimal StampTax { get; set; }                   // Damga vergisi
    public decimal MinimumWageExemption { get; set; }       // Asgari ücret istisnasý

    // Net ücret
    public decimal TotalDeductions { get; set; }            // Toplam kesinti
    public decimal NetSalary { get; set; }                  // Net maaþ

    // Maliyet
    public decimal TotalEmployerCost { get; set; }          // Toplam iþveren maliyeti

    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public string? Notes { get; set; }

    // Navigation
    public virtual Employee Employee { get; set; } = null!;
}

public enum PayrollStatus
{
    Draft = 1,
    Calculated = 2,
    Approved = 3,
    Paid = 4,
    Cancelled = 5
}
