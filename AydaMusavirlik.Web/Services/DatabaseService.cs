using AydaMusavirlik.Models.Settings;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace AydaMusavirlik.Services;

/// <summary>
/// Veritabaný Yönetim Servisi
/// </summary>
public class DatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly List<DatabaseConnection> _connections;
    private DatabaseConnection? _activeConnection;

    public DatabaseService(ILogger<DatabaseService> logger)
    {
        _logger = logger;
        _connections = new List<DatabaseConnection>();
        InitializeDefaultConnections();
    }

    private void InitializeDefaultConnections()
    {
        // Varsayýlan SQLite baðlantýsý
        _connections.Add(new DatabaseConnection
        {
            Id = 1,
            Name = "Yerel SQLite",
            Type = DatabaseType.SQLite,
            Database = "AydaMusavirlik.db",
            IsActive = true,
            IsDefault = true,
            LastTestedAt = DateTime.UtcNow,
            LastTestResult = true,
            LastTestMessage = "Baðlantý baþarýlý"
        });

        // Örnek PostgreSQL
        _connections.Add(new DatabaseConnection
        {
            Id = 2,
            Name = "PostgreSQL Sunucu",
            Type = DatabaseType.PostgreSQL,
            Server = "localhost",
            Port = 5432,
            Database = "ayda_musavirlik",
            Username = "postgres",
            Password = "Fast123",
            IsActive = false
        });

        // Örnek SQL Server
        _connections.Add(new DatabaseConnection
        {
            Id = 3,
            Name = "SQL Server",
            Type = DatabaseType.SqlServer,
            Server = "localhost",
            Port = 1433,
            Database = "AydaMusavirlik",
            Username = "sa",
            Password = "",
            IsActive = false
        });

        _activeConnection = _connections.First(c => c.IsActive);
    }

    public Task<List<DatabaseConnection>> GetAllConnectionsAsync()
    {
        return Task.FromResult(_connections.ToList());
    }

    public Task<DatabaseConnection?> GetActiveConnectionAsync()
    {
        return Task.FromResult(_activeConnection);
    }

    public Task<DatabaseConnection?> GetByIdAsync(int id)
    {
        return Task.FromResult(_connections.FirstOrDefault(c => c.Id == id));
    }

    public Task<DatabaseConnection> AddConnectionAsync(DatabaseConnection connection)
    {
        connection.Id = _connections.Count > 0 ? _connections.Max(c => c.Id) + 1 : 1;
        connection.CreatedAt = DateTime.UtcNow;
        _connections.Add(connection);
        _logger.LogInformation("Yeni veritabaný baðlantýsý eklendi: {Name}", connection.Name);
        return Task.FromResult(connection);
    }

    public Task<DatabaseConnection> UpdateConnectionAsync(DatabaseConnection connection)
    {
        var existing = _connections.FirstOrDefault(c => c.Id == connection.Id);
        if (existing != null)
        {
            var index = _connections.IndexOf(existing);
            _connections[index] = connection;
        }
        return Task.FromResult(connection);
    }

    public Task DeleteConnectionAsync(int id)
    {
        var connection = _connections.FirstOrDefault(c => c.Id == id);
        if (connection != null)
        {
            if (connection.IsActive)
            {
                throw new InvalidOperationException("Aktif veritabaný baðlantýsý silinemez!");
            }
            _connections.Remove(connection);
        }
        return Task.CompletedTask;
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync(DatabaseConnection connection)
    {
        try
        {
            switch (connection.Type)
            {
                case DatabaseType.SQLite:
                    return await TestSQLiteConnectionAsync(connection);
                case DatabaseType.PostgreSQL:
                    return await TestPostgreSQLConnectionAsync(connection);
                case DatabaseType.SqlServer:
                    return await TestSqlServerConnectionAsync(connection);
                default:
                    return (false, "Desteklenmeyen veritabaný türü");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Veritabaný baðlantý testi baþarýsýz: {Name}", connection.Name);
            return (false, $"Baðlantý hatasý: {ex.Message}");
        }
    }

    private async Task<(bool, string)> TestSQLiteConnectionAsync(DatabaseConnection connection)
    {
        try
        {
            await using var conn = new SqliteConnection(connection.ConnectionString);
            await conn.OpenAsync();

            connection.LastTestedAt = DateTime.UtcNow;
            connection.LastTestResult = true;
            connection.LastTestMessage = "SQLite baðlantýsý baþarýlý";

            return (true, connection.LastTestMessage);
        }
        catch (Exception ex)
        {
            connection.LastTestedAt = DateTime.UtcNow;
            connection.LastTestResult = false;
            connection.LastTestMessage = ex.Message;
            return (false, ex.Message);
        }
    }

    private Task<(bool, string)> TestPostgreSQLConnectionAsync(DatabaseConnection connection)
    {
        // PostgreSQL testi - Npgsql paketi gerekli
        connection.LastTestedAt = DateTime.UtcNow;
        connection.LastTestResult = false;
        connection.LastTestMessage = "PostgreSQL desteði için Npgsql paketi gerekli";
        return Task.FromResult((false, connection.LastTestMessage));
    }

    private Task<(bool, string)> TestSqlServerConnectionAsync(DatabaseConnection connection)
    {
        // SQL Server testi - Microsoft.Data.SqlClient paketi gerekli
        connection.LastTestedAt = DateTime.UtcNow;
        connection.LastTestResult = false;
        connection.LastTestMessage = "SQL Server desteði için Microsoft.Data.SqlClient paketi gerekli";
        return Task.FromResult((false, connection.LastTestMessage));
    }

    public async Task<bool> SetActiveConnectionAsync(int id)
    {
        var connection = _connections.FirstOrDefault(c => c.Id == id);
        if (connection == null) return false;

        // Önce baðlantýyý test et
        var (success, message) = await TestConnectionAsync(connection);
        if (!success)
        {
            _logger.LogWarning("Veritabaný aktif edilemedi, baðlantý testi baþarýsýz: {Message}", message);
            return false;
        }

        // Tüm baðlantýlarý pasif yap
        foreach (var conn in _connections)
        {
            conn.IsActive = false;
        }

        // Seçilen baðlantýyý aktif yap
        connection.IsActive = true;
        _activeConnection = connection;

        _logger.LogInformation("Aktif veritabaný deðiþtirildi: {Name}", connection.Name);
        return true;
    }

    public string GetCurrentConnectionString()
    {
        return _activeConnection?.ConnectionString ?? "Data Source=AydaMusavirlik.db";
    }

    public DatabaseType GetCurrentDatabaseType()
    {
        return _activeConnection?.Type ?? DatabaseType.SQLite;
    }
}
