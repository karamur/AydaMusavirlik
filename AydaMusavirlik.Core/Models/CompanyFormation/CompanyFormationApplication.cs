using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Core.Models.CompanyFormation;

/// <summary>
/// Þirket kuruluþ baþvurusu
/// </summary>
public class CompanyFormationApplication : SoftDeleteEntity
{
    public string ApplicationNumber { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public string ProposedCompanyName { get; set; } = string.Empty;
    public string? AlternativeName1 { get; set; }
    public string? AlternativeName2 { get; set; }
    public CompanyType CompanyType { get; set; }
    public decimal Capital { get; set; }
    public string? HeadquartersCity { get; set; }
    public string? HeadquartersDistrict { get; set; }
    public string? HeadquartersAddress { get; set; }
    public string? BusinessActivities { get; set; }             // Faaliyet konularý
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;
    public string? Notes { get; set; }

    // Baþvuru sahibi bilgileri
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantPhone { get; set; } = string.Empty;
    public string? ApplicantEmail { get; set; }

    // Navigation
    public virtual ICollection<Shareholder> Shareholders { get; set; } = new List<Shareholder>();
    public virtual ICollection<ApplicationDocument> Documents { get; set; } = new List<ApplicationDocument>();
    public virtual ArticlesOfAssociation? ArticlesOfAssociation { get; set; }
}

public enum ApplicationStatus
{
    Draft = 1,               // Taslak
    PendingDocuments = 2,    // Evrak bekleniyor
    NameReserved = 3,        // Unvan tescil edildi
    NotaryApproved = 4,      // Noter onaylandý
    TradeRegistered = 5,     // Ticaret sicil tescili
    TaxRegistered = 6,       // Vergi dairesi kaydý
    Completed = 7,           // Tamamlandý
    Cancelled = 8            // Ýptal
}

/// <summary>
/// Ortak/Pay sahibi bilgileri
/// </summary>
public class Shareholder : BaseEntity
{
    public int ApplicationId { get; set; }
    public ShareholderType Type { get; set; }
    
    // Gerçek kiþi için
    public string? TcKimlikNo { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FatherName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? BirthPlace { get; set; }
    public string? Nationality { get; set; }

    // Tüzel kiþi için
    public string? CompanyName { get; set; }
    public string? TaxNumber { get; set; }
    public string? TradeRegistryNumber { get; set; }
    public string? RepresentativeName { get; set; }

    // Ortak bilgiler
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public decimal ShareAmount { get; set; }          // Pay tutarý
    public decimal SharePercentage { get; set; }      // Pay oraný %
    public bool IsFounder { get; set; } = true;       // Kurucu ortak mý?
    public bool IsDirector { get; set; }              // Müdür mü?
    public bool HasSignatureAuthority { get; set; }   // Ýmza yetkisi var mý?

    public string FullName => Type == ShareholderType.Individual 
        ? $"{FirstName} {LastName}" 
        : CompanyName ?? string.Empty;

    // Navigation
    public virtual CompanyFormationApplication Application { get; set; } = null!;
}

public enum ShareholderType
{
    Individual = 1,    // Gerçek kiþi
    Corporate = 2      // Tüzel kiþi
}
