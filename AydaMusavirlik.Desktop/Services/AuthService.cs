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

    // Offline test kullanicilari
    private static readonly Dictionary<string, (string Password, string FullName, string Role)> _users = new()
    {
        ["admin"] = ("admin", "Sistem Yoneticisi", "Admin"),
        ["muhasebe"] = ("muhasebe123", "Ayse Muhasebeci", "Accountant"),
        ["yonetici"] = ("yonetici123", "Mehmet Yonetici", "Manager")
    };

    public AuthService(AuthTokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public bool IsAuthenticated => _tokenStore.IsAuthenticated;
    public string? CurrentUser => _tokenStore.Username;
    public string? CurrentUserFullName => _tokenStore.FullName;
    public string? CurrentUserRole => _tokenStore.Role;

    public Task<LoginResult> LoginAsync(string username, string password)
    {
        // Offline dogrulama - buyuk/kucuk harf duyarsiz
        var normalizedUsername = username.ToLowerInvariant().Trim();
        
        if (_users.TryGetValue(normalizedUsername, out var user))
        {
            if (user.Password == password)
            {
                _tokenStore.Username = username;
                _tokenStore.FullName = user.FullName;
                _tokenStore.Role = user.Role;
                _tokenStore.Token = Guid.NewGuid().ToString();
                _tokenStore.ExpiresAt = DateTime.UtcNow.AddHours(8);

                return Task.FromResult(new LoginResult
                {
                    Success = true,
                    FullName = user.FullName
                });
            }
        }

        return Task.FromResult(new LoginResult
        {
            Success = false,
            Error = "Kullanici adi veya sifre hatali!"
        });
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
}