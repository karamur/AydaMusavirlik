namespace AydaMusavirlik.Core.Models.CompanyFormation;

/// <summary>
/// Ana sözleþme
/// </summary>
public class ArticlesOfAssociation : Common.BaseEntity
{
    public int ApplicationId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyNameEnglish { get; set; }
    public Common.CompanyType CompanyType { get; set; }
    
    // Merkez bilgileri
    public string HeadquartersCity { get; set; } = string.Empty;
    public string? HeadquartersDistrict { get; set; }
    public string HeadquartersAddress { get; set; } = string.Empty;
    
    // Süre ve sermaye
    public int? DurationYears { get; set; }           // Süre (yýl), null = süresiz
    public decimal Capital { get; set; }
    public int? TotalShares { get; set; }             // Toplam pay sayýsý (A.Þ. için)
    public decimal? ShareNominalValue { get; set; }   // Pay nominal deðeri
    
    // Faaliyet konularý
    public string BusinessActivities { get; set; } = string.Empty;
    
    // Yönetim
    public string? ManagementStructure { get; set; }  // Yönetim yapýsý (JSON)
    public string? GeneralAssemblyRules { get; set; } // Genel kurul kurallarý
    public string? ProfitDistribution { get; set; }   // Kar daðýtým esaslarý
    
    // Ek maddeler
    public string? AdditionalArticles { get; set; }
    
    // Durum
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? NotaryInfo { get; set; }
    public string? NotaryDate { get; set; }
    
    // Oluþturulan belge
    public string? GeneratedDocument { get; set; }    // HTML/PDF içeriði
    public DateTime? GeneratedAt { get; set; }

    // Navigation
    public virtual CompanyFormationApplication Application { get; set; } = null!;
}

public enum ArticleStatus
{
    Draft = 1,           // Taslak
    Reviewing = 2,       // Ýnceleniyor
    Approved = 3,        // Onaylandý
    NotarySigned = 4,    // Noter imzalý
    Registered = 5       // Tescil edildi
}

/// <summary>
/// Baþvuru evraklarý
/// </summary>
public class ApplicationDocument : Common.BaseEntity
{
    public int ApplicationId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public string? Notes { get; set; }

    // Navigation
    public virtual CompanyFormationApplication Application { get; set; } = null!;
}

public enum DocumentType
{
    KimlikFotokopisi = 1,        // Kimlik fotokopisi
    IkametgahBelgesi = 2,        // Ýkametgah belgesi
    ImzaSirkuleri = 3,           // Ýmza sirküleri
    TicariSicilGazetesi = 4,     // Ticaret sicil gazetesi
    VergiLevhasi = 5,            // Vergi levhasý
    FaaliyetBelgesi = 6,         // Faaliyet belgesi
    VekaletName = 7,             // Vekaletname
    AnaSozlesme = 8,             // Ana sözleþme
    KurulKarari = 9,             // Yönetim/Genel kurul kararý
    Diger = 99                   // Diðer
}

public enum DocumentStatus
{
    Pending = 1,      // Bekliyor
    Received = 2,     // Alýndý
    Approved = 3,     // Onaylandý
    Rejected = 4      // Reddedildi
}
