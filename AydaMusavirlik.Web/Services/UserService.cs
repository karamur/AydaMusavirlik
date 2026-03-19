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

    private void InitializeDefaultUsers()
    {
        // Admin kullanýcýsý - tüm modüllere eriþim
        _users.Add(new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = AuthService.HashPassword("admin"),
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
            PasswordHash = AuthService.HashPassword("muhasebe123"),
            FirstName = "Ahmet",
            LastName = "Yýlmaz",
            Email = "ahmet@aydamusavirlik.com",
            Role = UserRole.Accountant,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Test yönetici
        _users.Add(new User
        {
            Id = 3,
            Username = "yonetici",
            PasswordHash = AuthService.HashPassword("yonetici123"),
            FirstName = "Ayþe",
            LastName = "Demir",
            Email = "ayse@aydamusavirlik.com",
            Role = UserRole.Manager,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        _logger.LogInformation("Varsayýlan kullanýcýlar oluþturuldu: {Count}", _users.Count);
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        return Task.FromResult(user);
    }

    public Task<User?> GetByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<List<User>> GetAllAsync()
    {
        return Task.FromResult(_users.Where(u => u.IsActive).ToList());
    }

    public Task<User> CreateAsync(User user)
    {
        user.Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;
        _users.Add(user);
        _logger.LogInformation("Kullanýcý oluþturuldu: {Username}", user.Username);
        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            var index = _users.IndexOf(existing);
            user.UpdatedAt = DateTime.UtcNow;
            _users[index] = user;
        }
        return Task.FromResult(user);
    }

    public Task DeleteAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            user.IsActive = false;
        }
        return Task.CompletedTask;
    }

    public Task ResetPasswordAsync(int userId, string newPassword)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.PasswordHash = AuthService.HashPassword(newPassword);
            user.IsLocked = false;
            user.FailedLoginAttempts = 0;
        }
        return Task.CompletedTask;
    }

    public Task UnlockAccountAsync(int userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.IsLocked = false;
            user.FailedLoginAttempts = 0;
        }
        return Task.CompletedTask;
    }
}
