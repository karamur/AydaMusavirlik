using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Configuration;
using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Services;

namespace AydaMusavirlik.Desktop.Services;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string username, string password);
    void Logout();
    bool IsAuthenticated { get; }
    string? CurrentUser { get; }
    string? CurrentUserFullName { get; }
    string? CurrentUserRole { get; }
}

public class AuthService : IAuthService
{
    private readonly AuthTokenStore _tokenStore;
    private readonly ApiClient? _apiClient;
    private readonly ISettingsService _settingsService;

    // Offline test kullanicilari (Veritabani yoksa)
    private static readonly Dictionary<string, (string Password, string FullName, string Role)> _fallbackUsers = new()
    {
        ["admin"] = ("admin", "Sistem Yoneticisi", "Admin"),
        ["muhasebe"] = ("muhasebe123", "Ayse Muhasebeci", "Accountant"),
        ["yonetici"] = ("yonetici123", "Mehmet Yonetici", "Manager")
    };

    public AuthService(AuthTokenStore tokenStore, ISettingsService settingsService, ApiClient? apiClient = null)
    {
        _tokenStore = tokenStore;
        _settingsService = settingsService;
        _apiClient = apiClient;
    }

    public bool IsAuthenticated => _tokenStore.IsAuthenticated;
    public string? CurrentUser => _tokenStore.Username;
    public string? CurrentUserFullName => _tokenStore.FullName;
    public string? CurrentUserRole => _tokenStore.Role;

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        // 1. Oncelikle API ile dene
        if (_apiClient != null)
        {
            try
            {
                var response = await _apiClient.PostAsync<LoginResponse>("api/auth/login", new
                {
                    Username = username,
                    Password = password
                });

                if (response.Success && response.Data != null)
                {
                    _tokenStore.Token = response.Data.Token;
                    _tokenStore.Username = response.Data.Username;
                    _tokenStore.FullName = response.Data.FullName;
                    _tokenStore.Role = response.Data.Role;
                    _tokenStore.ExpiresAt = response.Data.ExpiresAt;

                    return new LoginResult
                    {
                        Success = true,
                        FullName = response.Data.FullName,
                        IsOnline = true
                    };
                }
            }
            catch
            {
                // API baglantisi basarisiz, veritabanina bak
            }
        }

        // 2. Veritabaninda dogrula
        try
        {
            var dbResult = await AuthenticateFromDatabaseAsync(username, password);
            if (dbResult.Success)
            {
                return dbResult;
            }
        }
        catch
        {
            // Veritabani baglantisi basarisiz, fallback kullan
        }

        // 3. Fallback kullanicilar
        return AuthenticateFromFallback(username, password);
    }

    private async Task<LoginResult> AuthenticateFromDatabaseAsync(string username, string password)
    {
        var dbSettings = _settingsService.Settings.Database;

        try
        {
            using var context = DatabaseFactory.CreateContext(dbSettings);

            if (!await context.Database.CanConnectAsync())
            {
                return new LoginResult { Success = false, Error = "Veritabanina baglanilamadi" };
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);

            if (user == null)
            {
                return new LoginResult { Success = false, Error = "Kullanici bulunamadi" };
            }

            // Sifre kontrolu
            var passwordHash = HashPassword(password);
            if (user.PasswordHash != passwordHash)
            {
                return new LoginResult { Success = false, Error = "Sifre hatali" };
            }

            // Basarili giris
            _tokenStore.Username = user.Username;
            _tokenStore.FullName = $"{user.FirstName} {user.LastName}";
            _tokenStore.Role = user.Role.ToString();
            _tokenStore.Token = Guid.NewGuid().ToString();
            _tokenStore.ExpiresAt = DateTime.UtcNow.AddHours(8);

            return new LoginResult
            {
                Success = true,
                FullName = $"{user.FirstName} {user.LastName}",
                IsOnline = false
            };
        }
        catch (Exception ex)
        {
            return new LoginResult { Success = false, Error = $"Veritabani hatasi: {ex.Message}" };
        }
    }

    private LoginResult AuthenticateFromFallback(string username, string password)
    {
        var normalizedUsername = username.ToLowerInvariant().Trim();

        if (_fallbackUsers.TryGetValue(normalizedUsername, out var user))
        {
            if (user.Password == password)
            {
                _tokenStore.Username = username;
                _tokenStore.FullName = user.FullName;
                _tokenStore.Role = user.Role;
                _tokenStore.Token = Guid.NewGuid().ToString();
                _tokenStore.ExpiresAt = DateTime.UtcNow.AddHours(8);

                return new LoginResult
                {
                    Success = true,
                    FullName = user.FullName,
                    IsOnline = false
                };
            }
        }

        return new LoginResult
        {
            Success = false,
            Error = "Kullanici adi veya sifre hatali!"
        };
    }

    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password + "AydaSalt2024");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public void Logout()
    {
        _tokenStore.Clear();
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string? FullName { get; set; }
    public string? Error { get; set; }
    public bool IsOnline { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}