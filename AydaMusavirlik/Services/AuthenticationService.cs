using System.Security.Cryptography;
using System.Text;
using AydaMusavirlik.Models.Common;

namespace AydaMusavirlik.Services;

/// <summary>
/// Kimlik doðrulama servisi
/// </summary>
public class AuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly UserService _userService;

    private User? _currentUser;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        UserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    /// <summary>
    /// Mevcut oturum açmýþ kullanýcý
    /// </summary>
    public User? CurrentUser => _currentUser;

    /// <summary>
    /// Oturum açýk mý
    /// </summary>
    public bool IsAuthenticated => _currentUser != null;

    /// <summary>
    /// Admin mi
    /// </summary>
    public bool IsAdmin => _currentUser?.Role == UserRole.Admin;

    /// <summary>
    /// Giriþ yap
    /// </summary>
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
                return new LoginResult { Success = false, ErrorMessage = "Hesabýnýz kilitlenmiþ. Yöneticiyle iletiþime geçin." };
            }

            var passwordHash = HashPassword(password);
            if (user.PasswordHash != passwordHash)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                    _logger.LogWarning("Hesap kilitlendi: {Username}", username);
                }

                await _userService.UpdateAsync(user);

                _logger.LogWarning("Hatalý þifre: {Username}", username);
                return new LoginResult { Success = false, ErrorMessage = "Hatalý þifre" };
            }

            // Baþarýlý giriþ
            user.FailedLoginAttempts = 0;
            user.LastLoginAt = DateTime.UtcNow;
            await _userService.UpdateAsync(user);

            _currentUser = user;

            _logger.LogInformation("Giriþ baþarýlý: {Username}, Rol: {Role}", username, user.Role);

            return new LoginResult 
            { 
                Success = true, 
                User = user,
                Message = $"Hoþ geldiniz, {user.FullName}!"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Giriþ hatasý: {Username}", username);
            return new LoginResult { Success = false, ErrorMessage = "Giriþ sýrasýnda bir hata oluþtu" };
        }
    }

    /// <summary>
    /// Çýkýþ yap
    /// </summary>
    public Task LogoutAsync()
    {
        if (_currentUser != null)
        {
            _logger.LogInformation("Çýkýþ yapýldý: {Username}", _currentUser.Username);
            _currentUser = null;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Þifre deðiþtir
    /// </summary>
    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        if (_currentUser == null)
            return false;

        var currentHash = HashPassword(currentPassword);
        if (_currentUser.PasswordHash != currentHash)
        {
            _logger.LogWarning("Þifre deðiþikliði - mevcut þifre hatalý: {Username}", _currentUser.Username);
            return false;
        }

        _currentUser.PasswordHash = HashPassword(newPassword);
        await _userService.UpdateAsync(_currentUser);

        _logger.LogInformation("Þifre deðiþtirildi: {Username}", _currentUser.Username);
        return true;
    }

    /// <summary>
    /// Þifre hashleme
    /// </summary>
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + "AYDA_SALT_2024");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

/// <summary>
/// Giriþ sonucu
/// </summary>
public class LoginResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public User? User { get; set; }
}
