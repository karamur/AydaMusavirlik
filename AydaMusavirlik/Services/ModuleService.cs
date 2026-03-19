using AydaMusavirlik.Modules;

namespace AydaMusavirlik.Services;

/// <summary>
/// Modül yönetim servisi
/// </summary>
public class ModuleService
{
    private readonly ILogger<ModuleService> _logger;
    private readonly LicenseService _licenseService;
    private readonly List<ModuleInfo> _modules = new();

    public ModuleService(
        ILogger<ModuleService> logger,
        LicenseService licenseService)
    {
        _logger = logger;
        _licenseService = licenseService;
        InitializeModules();
    }

    private void InitializeModules()
    {
        // Muhasebe Modülü
        _modules.Add(new ModuleInfo
        {
            ModuleId = ModuleConstants.MODULE_MUHASEBE,
            ModuleName = "Muhasebe",
            Description = "Gelir/Gider takibi, hesap planý, mali tablolar",
            Icon = "Calculator",
            Version = "1.0.0",
            Order = 10,
            MenuItems = new List<ModuleMenuItem>
            {
                new() { Title = "Dashboard", Route = "/muhasebe", Icon = "Dashboard", Order = 1 },
                new() { Title = "Hesap Planý", Route = "/muhasebe/hesap-plani", Icon = "List", Order = 2 },
                new() { Title = "Fiþler", Route = "/muhasebe/fisler", Icon = "FileText", Order = 3,
                    Children = new List<ModuleMenuItem>
                    {
                        new() { Title = "Mahsup Fiþi", Route = "/muhasebe/fisler/mahsup", Icon = "FileText" },
                        new() { Title = "Tahsilat Fiþi", Route = "/muhasebe/fisler/tahsilat", Icon = "TrendingUp" },
                        new() { Title = "Tediye Fiþi", Route = "/muhasebe/fisler/tediye", Icon = "TrendingDown" }
                    }
                },
                new() { Title = "Raporlar", Route = "/muhasebe/raporlar", Icon = "BarChart", Order = 4 },
                new() { Title = "Mizan", Route = "/muhasebe/mizan", Icon = "PieChart", Order = 5 }
            }
        });

        // Bordro Modülü
        _modules.Add(new ModuleInfo
        {
            ModuleId = ModuleConstants.MODULE_BORDRO,
            ModuleName = "Bordro",
            Description = "Personel maaþ ve bordro yönetimi",
            Icon = "Users",
            Version = "1.0.0",
            Order = 20,
            MenuItems = new List<ModuleMenuItem>
            {
                new() { Title = "Dashboard", Route = "/bordro", Icon = "Dashboard", Order = 1 },
                new() { Title = "Personeller", Route = "/bordro/personeller", Icon = "Users", Order = 2 },
                new() { Title = "Bordro Hesaplama", Route = "/bordro/hesaplama", Icon = "Calculator", Order = 3 },
                new() { Title = "Ýzin Takibi", Route = "/bordro/izinler", Icon = "Calendar", Order = 4 },
                new() { Title = "Raporlar", Route = "/bordro/raporlar", Icon = "BarChart", Order = 5 }
            }
        });

        // Denetim Modülü
        _modules.Add(new ModuleInfo
        {
            ModuleId = ModuleConstants.MODULE_DENETIM,
            ModuleName = "Denetim",
            Description = "Ýç denetim ve kontrol süreçleri",
            Icon = "Search",
            Version = "1.0.0",
            Order = 30,
            MenuItems = new List<ModuleMenuItem>
            {
                new() { Title = "Dashboard", Route = "/denetim", Icon = "Dashboard", Order = 1 },
                new() { Title = "Denetim Planý", Route = "/denetim/plan", Icon = "Calendar", Order = 2 },
                new() { Title = "Bulgular", Route = "/denetim/bulgular", Icon = "Warning", Order = 3 },
                new() { Title = "Raporlar", Route = "/denetim/raporlar", Icon = "BarChart", Order = 4 }
            }
        });

        // Finansal Analiz Modülü
        _modules.Add(new ModuleInfo
        {
            ModuleId = ModuleConstants.MODULE_FINANSAL_ANALIZ,
            ModuleName = "Finansal Analiz",
            Description = "Mali analiz ve raporlama",
            Icon = "BarChart",
            Version = "1.0.0",
            Order = 40,
            MenuItems = new List<ModuleMenuItem>
            {
                new() { Title = "Dashboard", Route = "/finansal-analiz", Icon = "Dashboard", Order = 1 },
                new() { Title = "Oran Analizi", Route = "/finansal-analiz/oranlar", Icon = "PieChart", Order = 2 },
                new() { Title = "Trend Analizi", Route = "/finansal-analiz/trend", Icon = "TrendingUp", Order = 3 },
                new() { Title = "Raporlar", Route = "/finansal-analiz/raporlar", Icon = "BarChart", Order = 4 }
            }
        });

        // AR-GE Modülü
        _modules.Add(new ModuleInfo
        {
            ModuleId = ModuleConstants.MODULE_ARGE,
            ModuleName = "AR-GE",
            Description = "AR-GE proje ve teþvik yönetimi",
            Icon = "Lightbulb",
            Version = "1.0.0",
            Order = 50,
            MenuItems = new List<ModuleMenuItem>
            {
                new() { Title = "Dashboard", Route = "/arge", Icon = "Dashboard", Order = 1 },
                new() { Title = "Projeler", Route = "/arge/projeler", Icon = "Folder", Order = 2 },
                new() { Title = "Personel", Route = "/arge/personel", Icon = "Users", Order = 3 },
                new() { Title = "Teþvikler", Route = "/arge/tesvikler", Icon = "Gift", Order = 4 }
            }
        });

        // Þirket Kuruluþu Modülü
        _modules.Add(new ModuleInfo
        {
            ModuleId = ModuleConstants.MODULE_SIRKET_KURULUSU,
            ModuleName = "Þirket Kuruluþu",
            Description = "Þirket kurulum süreçleri",
            Icon = "Building",
            Version = "1.0.0",
            Order = 60,
            MenuItems = new List<ModuleMenuItem>
            {
                new() { Title = "Dashboard", Route = "/sirket-kurulusu", Icon = "Dashboard", Order = 1 },
                new() { Title = "Baþvurular", Route = "/sirket-kurulusu/basvurular", Icon = "FileText", Order = 2 },
                new() { Title = "Þablonlar", Route = "/sirket-kurulusu/sablonlar", Icon = "Template", Order = 3 }
            }
        });

        _logger.LogInformation("Modüller yüklendi: {Count} modül", _modules.Count);
    }

    /// <summary>
    /// Tüm modülleri getir
    /// </summary>
    public IReadOnlyList<ModuleInfo> GetAllModules() => _modules;

    /// <summary>
    /// Lisanslý modülleri getir
    /// </summary>
    public IReadOnlyList<ModuleInfo> GetLicensedModules()
    {
        var licensedIds = _licenseService.GetLicensedModules();
        return _modules.Where(m => licensedIds.Contains(m.ModuleId) && m.IsEnabled)
            .OrderBy(m => m.Order)
            .ToList();
    }

    /// <summary>
    /// Modül getir
    /// </summary>
    public ModuleInfo? GetModule(string moduleId)
    {
        return _modules.FirstOrDefault(m => m.ModuleId == moduleId);
    }

    /// <summary>
    /// Modül aktif mi
    /// </summary>
    public bool IsModuleEnabled(string moduleId)
    {
        var module = GetModule(moduleId);
        return module != null && module.IsEnabled && _licenseService.IsModuleLicensed(moduleId);
    }
}
