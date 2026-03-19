using AydaMusavirlik.Models.Licensing;
using AydaMusavirlik.Modules;

namespace AydaMusavirlik.Services;

/// <summary>
/// Lisans yönetim servisi
/// </summary>
public class LicenseService
{
    private readonly ILogger<LicenseService> _logger;
    private readonly IConfiguration _configuration;
    private LicenseInfo _currentLicense;

    public LicenseService(
        ILogger<LicenseService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        LoadLicense();
    }

    private void LoadLicense()
    {
        var devMode = _configuration.GetValue<bool>("License:DevMode", true);

        // Development/Demo modunda tüm modüllere eriþim
        _currentLicense = new LicenseInfo
        {
            LicenseId = "DEV-LICENSE",
            CustomerName = "Development",
            CompanyName = "AYDA Development",
            Type = LicenseType.Enterprise,
            LicensedModules = devMode ? ["*"] : ModuleConstants.AllModuleIds.ToList(),
            MaxUsers = 100,
            StartDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            IsActive = true
        };

        _logger.LogInformation("Lisans yüklendi. DevMode: {DevMode}, Type: {Type}", 
            devMode, _currentLicense.Type);
    }

    /// <summary>
    /// Mevcut lisansý döndür
    /// </summary>
    public LicenseInfo GetCurrentLicense() => _currentLicense;

    /// <summary>
    /// Modül lisanslý mý kontrol et
    /// </summary>
    public bool IsModuleLicensed(string moduleId)
    {
        if (_currentLicense == null || !_currentLicense.IsValid())
            return false;

        return _currentLicense.HasModule(moduleId);
    }

    /// <summary>
    /// Kalan gün sayýsý
    /// </summary>
    public int GetRemainingDays()
    {
        if (_currentLicense?.ExpirationDate == null)
            return int.MaxValue;

        var remaining = (_currentLicense.ExpirationDate.Value - DateTime.UtcNow).Days;
        return Math.Max(0, remaining);
    }

    /// <summary>
    /// Modül lisansý getir
    /// </summary>
    public ModuleLicense? GetModuleLicense(string moduleId)
    {
        if (_currentLicense == null)
            return null;

        return new ModuleLicense
        {
            ModuleId = moduleId,
            LicenseKey = _currentLicense.LicenseKey,
            CustomerName = _currentLicense.CustomerName,
            ExpirationDate = _currentLicense.ExpirationDate,
            Type = (ModuleLicenseType)(int)_currentLicense.Type,
            IsValid = _currentLicense.IsValid() && _currentLicense.HasModule(moduleId)
        };
    }

    /// <summary>
    /// Tüm lisanslý modülleri getir
    /// </summary>
    public List<string> GetLicensedModules()
    {
        if (_currentLicense == null)
            return new List<string>();

        if (_currentLicense.LicensedModules.Contains("*"))
            return ModuleConstants.AllModuleIds.ToList();

        return _currentLicense.LicensedModules;
    }
}
