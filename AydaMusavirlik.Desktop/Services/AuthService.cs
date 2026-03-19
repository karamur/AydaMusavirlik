namespace AydaMusavirlik.Desktop.Services;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string username, string password);
    Task<bool> RegisterAsync(string username, string email, string fullName, string password);
    void Logout();
    bool IsAuthenticated { get; }
    string? CurrentUser { get; }
    string? CurrentUserFullName { get; }
}

public class AuthService : IAuthService
{
    private readonly ApiClient _apiClient;
    private readonly AuthTokenStore _tokenStore;

    public AuthService(ApiClient apiClient, AuthTokenStore tokenStore)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
    }

    public bool IsAuthenticated => _tokenStore.IsAuthenticated;
    public string? CurrentUser => _tokenStore.Username;
    public string? CurrentUserFullName => _tokenStore.FullName;

    public async Task<LoginResult> LoginAsync(string username, string password)
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

            return new LoginResult { Success = true, FullName = response.Data.FullName };
        }

        return new LoginResult { Success = false, Error = response.Error ?? "Giris basarisiz" };
    }

    public async Task<bool> RegisterAsync(string username, string email, string fullName, string password)
    {
        var response = await _apiClient.PostAsync<object>("api/auth/register", new
        {
            Username = username,
            Email = email,
            FullName = fullName,
            Password = password
        });

        return response.Success;
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

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}