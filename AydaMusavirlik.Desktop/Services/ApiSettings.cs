namespace AydaMusavirlik.Desktop.Services;

public class ApiSettings
{
    public string BaseUrl { get; set; } = "http://localhost:5000";
    public int TimeoutSeconds { get; set; } = 30;
}

public class AuthTokenStore
{
    public string? Token { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token) && ExpiresAt > DateTime.UtcNow;

    public void Clear()
    {
        Token = null;
        Username = null;
        FullName = null;
        Role = null;
        ExpiresAt = null;
    }
}