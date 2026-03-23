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
    private readonly ApiClient? _apiClient;
    private readonly AuthTokenStore _tokenStore;

    // Offline test kullanicilari (API baglantisi yoksa)
    private static readonly Dictionary<string, (string Password, string FullName, string Role)> _offlineUsers = new()
    {
        ["admin"] = ("admin", "Sistem Yoneticisi", "Admin"),
        ["muhasebe"] = ("muhasebe", "Ayse Muhasebeci", "Accountant"),
        ["yonetici"] = ("yonetici", "Mehmet Yonetici", "Manager"),
        ["test"] = ("test", "Test Kullanici", "User"),
        ["demo"] = ("demo", "Demo Kullanici", "User")
    };

    public AuthService(AuthTokenStore tokenStore, ApiClient? apiClient = null)
    {
        _tokenStore = tokenStore;
        _apiClient = apiClient;
    }

    public bool IsAuthenticated => _tokenStore.IsAuthenticated;
    public string? CurrentUser => _tokenStore.Username;
    public string? CurrentUserFullName => _tokenStore.FullName;
    public string? CurrentUserRole => _tokenStore.Role;

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        // Oncelikle Offline dogrulama yap (API olmadan calismasi icin)
        var offlineResult = OfflineLogin(username, password);
        if (offlineResult.Success)
        {
            return offlineResult;
        }

        // API ile dene
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
                // API baglantisi basarisiz
            }
        }

        return new LoginResult
        {
            Success = false,
            Error = "Kullanici adi veya sifre hatali!"
        };
    }

    private LoginResult OfflineLogin(string username, string password)
    {
        var normalizedUsername = username.ToLowerInvariant().Trim();

        if (_offlineUsers.TryGetValue(normalizedUsername, out var user))
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