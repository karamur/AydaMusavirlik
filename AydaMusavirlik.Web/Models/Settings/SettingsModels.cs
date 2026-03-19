namespace AydaMusavirlik.Models.Settings;

/// <summary>
/// Veritabaný baðlantý ayarlarý
/// </summary>
public class DatabaseConnection
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DatabaseType Type { get; set; }
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastTestedAt { get; set; }
    public bool? LastTestResult { get; set; }
    public string? LastTestMessage { get; set; }

    public string ConnectionString => Type switch
    {
        DatabaseType.SQLite => $"Data Source={Database}",
        DatabaseType.PostgreSQL => $"Host={Server};Port={Port};Database={Database};Username={Username};Password={Password}",
        DatabaseType.SqlServer => $"Server={Server},{Port};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True",
        DatabaseType.MySQL => $"Server={Server};Port={Port};Database={Database};Uid={Username};Pwd={Password}",
        _ => string.Empty
    };

    public string DisplayName => $"{Name} ({Type})";
    public string ServerDisplay => Type == DatabaseType.SQLite ? Database : $"{Server}:{Port}/{Database}";
}

public enum DatabaseType
{
    SQLite = 1,
    PostgreSQL = 2,
    SqlServer = 3,
    MySQL = 4
}

/// <summary>
/// Uygulama ayarlarý
/// </summary>
public class AppSettings
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "Genel";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
