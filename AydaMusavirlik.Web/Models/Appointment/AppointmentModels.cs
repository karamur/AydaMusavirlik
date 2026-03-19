namespace AydaMusavirlik.Models.Appointment;

/// <summary>
/// Randevu modeli
/// </summary>
public class Appointment
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int? ContactId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan Duration => EndDate - StartDate;
    public AppointmentType Type { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public AppointmentPriority Priority { get; set; } = AppointmentPriority.Normal;
    public string? Location { get; set; }
    public bool IsOnline { get; set; }
    public string? MeetingUrl { get; set; }
    public string? Notes { get; set; }
    public int AssignedUserId { get; set; }
    public string AssignedUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrencePattern? Recurrence { get; set; }
    public List<AppointmentReminder> Reminders { get; set; } = new();
    public List<AppointmentDocument> Documents { get; set; } = new();

    // Renk kodu (takvim görünümü için)
    public string ColorCode => Type switch
    {
        AppointmentType.Meeting => "#1976D2",
        AppointmentType.Consultation => "#388E3C",
        AppointmentType.TaxDeadline => "#D32F2F",
        AppointmentType.Audit => "#7B1FA2",
        AppointmentType.Training => "#F57C00",
        AppointmentType.CourtHearing => "#C2185B",
        AppointmentType.BankAppointment => "#0097A7",
        AppointmentType.Other => "#757575",
        _ => "#1976D2"
    };
}

public enum AppointmentType
{
    Meeting = 1,            // Toplantý
    Consultation = 2,       // Danýþmanlýk görüþmesi
    TaxDeadline = 3,        // Vergi son tarihi
    Audit = 4,              // Denetim
    Training = 5,           // Eðitim
    CourtHearing = 6,       // Mahkeme duruþmasý
    BankAppointment = 7,    // Banka randevusu
    SocialSecurity = 8,     // SGK iþlemleri
    TaxOffice = 9,          // Vergi dairesi
    Other = 10              // Diðer
}

public enum AppointmentStatus
{
    Scheduled = 1,          // Planlandý
    Confirmed = 2,          // Onaylandý
    InProgress = 3,         // Devam ediyor
    Completed = 4,          // Tamamlandý
    Cancelled = 5,          // Ýptal edildi
    Postponed = 6,          // Ertelendi
    NoShow = 7              // Gelmedi
}

public enum AppointmentPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

/// <summary>
/// Tekrarlama deseni
/// </summary>
public class RecurrencePattern
{
    public RecurrenceType Type { get; set; }
    public int Interval { get; set; } = 1;
    public DayOfWeek[]? DaysOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public int? MonthOfYear { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MaxOccurrences { get; set; }
}

public enum RecurrenceType
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4
}

/// <summary>
/// Randevu hatýrlatýcýsý
/// </summary>
public class AppointmentReminder
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int MinutesBefore { get; set; }
    public ReminderType Type { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
}

public enum ReminderType
{
    Email = 1,
    SMS = 2,
    Push = 3,
    InApp = 4
}

/// <summary>
/// Randevu belgesi
/// </summary>
public class AppointmentDocument
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Takvim görünümü için özet
/// </summary>
public class CalendarEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Color { get; set; } = "#1976D2";
    public bool AllDay { get; set; }
    public string? Description { get; set; }
    public AppointmentType Type { get; set; }
    public AppointmentStatus Status { get; set; }
}

/// <summary>
/// Vergi takvimi öðesi
/// </summary>
public class TaxCalendarItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TaxType TaxType { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsRecurring { get; set; }
    public int DaysRemaining => (DueDate.Date - DateTime.Today).Days;
    public string StatusColor => DaysRemaining switch
    {
        <= 0 => "#D32F2F",  // Geçmiþ - Kýrmýzý
        <= 3 => "#F57C00",  // Acil - Turuncu
        <= 7 => "#FBC02D",  // Yakýn - Sarý
        _ => "#388E3C"      // Normal - Yeþil
    };
}

public enum TaxType
{
    KDV = 1,                    // KDV Beyannamesi
    Muhtasar = 2,               // Muhtasar Beyanname
    GelirVergisi = 3,           // Gelir Vergisi
    KurumlarVergisi = 4,        // Kurumlar Vergisi
    GeciciVergi = 5,            // Geçici Vergi
    DamgaVergisi = 6,           // Damga Vergisi
    SGK = 7,                    // SGK Bildirgeleri
    BaBeyanname = 8,            // Ba-Bs Formu
    EDefter = 9,                // E-Defter
    EFatura = 10                // E-Fatura
}

/// <summary>
/// Randevu istatistikleri
/// </summary>
public class AppointmentStatistics
{
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public int TodayAppointments { get; set; }
    public int ThisWeekAppointments { get; set; }
    public int OverdueDeadlines { get; set; }
    public Dictionary<AppointmentType, int> ByType { get; set; } = new();
    public Dictionary<int, int> ByMonth { get; set; } = new();
    public double CompletionRate => TotalAppointments > 0 
        ? (double)CompletedAppointments / TotalAppointments * 100 : 0;
}
