using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Core.Entities;

/// <summary>
/// Bildirim entity
/// </summary>
public class Notification : BaseEntity
{
    public int CompanyId { get; set; }
    public int? UserId { get; set; }

    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public NotificationCategory Category { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }

    public string? ActionUrl { get; set; }
    public string? ActionData { get; set; }

    public DateTime? ScheduledFor { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Navigation
    public virtual Company? Company { get; set; }
}

/// <summary>
/// Hatirlatici entity
/// </summary>
public class Reminder : BaseEntity
{
    public int CompanyId { get; set; }
    public int? UserId { get; set; }

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public ReminderType Type { get; set; }

    public DateTime DueDate { get; set; }
    public int ReminderDaysBefore { get; set; } = 3;

    public bool IsRecurring { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public bool NotificationSent { get; set; }

    // Navigation
    public virtual Company? Company { get; set; }
}

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success,
    Reminder
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public enum NotificationCategory
{
    System,
    Accounting,
    Payroll,
    Tax,
    Leave,
    Document,
    Payment,
    Report
}

public enum ReminderType
{
    TaxDeclaration,      // Vergi beyanname
    SgkDeclaration,      // SGK bildirge
    PayrollProcess,      // Bordro islemleri
    InvoiceDue,          // Fatura vadesi
    ContractExpiry,      // Sozlesme bitisi
    LicenseRenewal,      // Lisans yenileme
    AuditDate,           // Denetim tarihi
    MeetingDate,         // Toplanti
    Custom               // Ozel
}

public enum RecurrencePattern
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}