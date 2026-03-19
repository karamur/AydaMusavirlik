namespace AydaMusavirlik.Models.Common;

/// <summary>
/// Firma yetkili/iletiþim kiþisi
/// </summary>
public class Contact : BaseEntity
{
    public int CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Department { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
    public string? Notes { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    // Navigation
    public virtual Company Company { get; set; } = null!;
}
