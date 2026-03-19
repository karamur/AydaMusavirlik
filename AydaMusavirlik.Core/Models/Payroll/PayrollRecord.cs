
using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Core.Models.Payroll;

/// <summary>
/// Bordro kaydi
/// </summary>
public class PayrollRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime PaymentDate { get; set; }

    // Calisma sureleri
    public int WorkingDays { get; set; }
    public int OvertimeHours { get; set; }
    public int WeekendOvertimeHours { get; set; }
    public int HolidayOvertimeHours { get; set; }

    // Brut kazanclar
    public decimal GrossSalary { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal BonusPay { get; set; }
    public decimal OtherEarnings { get; set; }
    public decimal TotalGross { get; set; }

    // SGK kesintileri
    public decimal SgkWorkerDeduction { get; set; }         // SGK isci payi (%14)
    public decimal SgkUnemploymentWorker { get; set; }      // Issizlik sigortasi isci (%1)
    public decimal SgkEmployerCost { get; set; }            // SGK isveren payi (%20.5)
    public decimal SgkUnemploymentEmployer { get; set; }    // Issizlik sigortasi isveren (%2)

    // Vergi kesintileri
    public decimal IncomeTaxBase { get; set; }              // Gelir vergisi matrahi
    public decimal IncomeTax { get; set; }                  // Gelir vergisi
    public decimal StampTax { get; set; }                   // Damga vergisi
    public decimal MinimumWageExemption { get; set; }       // Asgari ucret istisnasi

    // Net ucret
    public decimal TotalDeductions { get; set; }            // Toplam kesinti
    public decimal NetSalary { get; set; }                  // Net maas

    // Maliyet
    public decimal TotalEmployerCost { get; set; }          // Toplam isveren maliyeti

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
