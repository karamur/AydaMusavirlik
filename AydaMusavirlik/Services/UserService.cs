using AydaMusavirlik.Models.Common;

namespace AydaMusavirlik.Services;

/// <summary>
/// Kullanýcý yönetim servisi
/// </summary>
public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly List<User> _users = new();

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
        InitializeDefaultUsers();
    }

    /// <summary>
    /// Varsayýlan kullanýcýlarý oluþtur
    /// </summary>
    private void InitializeDefaultUsers()
    {
        // Admin kullanýcýsý - tüm modüllere eriþim
        _users.Add(new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = AuthenticationService.HashPassword("admin"),
            FirstName = "Sistem",
            LastName = "Yöneticisi",
            Email = "admin@aydamusavirlik.com",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Test muhasebeci
        _users.Add(new User
        {
            Id = 2,
            Username = "muhasebe",
            PasswordHash = AuthenticationService.HashPassword("muhasebe123"),
            FirstName = "Ahmet",
            LastName = "Yýlmaz",
            Email = "ahmet@aydamusavirlik.com",
            Role = UserRole.Accountant,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Test denetçi
        _users.Add(new User
        {
            Id = 3,
            Username = "denetci",
            PasswordHash = AuthenticationService.HashPassword("denetci123"),
            FirstName = "Mehmet",
            LastName = "Kaya",
            Email = "mehmet@aydamusavirlik.com",
            Role = UserRole.Auditor,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Test manager
        _users.Add(new User
        {
            Id = 4,
            Username = "yonetici",
            PasswordHash = AuthenticationService.HashPassword("yonetici123"),
            FirstName = "Ayþe",
            LastName = "Demir",
            Email = "ayse@aydamusavirlik.com",
            Role = UserRole.Manager,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        _logger.LogInformation("Varsayýlan kullanýcýlar oluþturuldu: {Count} kullanýcý", _users.Count);
    }

    /// <summary>
    /// Kullanýcý adýna göre getir
    /// </summary>
    public Task<User?> GetByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        return Task.FromResult(user);
    }

    /// <summary>
    /// ID'ye göre getir
    /// </summary>
    public Task<User?> GetByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    /// <summary>
    /// Tüm kullanýcýlarý getir
    /// </summary>
    public Task<List<User>> GetAllAsync()
    {
        return Task.FromResult(_users.Where(u => u.IsActive).ToList());
    }

    /// <summary>
    /// Kullanýcý ekle
    /// </summary>
    public Task<User> CreateAsync(User user)
    {
        user.Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;
        _users.Add(user);

        _logger.LogInformation("Kullanýcý oluþturuldu: {Username}", user.Username);
        return Task.FromResult(user);
    }

    /// <summary>
    /// Kullanýcý güncelle
    /// </summary>
    public Task<User> UpdateAsync(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            var index = _users.IndexOf(existing);
            user.UpdatedAt = DateTime.UtcNow;
            _users[index] = user;
            _logger.LogInformation("Kullanýcý güncellendi: {Username}", user.Username);
        }
        return Task.FromResult(user);
    }

    /// <summary>
    /// Kullanýcý sil (soft delete)
    /// </summary>
    public Task DeleteAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            _logger.LogInformation("Kullanýcý silindi: {Username}", user.Username);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Þifre sýfýrla
    /// </summary>
    public Task ResetPasswordAsync(int userId, string newPassword)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.PasswordHash = AuthenticationService.HashPassword(newPassword);
            user.IsLocked = false;
            user.FailedLoginAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;
            _logger.LogInformation("Þifre sýfýrlandý: {Username}", user.Username);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Hesap kilidini aç
    /// </summary>
    public Task UnlockAccountAsync(int userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.IsLocked = false;
            user.FailedLoginAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;
            _logger.LogInformation("Hesap kilidi açýldý: {Username}", user.Username);
        }
        return Task.CompletedTask;
    }
}
