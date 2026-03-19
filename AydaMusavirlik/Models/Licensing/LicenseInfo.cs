namespace AydaMusavirlik.Models.Licensing;

/// <summary>
/// Lisans bilgisi modeli
/// </summary>
public class LicenseInfo
{
    /// <summary>
    /// Benzersiz lisans ID
    /// </summary>
    public string LicenseId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Lisans anahtarý (þifrelenmiþ)
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;

    /// <summary>
    /// Müþteri adý
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Müþteri email
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Þirket adý
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Vergi numarasý
    /// </summary>
    public string TaxNumber { get; set; } = string.Empty;

    /// <summary>
    /// Lisans tipi
    /// </summary>
    public LicenseType Type { get; set; } = LicenseType.Trial;

    /// <summary>
    /// Lisanslý modül ID'leri
    /// </summary>
    public List<string> LicensedModules { get; set; } = [];

    /// <summary>
    /// Baþlangýç tarihi
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Bitiþ tarihi
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Maksimum kullanýcý sayýsý
    /// </summary>
    public int MaxUsers { get; set; } = 1;

    /// <summary>
    /// Makine kodu (donaným bazlý)
    /// </summary>
    public string? MachineCode { get; set; }

    /// <summary>
    /// Lisans aktif mi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Dijital imza (RSA)
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Oluþturma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Lisans geçerli mi kontrol et
    /// </summary>
    public bool IsValid()
    {
        if (!IsActive) return false;
        if (ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow) return false;
        return true;
    }

    /// <summary>
    /// Belirli bir modül lisanslý mý
    /// </summary>
    public bool HasModule(string moduleId)
    {
        return LicensedModules.Contains(moduleId) || 
               LicensedModules.Contains("*"); // * = tüm modüller
    }
}

/// <summary>
/// Lisans tipi
/// </summary>
public enum LicenseType
{
    Trial = 0,
    Basic = 1,
    Standard = 2,
    Professional = 3,
    Enterprise = 4,
    Unlimited = 5
}

/// <summary>
/// Lisans tipi özelliklerini döndürür
/// </summary>
public static class LicenseTypeExtensions
{
    public static string GetDisplayName(this LicenseType type) => type switch
    {
        LicenseType.Trial => "Deneme Lisansý",
        LicenseType.Basic => "Temel Lisans",
        LicenseType.Standard => "Standart Lisans",
        LicenseType.Professional => "Profesyonel Lisans",
        LicenseType.Enterprise => "Kurumsal Lisans",
        LicenseType.Unlimited => "Unlimited Lisans",
        _ => "Bilinmeyen"
    };

    public static int GetMaxModules(this LicenseType type) => type switch
    {
        LicenseType.Trial => 1,
        LicenseType.Basic => 2,
        LicenseType.Standard => 5,
        LicenseType.Professional => 10,
        LicenseType.Enterprise => int.MaxValue,
        LicenseType.Unlimited => int.MaxValue,
        _ => 0
    };

    public static int GetDefaultUsers(this LicenseType type) => type switch
    {
        LicenseType.Trial => 1,
        LicenseType.Basic => 2,
        LicenseType.Standard => 5,
        LicenseType.Professional => 20,
        LicenseType.Enterprise => 100,
        LicenseType.Unlimited => int.MaxValue,
        _ => 1
    };
}
