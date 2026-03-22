using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Core.Models.Payroll;

/// <summary>
/// Izin talebi
/// </summary>
public class LeaveRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public int CompanyId { get; set; }
    
    // Izin bilgileri
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
    
    // Iletisim
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
/// Izin turleri
/// </summary>
public enum LeaveType
{
    Annual = 1,
    Sick = 2,
    Maternity = 3,
    Paternity = 4,
    Marriage = 5,
    Bereavement = 6,
    Unpaid = 7,
    Administrative = 8,
    Compensatory = 9,
    Military = 10,
    Education = 11
}

/// <summary>
/// Izin durumlari
/// </summary>
public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    InProgress = 5
}

/// <summary>
/// Izin turu helper
/// </summary>
public static class LeaveTypeHelper
{
    public static string GetDisplayName(LeaveType type) => type switch
    {
        LeaveType.Annual => "Yillik Izin",
        LeaveType.Sick => "Hastalik Izni",
        LeaveType.Maternity => "Dogum Izni",
        LeaveType.Paternity => "Babalik Izni",
        LeaveType.Marriage => "Evlilik Izni",
        LeaveType.Bereavement => "Olum Izni",
        LeaveType.Unpaid => "Ucretsiz Izin",
        LeaveType.Administrative => "Idari Izin",
        LeaveType.Compensatory => "Mazeret Izni",
        LeaveType.Military => "Askerlik Izni",
        LeaveType.Education => "Egitim Izni",
        _ => "Bilinmeyen"
    };

    public static int GetMaxDays(LeaveType type) => type switch
    {
        LeaveType.Annual => 14,
        LeaveType.Sick => 0,
        LeaveType.Maternity => 112,
        LeaveType.Paternity => 5,
        LeaveType.Marriage => 3,
        LeaveType.Bereavement => 3,
        LeaveType.Unpaid => 90,
        LeaveType.Administrative => 1,
        LeaveType.Compensatory => 5,
        LeaveType.Military => 90,
        LeaveType.Education => 5,
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
        LeaveStatus.Approved => "Onaylandi",
        LeaveStatus.Rejected => "Reddedildi",
        LeaveStatus.Cancelled => "Iptal Edildi",
        LeaveStatus.InProgress => "Kullaniliyor",
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