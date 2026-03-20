namespace AydaMusavirlik.Core.Configuration;

/// <summary>
/// Desteklenen veritabani turleri
/// </summary>
public enum DatabaseProvider
{
    SQLite = 0,
    SqlServer = 1,
    PostgreSQL = 2
}

/// <summary>
/// Veritabani baglanti ayarlari
/// </summary>
public class DatabaseSettings
{
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SQLite;
    public string ConnectionString { get; set; } = string.Empty;

    // SQLite
    public string SqliteFilePath { get; set; } = "AydaMusavirlik.db";

    // SQL Server
    public string SqlServerHost { get; set; } = "localhost";
    public int SqlServerPort { get; set; } = 1433;
    public string SqlServerDatabase { get; set; } = "AydaMusavirlik";
    public string SqlServerUsername { get; set; } = string.Empty;
    public string SqlServerPassword { get; set; } = string.Empty;
    public bool SqlServerTrustedConnection { get; set; } = true;

    // PostgreSQL
    public string PostgresHost { get; set; } = "localhost";
    public int PostgresPort { get; set; } = 5432;
    public string PostgresDatabase { get; set; } = "aydamusavirlik";
    public string PostgresUsername { get; set; } = "postgres";
    public string PostgresPassword { get; set; } = string.Empty;

    /// <summary>
    /// Baglanti dizesini olusturur
    /// </summary>
    public string BuildConnectionString()
    {
        return Provider switch
        {
            DatabaseProvider.SQLite => $"Data Source={SqliteFilePath}",

            DatabaseProvider.SqlServer => SqlServerTrustedConnection
                ? $"Server={SqlServerHost},{SqlServerPort};Database={SqlServerDatabase};Trusted_Connection=True;TrustServerCertificate=True;"
                : $"Server={SqlServerHost},{SqlServerPort};Database={SqlServerDatabase};User Id={SqlServerUsername};Password={SqlServerPassword};TrustServerCertificate=True;",

            DatabaseProvider.PostgreSQL => $"Host={PostgresHost};Port={PostgresPort};Database={PostgresDatabase};Username={PostgresUsername};Password={PostgresPassword}",

            _ => throw new NotSupportedException($"Desteklenmeyen veritabani: {Provider}")
        };
    }

    /// <summary>
    /// Varsayilan SQLite ayarlari
    /// </summary>
    public static DatabaseSettings DefaultSqlite() => new()
    {
        Provider = DatabaseProvider.SQLite,
        SqliteFilePath = "AydaMusavirlik.db"
    };

    /// <summary>
    /// Ayarlari kopyalar
    /// </summary>
    public DatabaseSettings Clone() => new()
    {
        Provider = Provider,
        ConnectionString = ConnectionString,
        SqliteFilePath = SqliteFilePath,
        SqlServerHost = SqlServerHost,
        SqlServerPort = SqlServerPort,
        SqlServerDatabase = SqlServerDatabase,
        SqlServerUsername = SqlServerUsername,
        SqlServerPassword = SqlServerPassword,
        SqlServerTrustedConnection = SqlServerTrustedConnection,
        PostgresHost = PostgresHost,
        PostgresPort = PostgresPort,
        PostgresDatabase = PostgresDatabase,
        PostgresUsername = PostgresUsername,
        PostgresPassword = PostgresPassword
    };
}

/// <summary>
/// Uygulama ayarlari
/// </summary>
public class AppSettings
{
    public const string SettingsFileName = "appsettings.json";

    public DatabaseSettings Database { get; set; } = DatabaseSettings.DefaultSqlite();
    public string AppVersion { get; set; } = "1.0.0";
    public string LastCompanyId { get; set; } = string.Empty;
    public string Theme { get; set; } = "Dark";
    public string Language { get; set; } = "tr-TR";
}