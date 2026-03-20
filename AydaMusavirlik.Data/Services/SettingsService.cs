using System.Text.Json;
using AydaMusavirlik.Core.Configuration;

namespace AydaMusavirlik.Data.Services;

/// <summary>
/// Uygulama ayarlarini yoneten servis
/// </summary>
public interface ISettingsService
{
    AppSettings Settings { get; }
    Task LoadAsync();
    Task SaveAsync();
    Task<bool> UpdateDatabaseSettingsAsync(DatabaseSettings newSettings);
}

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private AppSettings _settings = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppSettings Settings => _settings;

    public SettingsService(string? basePath = null)
    {
        var appDataPath = basePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AydaMusavirlik"
        );

        Directory.CreateDirectory(appDataPath);
        _settingsPath = Path.Combine(appDataPath, AppSettings.SettingsFileName);
    }

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            else
            {
                // Varsayilan ayarlarla baslat
                _settings = new AppSettings();
                await SaveAsync();
            }
        }
        catch
        {
            _settings = new AppSettings();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, JsonOptions);
            await File.WriteAllTextAsync(_settingsPath, json);
        }
        catch
        {
            // Loglama yapilabilir
        }
    }

    public async Task<bool> UpdateDatabaseSettingsAsync(DatabaseSettings newSettings)
    {
        try
        {
            // Yeni baglanti test et
            var (success, message) = await DatabaseFactory.TestConnectionAsync(newSettings);

            if (!success)
            {
                // Veritabanini olusturmaya calis
                var created = await DatabaseFactory.EnsureDatabaseCreatedAsync(newSettings);
                if (!created)
                {
                    return false;
                }
            }

            // Ayarlari guncelle
            _settings.Database = newSettings.Clone();
            await SaveAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}