namespace AydaMusavirlik.Modules;

/// <summary>
/// Modül sabitleri - Tüm modül ID'leri burada tanýmlý
/// </summary>
public static class ModuleConstants
{
    // Modül ID'leri
    public const string MODULE_MUHASEBE = "AYDA.MUHASEBE";
    public const string MODULE_BORDRO = "AYDA.BORDRO";
    public const string MODULE_ARGE = "AYDA.ARGE";
    public const string MODULE_DENETIM = "AYDA.DENETIM";
    public const string MODULE_FINANSAL_ANALIZ = "AYDA.FINANSAL_ANALIZ";
    public const string MODULE_SIRKET_KURULUSU = "AYDA.SIRKET_KURULUSU";

    /// <summary>
    /// Tüm modül ID'lerini döndürür
    /// </summary>
    public static IReadOnlyList<string> AllModuleIds =>
    [
        MODULE_MUHASEBE,
        MODULE_BORDRO,
        MODULE_ARGE,
        MODULE_DENETIM,
        MODULE_FINANSAL_ANALIZ,
        MODULE_SIRKET_KURULUSU
    ];

    /// <summary>
    /// Modül adýný döndürür
    /// </summary>
    public static string GetModuleName(string moduleId) => moduleId switch
    {
        MODULE_MUHASEBE => "Muhasebe",
        MODULE_BORDRO => "Bordro",
        MODULE_ARGE => "AR-GE",
        MODULE_DENETIM => "Denetim",
        MODULE_FINANSAL_ANALIZ => "Finansal Analiz",
        MODULE_SIRKET_KURULUSU => "Þirket Kuruluþu",
        _ => moduleId.Replace("AYDA.", "")
    };
}

/// <summary>
/// Modül bilgisi
/// </summary>
public class ModuleInfo
{
    public string ModuleId { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "Circle";
    public string Version { get; set; } = "1.0.0";
    public bool IsEnabled { get; set; } = true;
    public int Order { get; set; }
    public List<ModuleMenuItem> MenuItems { get; set; } = new();
}

/// <summary>
/// Modül menü öðesi
/// </summary>
public class ModuleMenuItem
{
    public string Title { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Icon { get; set; } = "Circle";
    public int Order { get; set; }
    public List<ModuleMenuItem> Children { get; set; } = new();
}

/// <summary>
/// Modül lisans bilgisi
/// </summary>
public class ModuleLicense
{
    public string ModuleId { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
    public ModuleLicenseType Type { get; set; } = ModuleLicenseType.Trial;
    public bool IsValid { get; set; }
}

public enum ModuleLicenseType
{
    Trial = 0,
    Basic = 1,
    Standard = 2,
    Professional = 3,
    Enterprise = 4
}
