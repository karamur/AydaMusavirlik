namespace AydaMusavirlik.Core.Models.Common;

/// <summary>
/// Müþteri/Firma bilgileri
/// </summary>
public class Company : SoftDeleteEntity
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? TradeRegistryNumber { get; set; }
    public string? MersisNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public CompanyType CompanyType { get; set; }
    public DateTime? FoundationDate { get; set; }
    public decimal? Capital { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public virtual ICollection<AccountingRecord> AccountingRecords { get; set; } = new List<AccountingRecord>();
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<ArGeProject> ArGeProjects { get; set; } = new List<ArGeProject>();
    public virtual ICollection<AuditReport> AuditReports { get; set; } = new List<AuditReport>();
}

public enum CompanyType
{
    LimitedSirketi = 1,      // Limited Þirketi
    AnonimSirket = 2,        // Anonim Þirket
    SahisFirmasi = 3,        // Þahýs Firmasý
    KollektifSirket = 4,     // Kollektif Þirket
    KomanditSirket = 5,      // Komandit Þirket
    Kooperatif = 6,          // Kooperatif
    Dernek = 7,              // Dernek
    Vakif = 8                // Vakýf
}
