using System.IO;
using System.Text.Json;

namespace AydaMusavirlik.Desktop.Services;

/// <summary>
/// Ayarlar servisi interface
/// </summary>
public interface ISettingsService
{
    AppSettings Settings { get; }
    Task<bool> SaveSettingsAsync();
    Task<bool> UpdateDatabaseSettingsAsync(DesktopDatabaseSettings settings);
    Task LoadSettingsAsync();
}

/// <summary>
/// Uygulama ayarlari
/// </summary>
public class AppSettings
{
    public DesktopDatabaseSettings Database { get; set; } = new();
    public GeneralSettings General { get; set; } = new();
}

/// <summary>
/// Veritabani ayarlari (Desktop)
/// </summary>
public class DesktopDatabaseSettings
{
    public string Provider { get; set; } = "SQLite";
    public string SqliteFilePath { get; set; } = "AydaMusavirlik.db";
    public string SqlServerHost { get; set; } = "localhost";
    public int SqlServerPort { get; set; } = 1433;
    public string SqlServerDatabase { get; set; } = "AydaMusavirlik";
    public bool SqlServerTrustedConnection { get; set; } = true;
    public string SqlServerUsername { get; set; } = "";
    public string SqlServerPassword { get; set; } = "";
    public string PostgresHost { get; set; } = "localhost";
    public int PostgresPort { get; set; } = 5432;
    public string PostgresDatabase { get; set; } = "aydamusavirlik";
    public string PostgresUsername { get; set; } = "postgres";
    public string PostgresPassword { get; set; } = "";
}

/// <summary>
/// Genel ayarlar
/// </summary>
public class GeneralSettings
{
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "tr-TR";
    public bool AutoBackup { get; set; } = true;
    public int BackupIntervalDays { get; set; } = 7;
}

/// <summary>
/// Ayarlar servisi implementasyonu
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    public AppSettings Settings { get; private set; } = new();

    public SettingsService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AydaMusavirlik"
        );

        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        _settingsFilePath = Path.Combine(appDataPath, "settings.json");

        // Ayarlari yukle
        LoadSettingsAsync().Wait();
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            Settings = new AppSettings();
        }
    }

    public async Task<bool> SaveSettingsAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateDatabaseSettingsAsync(DesktopDatabaseSettings settings)
    {
        Settings.Database = settings;
        return await SaveSettingsAsync();
    }
}
