using AydaMusavirlik.Models.Appointment;

namespace AydaMusavirlik.Services;

/// <summary>
/// Randevu ve Takvim Yönetim Servisi
/// </summary>
public class AppointmentService
{
    private readonly ILogger<AppointmentService> _logger;
    private readonly List<Appointment> _appointments = new();
    private readonly List<TaxCalendarItem> _taxCalendar = new();

    public AppointmentService(ILogger<AppointmentService> logger)
    {
        _logger = logger;
        InitializeSampleData();
        InitializeTaxCalendar();
    }

    private void InitializeSampleData()
    {
        var today = DateTime.Today;

        _appointments.AddRange(new[]
        {
            new Appointment
            {
                Id = 1,
                CompanyId = 1,
                CompanyName = "ABC Teknoloji A.Þ.",
                Title = "Aylýk Muhasebe Görüþmesi",
                Description = "Ocak ayý muhasebe kayýtlarýnýn incelenmesi",
                StartDate = today.AddDays(1).AddHours(10),
                EndDate = today.AddDays(1).AddHours(11),
                Type = AppointmentType.Meeting,
                Status = AppointmentStatus.Confirmed,
                Priority = AppointmentPriority.Normal,
                Location = "Þirket Ofisi",
                AssignedUserId = 1,
                AssignedUserName = "Ahmet Yýlmaz",
                Reminders = new List<AppointmentReminder>
                {
                    new() { Id = 1, AppointmentId = 1, MinutesBefore = 60, Type = ReminderType.Email },
                    new() { Id = 2, AppointmentId = 1, MinutesBefore = 15, Type = ReminderType.InApp }
                }
            },
            new Appointment
            {
                Id = 2,
                CompanyId = 2,
                CompanyName = "XYZ Danýþmanlýk Ltd. Þti.",
                Title = "Vergi Dairesi Randevusu",
                Description = "KDV incelemesi hakkýnda görüþme",
                StartDate = today.AddDays(2).AddHours(14),
                EndDate = today.AddDays(2).AddHours(15).AddMinutes(30),
                Type = AppointmentType.TaxOffice,
                Status = AppointmentStatus.Scheduled,
                Priority = AppointmentPriority.High,
                Location = "Kadýköy Vergi Dairesi",
                AssignedUserId = 2,
                AssignedUserName = "Ayþe Demir"
            },
            new Appointment
            {
                Id = 3,
                CompanyId = 1,
                CompanyName = "ABC Teknoloji A.Þ.",
                Title = "Online Danýþmanlýk",
                Description = "Yeni yatýrým teþvikleri hakkýnda bilgilendirme",
                StartDate = today.AddHours(14),
                EndDate = today.AddHours(15),
                Type = AppointmentType.Consultation,
                Status = AppointmentStatus.Scheduled,
                Priority = AppointmentPriority.Normal,
                IsOnline = true,
                MeetingUrl = "https://meet.google.com/abc-defg-hij",
                AssignedUserId = 1,
                AssignedUserName = "Ahmet Yýlmaz"
            },
            new Appointment
            {
                Id = 4,
                CompanyId = 3,
                CompanyName = "Ahmet Yýlmaz - Þahýs",
                Title = "Yýllýk Denetim",
                Description = "2024 yýlý mali denetim",
                StartDate = today.AddDays(7).AddHours(9),
                EndDate = today.AddDays(7).AddHours(17),
                Type = AppointmentType.Audit,
                Status = AppointmentStatus.Scheduled,
                Priority = AppointmentPriority.High,
                Location = "Müþteri Ofisi",
                AssignedUserId = 3,
                AssignedUserName = "Mehmet Kaya"
            },
            new Appointment
            {
                Id = 5,
                CompanyId = 2,
                CompanyName = "XYZ Danýþmanlýk Ltd. Þti.",
                Title = "SGK Toplantýsý",
                Description = "Prim borcu yapýlandýrmasý",
                StartDate = today.AddDays(-1).AddHours(11),
                EndDate = today.AddDays(-1).AddHours(12),
                Type = AppointmentType.SocialSecurity,
                Status = AppointmentStatus.Completed,
                Priority = AppointmentPriority.Normal,
                Location = "SGK Ýl Müdürlüðü",
                AssignedUserId = 2,
                AssignedUserName = "Ayþe Demir"
            },
            new Appointment
            {
                Id = 6,
                CompanyId = 1,
                CompanyName = "ABC Teknoloji A.Þ.",
                Title = "Banka Kredi Görüþmesi",
                Description = "Ýþletme kredisi baþvurusu",
                StartDate = today.AddDays(3).AddHours(10),
                EndDate = today.AddDays(3).AddHours(11),
                Type = AppointmentType.BankAppointment,
                Status = AppointmentStatus.Confirmed,
                Priority = AppointmentPriority.High,
                Location = "Garanti BBVA Þubesi",
                AssignedUserId = 1,
                AssignedUserName = "Ahmet Yýlmaz"
            }
        });

        _logger.LogInformation("Örnek randevular oluþturuldu: {Count}", _appointments.Count);
    }

    private void InitializeTaxCalendar()
    {
        var today = DateTime.Today;
        var currentMonth = new DateTime(today.Year, today.Month, 1);

        _taxCalendar.AddRange(new[]
        {
            new TaxCalendarItem
            {
                Id = 1,
                Title = "KDV Beyannamesi",
                Description = "Aylýk KDV beyannamesi son gün",
                DueDate = new DateTime(today.Year, today.Month, 26),
                TaxType = TaxType.KDV,
                IsRecurring = true
            },
            new TaxCalendarItem
            {
                Id = 2,
                Title = "Muhtasar Beyanname",
                Description = "Muhtasar ve prim hizmet beyannamesi",
                DueDate = new DateTime(today.Year, today.Month, 26),
                TaxType = TaxType.Muhtasar,
                IsRecurring = true
            },
            new TaxCalendarItem
            {
                Id = 3,
                Title = "SGK Bildirgeleri",
                Description = "Aylýk prim ve hizmet belgesi",
                DueDate = new DateTime(today.Year, today.Month, 26),
                TaxType = TaxType.SGK,
                IsRecurring = true
            },
            new TaxCalendarItem
            {
                Id = 4,
                Title = "Ba-Bs Formlarý",
                Description = "Mal ve hizmet alým/satým bildirim formu",
                DueDate = currentMonth.AddMonths(1).AddDays(-1),
                TaxType = TaxType.BaBeyanname,
                IsRecurring = true
            },
            new TaxCalendarItem
            {
                Id = 5,
                Title = "E-Defter Beratý",
                Description = "E-Defter beratý yükleme son gün",
                DueDate = currentMonth.AddMonths(1).AddDays(-1),
                TaxType = TaxType.EDefter,
                IsRecurring = true
            },
            new TaxCalendarItem
            {
                Id = 6,
                Title = "Geçici Vergi (1. Dönem)",
                Description = "1. Dönem geçici vergi beyannamesi",
                DueDate = new DateTime(today.Year, 5, 17),
                TaxType = TaxType.GeciciVergi,
                IsRecurring = true
            },
            new TaxCalendarItem
            {
                Id = 7,
                Title = "Geçici Vergi (2. Dönem)",
                Description = "2. Dönem geçici vergi beyannamesi",
                DueDate = new DateTime(today.Year, 8, 17),
                TaxType = TaxType.GeciciVergi,
                IsRecurring = true
            },
            new TaxCalendarItem
            {
                Id = 8,
                Title = "Geçici Vergi (3. Dönem)",
                Description = "3. Dönem geçici vergi beyannamesi",
                DueDate = new DateTime(today.Year, 11, 17),
                TaxType = TaxType.GeciciVergi,
                IsRecurring = true
            },
            new TaxCalendarItem
            {
                Id = 9,
                Title = "Geçici Vergi (4. Dönem)",
                Description = "4. Dönem geçici vergi beyannamesi",
                DueDate = new DateTime(today.Year + 1, 2, 17),
                TaxType = TaxType.GeciciVergi,
                IsRecurring = true
            },
            new TaxCalendarItem
            {
                Id = 10,
                Title = "Kurumlar Vergisi",
                Description = "Yýllýk kurumlar vergisi beyannamesi",
                DueDate = new DateTime(today.Year, 4, 30),
                TaxType = TaxType.KurumlarVergisi,
                IsRecurring = true
            }
        });

        _logger.LogInformation("Vergi takvimi oluþturuldu: {Count} öðe", _taxCalendar.Count);
    }

    // Randevu CRUD Ýþlemleri
    public Task<List<Appointment>> GetAllAsync()
    {
        return Task.FromResult(_appointments.OrderBy(a => a.StartDate).ToList());
    }

    public Task<Appointment?> GetByIdAsync(int id)
    {
        return Task.FromResult(_appointments.FirstOrDefault(a => a.Id == id));
    }

    public Task<List<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        var appointments = _appointments
            .Where(a => a.StartDate >= start && a.StartDate <= end)
            .OrderBy(a => a.StartDate)
            .ToList();
        return Task.FromResult(appointments);
    }

    public Task<List<Appointment>> GetByCompanyAsync(int companyId)
    {
        return Task.FromResult(_appointments
            .Where(a => a.CompanyId == companyId)
            .OrderByDescending(a => a.StartDate)
            .ToList());
    }

    public Task<List<Appointment>> GetTodayAsync()
    {
        var today = DateTime.Today;
        return Task.FromResult(_appointments
            .Where(a => a.StartDate.Date == today)
            .OrderBy(a => a.StartDate)
            .ToList());
    }

    public Task<List<Appointment>> GetUpcomingAsync(int days = 7)
    {
        var today = DateTime.Today;
        var endDate = today.AddDays(days);
        return Task.FromResult(_appointments
            .Where(a => a.StartDate.Date >= today && a.StartDate.Date <= endDate)
            .OrderBy(a => a.StartDate)
            .ToList());
    }

    public Task<Appointment> CreateAsync(Appointment appointment)
    {
        appointment.Id = _appointments.Count > 0 ? _appointments.Max(a => a.Id) + 1 : 1;
        appointment.CreatedAt = DateTime.UtcNow;
        _appointments.Add(appointment);
        _logger.LogInformation("Randevu oluþturuldu: {Title}", appointment.Title);
        return Task.FromResult(appointment);
    }

    public Task<Appointment> UpdateAsync(Appointment appointment)
    {
        var existing = _appointments.FirstOrDefault(a => a.Id == appointment.Id);
        if (existing != null)
        {
            var index = _appointments.IndexOf(existing);
            appointment.UpdatedAt = DateTime.UtcNow;
            _appointments[index] = appointment;
        }
        return Task.FromResult(appointment);
    }

    public Task DeleteAsync(int id)
    {
        var appointment = _appointments.FirstOrDefault(a => a.Id == id);
        if (appointment != null)
        {
            _appointments.Remove(appointment);
        }
        return Task.CompletedTask;
    }

    public Task<bool> UpdateStatusAsync(int id, AppointmentStatus status)
    {
        var appointment = _appointments.FirstOrDefault(a => a.Id == id);
        if (appointment != null)
        {
            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    // Takvim Görünümü
    public Task<List<CalendarEvent>> GetCalendarEventsAsync(DateTime start, DateTime end)
    {
        var events = _appointments
            .Where(a => a.StartDate >= start && a.StartDate <= end)
            .Select(a => new CalendarEvent
            {
                Id = a.Id,
                Title = a.Title,
                Start = a.StartDate,
                End = a.EndDate,
                Color = a.ColorCode,
                AllDay = (a.EndDate - a.StartDate).TotalHours >= 8,
                Description = a.Description,
                Type = a.Type,
                Status = a.Status
            })
            .ToList();
        return Task.FromResult(events);
    }

    // Vergi Takvimi
    public Task<List<TaxCalendarItem>> GetTaxCalendarAsync()
    {
        return Task.FromResult(_taxCalendar.OrderBy(t => t.DueDate).ToList());
    }

    public Task<List<TaxCalendarItem>> GetUpcomingDeadlinesAsync(int days = 30)
    {
        var today = DateTime.Today;
        var endDate = today.AddDays(days);
        return Task.FromResult(_taxCalendar
            .Where(t => t.DueDate >= today && t.DueDate <= endDate && !t.IsCompleted)
            .OrderBy(t => t.DueDate)
            .ToList());
    }

    public Task<List<TaxCalendarItem>> GetOverdueDeadlinesAsync()
    {
        var today = DateTime.Today;
        return Task.FromResult(_taxCalendar
            .Where(t => t.DueDate < today && !t.IsCompleted)
            .OrderBy(t => t.DueDate)
            .ToList());
    }

    // Ýstatistikler
    public Task<AppointmentStatistics> GetStatisticsAsync()
    {
        var today = DateTime.Today;
        var weekEnd = today.AddDays(7);

        var stats = new AppointmentStatistics
        {
            TotalAppointments = _appointments.Count,
            CompletedAppointments = _appointments.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledAppointments = _appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            UpcomingAppointments = _appointments.Count(a => a.StartDate > DateTime.Now && a.Status != AppointmentStatus.Cancelled),
            TodayAppointments = _appointments.Count(a => a.StartDate.Date == today),
            ThisWeekAppointments = _appointments.Count(a => a.StartDate.Date >= today && a.StartDate.Date <= weekEnd),
            OverdueDeadlines = _taxCalendar.Count(t => t.DueDate < today && !t.IsCompleted),
            ByType = _appointments.GroupBy(a => a.Type).ToDictionary(g => g.Key, g => g.Count()),
            ByMonth = _appointments.GroupBy(a => a.StartDate.Month).ToDictionary(g => g.Key, g => g.Count())
        };

        return Task.FromResult(stats);
    }

    // Çakýþma Kontrolü
    public Task<bool> HasConflictAsync(DateTime start, DateTime end, int? excludeId = null)
    {
        var hasConflict = _appointments
            .Where(a => excludeId == null || a.Id != excludeId)
            .Any(a => start < a.EndDate && end > a.StartDate);
        return Task.FromResult(hasConflict);
    }

    // Müsaitlik Kontrolü
    public Task<List<TimeSlot>> GetAvailableSlotsAsync(DateTime date, int durationMinutes = 60)
    {
        var slots = new List<TimeSlot>();
        var workStart = date.Date.AddHours(9);
        var workEnd = date.Date.AddHours(18);
        var duration = TimeSpan.FromMinutes(durationMinutes);

        var dayAppointments = _appointments
            .Where(a => a.StartDate.Date == date.Date)
            .OrderBy(a => a.StartDate)
            .ToList();

        var current = workStart;
        while (current.Add(duration) <= workEnd)
        {
            var slotEnd = current.Add(duration);
            var isAvailable = !dayAppointments.Any(a => current < a.EndDate && slotEnd > a.StartDate);

            slots.Add(new TimeSlot
            {
                Start = current,
                End = slotEnd,
                IsAvailable = isAvailable
            });

            current = current.AddMinutes(30);
        }

        return Task.FromResult(slots);
    }
}

public class TimeSlot
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool IsAvailable { get; set; }
}
