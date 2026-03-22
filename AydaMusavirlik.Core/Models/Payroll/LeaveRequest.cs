using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Core.Models.Payroll;

/// <summary>
/// Ýzin talebi
/// </summary>
public class LeaveRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public int CompanyId { get; set; }

    // Ýzin bilgileri
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string? Description { get; set; }

    // Onay bilgileri
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public int? ApprovedById { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? ApprovalNote { get; set; }

    // Vekalet bilgileri
    public int? DeputyEmployeeId { get; set; }
    public string? DeputyNote { get; set; }

    // Ýletiþim
    public string? ContactPhone { get; set; }
    public string? ContactAddress { get; set; }

    // Form bilgileri
    public string? FormNumber { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.Now;

    // Navigation
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? ApprovedBy { get; set; }
    public virtual Employee? DeputyEmployee { get; set; }
}

/// <summary>
/// Ýzin türleri
/// </summary>
public enum LeaveType
{
    Annual = 1,           // Yýllýk izin
    Sick = 2,             // Hastalýk izni
    Maternity = 3,        // Doðum izni
    Paternity = 4,        // Babalýk izni
    Marriage = 5,         // Evlilik izni
    Bereavement = 6,      // Ölüm izni
    Unpaid = 7,           // Ücretsiz izin
    Administrative = 8,   // Ýdari izin
    Compensatory = 9,     // Mazeret izni
    Military = 10,        // Askerlik izni
    Education = 11        // Eðitim izni
}

/// <summary>
/// Ýzin durumlarý
/// </summary>
public enum LeaveStatus
{
    Pending = 1,          // Bekliyor
    Approved = 2,         // Onaylandý
    Rejected = 3,         // Reddedildi
    Cancelled = 4,        // Ýptal edildi
    InProgress = 5        // Kullanýlýyor
}

/// <summary>
/// Ýzin türü helper
/// </summary>
public static class LeaveTypeHelper
{
    public static string GetDisplayName(LeaveType type) => type switch
    {
        LeaveType.Annual => "Yýllýk Ýzin",
        LeaveType.Sick => "Hastalýk Ýzni",
        LeaveType.Maternity => "Doðum Ýzni",
        LeaveType.Paternity => "Babalýk Ýzni",
        LeaveType.Marriage => "Evlilik Ýzni",
        LeaveType.Bereavement => "Ölüm Ýzni",
        LeaveType.Unpaid => "Ücretsiz Ýzin",
        LeaveType.Administrative => "Ýdari Ýzin",
        LeaveType.Compensatory => "Mazeret Ýzni",
        LeaveType.Military => "Askerlik Ýzni",
        LeaveType.Education => "Eðitim Ýzni",
        _ => "Bilinmeyen"
    };

    public static int GetMaxDays(LeaveType type) => type switch
    {
        LeaveType.Annual => 14,        // Kanuni yýllýk izin
        LeaveType.Sick => 0,           // Rapor süresince
        LeaveType.Maternity => 112,    // 16 hafta
        LeaveType.Paternity => 5,      // 5 gün
        LeaveType.Marriage => 3,       // 3 gün
        LeaveType.Bereavement => 3,    // 3 gün
        LeaveType.Unpaid => 90,        // Max 3 ay
        LeaveType.Administrative => 1, // 1 gün
        LeaveType.Compensatory => 5,   // 5 gün
        LeaveType.Military => 90,      // Askerlik süresi
        LeaveType.Education => 5,      // 5 gün
        _ => 0
    };

    public static bool IsPaid(LeaveType type) => type switch
    {
        LeaveType.Unpaid => false,
        _ => true
    };
}

public static class LeaveStatusHelper
{
    public static string GetDisplayName(LeaveStatus status) => status switch
    {
        LeaveStatus.Pending => "Bekliyor",
        LeaveStatus.Approved => "Onaylandý",
        LeaveStatus.Rejected => "Reddedildi",
        LeaveStatus.Cancelled => "Ýptal Edildi",
        LeaveStatus.InProgress => "Kullanýlýyor",
        _ => "Bilinmeyen"
    };

    public static string GetColor(LeaveStatus status) => status switch
    {
        LeaveStatus.Pending => "#FF9800",
        LeaveStatus.Approved => "#4CAF50",
        LeaveStatus.Rejected => "#F44336",
        LeaveStatus.Cancelled => "#9E9E9E",
        LeaveStatus.InProgress => "#2196F3",
        _ => "#9E9E9E"
    };
}