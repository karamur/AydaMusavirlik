using System.Security.Cryptography;
using System.Text;
using AydaMusavirlik.Models.Common;

namespace AydaMusavirlik.Services;

/// <summary>
/// Kimlik doðrulama servisi
/// </summary>
public class AuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly UserService _userService;

    private User? _currentUser;

    public AuthService(ILogger<AuthService> logger, UserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public User? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    public bool IsAdmin => _currentUser?.Role == UserRole.Admin;

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        try
        {
            var user = await _userService.GetByUsernameAsync(username);

            if (user == null)
            {
                _logger.LogWarning("Kullanýcý bulunamadý: {Username}", username);
                return new LoginResult { Success = false, ErrorMessage = "Kullanýcý bulunamadý" };
            }

            if (user.IsLocked)
            {
                _logger.LogWarning("Hesap kilitli: {Username}", username);
                return new LoginResult { Success = false, ErrorMessage = "Hesabýnýz kilitlenmiþ" };
            }

            var passwordHash = HashPassword(password);
            if (user.PasswordHash != passwordHash)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                }
                await _userService.UpdateAsync(user);
                return new LoginResult { Success = false, ErrorMessage = "Hatalý þifre" };
            }

            // Baþarýlý giriþ
            user.FailedLoginAttempts = 0;
            user.LastLoginAt = DateTime.UtcNow;
            await _userService.UpdateAsync(user);

            _currentUser = user;

            _logger.LogInformation("Giriþ baþarýlý: {Username}", username);

            return new LoginResult 
            { 
                Success = true, 
                User = user,
                Message = $"Hoþ geldiniz, {user.FullName}!"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Giriþ hatasý");
            return new LoginResult { Success = false, ErrorMessage = "Giriþ sýrasýnda hata oluþtu" };
        }
    }

    public Task LogoutAsync()
    {
        if (_currentUser != null)
        {
            _logger.LogInformation("Çýkýþ yapýldý: {Username}", _currentUser.Username);
            _currentUser = null;
        }
        return Task.CompletedTask;
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + "AYDA_SALT_2024");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public User? User { get; set; }
}
