namespace AydaMusavirlik.Core.Models.Common;

/// <summary>
/// Uygulama kullanýcýsý
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime? LastLoginAt { get; set; }
    public bool IsLocked { get; set; }
    public int FailedLoginAttempts { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}

public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Accountant = 3,   // Muhasebeci
    Auditor = 4,      // Denetçi
    User = 5
}
