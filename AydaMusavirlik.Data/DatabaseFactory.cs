using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Configuration;
using AydaMusavirlik.Data.Services;

namespace AydaMusavirlik.Data;

/// <summary>
/// Veritabani baglantisi olusturan factory sinifi
/// </summary>
public static class DatabaseFactory
{
    /// <summary>
    /// DbContextOptions olusturur
    /// </summary>
    public static DbContextOptions<AppDbContext> CreateOptions(DatabaseSettings settings)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        var connectionString = string.IsNullOrEmpty(settings.ConnectionString) 
            ? settings.BuildConnectionString() 
            : settings.ConnectionString;

        switch (settings.Provider)
        {
            case DatabaseProvider.SQLite:
                builder.UseSqlite(connectionString);
                break;

            case DatabaseProvider.SqlServer:
                builder.UseSqlServer(connectionString, options =>
                {
                    options.EnableRetryOnFailure(3);
                    options.CommandTimeout(30);
                });
                break;

            case DatabaseProvider.PostgreSQL:
                builder.UseNpgsql(connectionString, options =>
                {
                    options.EnableRetryOnFailure(3);
                    options.CommandTimeout(30);
                });
                break;

            default:
                throw new NotSupportedException($"Desteklenmeyen veritabani: {settings.Provider}");
        }

        return builder.Options;
    }

    /// <summary>
    /// AppDbContext olusturur
    /// </summary>
    public static AppDbContext CreateContext(DatabaseSettings settings)
    {
        var options = CreateOptions(settings);
        return new AppDbContext(options);
    }

    /// <summary>
    /// Veritabanini olusturur ve migration uygular
    /// </summary>
    public static async Task<bool> EnsureDatabaseCreatedAsync(DatabaseSettings settings)
    {
        try
        {
            using var context = CreateContext(settings);
            await context.Database.EnsureCreatedAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Veritabani baglantisini test eder
    /// </summary>
    public static async Task<(bool Success, string Message)> TestConnectionAsync(DatabaseSettings settings)
    {
        try
        {
            using var context = CreateContext(settings);
            var canConnect = await context.Database.CanConnectAsync();

            if (canConnect)
            {
                return (true, "Baglanti basarili!");
            }

            return (false, "Veritabanina baglanilamadi.");
        }
        catch (Exception ex)
        {
            return (false, $"Baglanti hatasi: {ex.Message}");
        }
    }

    /// <summary>
    /// Veritabani bilgilerini getirir
    /// </summary>
    public static async Task<DatabaseInfo> GetDatabaseInfoAsync(DatabaseSettings settings)
    {
        var info = new DatabaseInfo
        {
            Provider = settings.Provider.ToString(),
            ConnectionString = MaskConnectionString(settings.BuildConnectionString())
        };

        try
        {
            using var context = CreateContext(settings);

            if (await context.Database.CanConnectAsync())
            {
                info.IsConnected = true;
                info.ServerVersion = context.Database.ProviderName ?? "Unknown";

                // Tablo sayilarini al
                info.CompanyCount = await context.Companies.CountAsync();
                info.AccountCount = await context.Accounts.CountAsync();
                info.RecordCount = await context.AccountingRecords.CountAsync();
                info.EmployeeCount = await context.Employees.CountAsync();
            }
        }
        catch (Exception ex)
        {
            info.ErrorMessage = ex.Message;
        }

        return info;
    }

    /// <summary>
    /// Baglanti dizesindeki hassas bilgileri maskeler
    /// </summary>
    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return connectionString;

        // Password'u maskele
        var result = System.Text.RegularExpressions.Regex.Replace(
            connectionString,
            @"(Password|Pwd)\s*=\s*[^;]+",
            "$1=******",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        return result;
    }

    /// <summary>
    /// Veritabanini olusturur ve seed data ekler
    /// </summary>
    public static async Task<(bool Success, string Message)> InitializeDatabaseAsync(DatabaseSettings settings, bool seedData = true)
    {
        try
        {
            using var context = CreateContext(settings);
            
            // Veritabanini olustur
            await context.Database.EnsureCreatedAsync();
            
            // Seed data ekle
            if (seedData)
            {
                var seedService = new SeedDataService(context);
                await seedService.SeedAllAsync();
            }
            
            return (true, "Veritabani basariyla olusturuldu ve veriler eklendi.");
        }
        catch (Exception ex)
        {
            return (false, $"Veritabani olusturma hatasi: {ex.Message}");
        }
    }
}

/// <summary>
/// Veritabani bilgileri
/// </summary>
public class DatabaseInfo
{
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string ServerVersion { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public string? ErrorMessage { get; set; }

    public int CompanyCount { get; set; }
    public int AccountCount { get; set; }
    public int RecordCount { get; set; }
    public int EmployeeCount { get; set; }
}