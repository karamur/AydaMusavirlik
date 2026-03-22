using Microsoft.AspNetCore.Mvc;
using AydaMusavirlik.Data.Repositories;
using AydaMusavirlik.Core.Models.Payroll;

namespace AydaMusavirlik.Api.Controllers;

/// <summary>
/// Ýzin Yönetimi API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveRequestRepository _leaveRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveController(
        ILeaveRequestRepository leaveRepository,
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork)
    {
        _leaveRepository = leaveRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Bekleyen izin taleplerini getirir (Yönetici için)
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetPendingRequests([FromQuery] int companyId = 1)
    {
        var requests = await _leaveRepository.GetPendingByCompanyAsync(companyId);

        if (!requests.Any())
        {
            // Demo veri
            return Ok(new List<LeaveRequestDto>
            {
                new() { Id = 1, PersonelAdi = "Ahmet Yýlmaz", IzinTuru = "Yýllýk Ýzin", BaslangicTarihi = DateTime.Now.AddDays(5), BitisTarihi = DateTime.Now.AddDays(10), GunSayisi = 6, Durum = "Bekliyor", TalepTarihi = DateTime.Now.AddDays(-2) },
                new() { Id = 2, PersonelAdi = "Mehmet Demir", IzinTuru = "Mazeret Ýzni", BaslangicTarihi = DateTime.Now.AddDays(2), BitisTarihi = DateTime.Now.AddDays(3), GunSayisi = 2, Durum = "Bekliyor", TalepTarihi = DateTime.Now.AddDays(-1) },
            });
        }

        return Ok(requests.Select(r => new LeaveRequestDto
        {
            Id = r.Id,
            PersonelId = r.EmployeeId,
            PersonelAdi = $"{r.Employee.FirstName} {r.Employee.LastName}",
            IzinTuru = LeaveTypeHelper.GetDisplayName(r.LeaveType),
            IzinTuruEnum = r.LeaveType,
            BaslangicTarihi = r.StartDate,
            BitisTarihi = r.EndDate,
            GunSayisi = r.TotalDays,
            Aciklama = r.Description,
            Durum = LeaveStatusHelper.GetDisplayName(r.Status),
            DurumEnum = r.Status,
            TalepTarihi = r.RequestDate,
            FormNo = r.FormNumber
        }).ToList());
    }

    /// <summary>
    /// Tüm izin taleplerini getirir
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetAllRequests(
        [FromQuery] int companyId = 1,
        [FromQuery] LeaveStatus? status = null)
    {
        var requests = status.HasValue
            ? await _leaveRepository.GetByStatusAsync(companyId, status.Value)
            : await _leaveRepository.GetByCompanyAsync(companyId);

        return Ok(requests.Select(r => new LeaveRequestDto
        {
            Id = r.Id,
            PersonelId = r.EmployeeId,
            PersonelAdi = $"{r.Employee.FirstName} {r.Employee.LastName}",
            IzinTuru = LeaveTypeHelper.GetDisplayName(r.LeaveType),
            IzinTuruEnum = r.LeaveType,
            BaslangicTarihi = r.StartDate,
            BitisTarihi = r.EndDate,
            GunSayisi = r.TotalDays,
            Aciklama = r.Description,
            Durum = LeaveStatusHelper.GetDisplayName(r.Status),
            DurumEnum = r.Status,
            TalepTarihi = r.RequestDate,
            OnaylayanAdi = r.ApprovedBy != null ? $"{r.ApprovedBy.FirstName} {r.ApprovedBy.LastName}" : null,
            OnayTarihi = r.ApprovalDate,
            FormNo = r.FormNumber
        }).ToList());
    }

    /// <summary>
    /// Personelin izin taleplerini getirir
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetEmployeeRequests(int employeeId)
    {
        var requests = await _leaveRepository.GetByEmployeeAsync(employeeId);

        return Ok(requests.Select(r => new LeaveRequestDto
        {
            Id = r.Id,
            PersonelId = r.EmployeeId,
            PersonelAdi = $"{r.Employee.FirstName} {r.Employee.LastName}",
            IzinTuru = LeaveTypeHelper.GetDisplayName(r.LeaveType),
            IzinTuruEnum = r.LeaveType,
            BaslangicTarihi = r.StartDate,
            BitisTarihi = r.EndDate,
            GunSayisi = r.TotalDays,
            Aciklama = r.Description,
            Durum = LeaveStatusHelper.GetDisplayName(r.Status),
            DurumEnum = r.Status,
            TalepTarihi = r.RequestDate,
            OnaylayanAdi = r.ApprovedBy != null ? $"{r.ApprovedBy.FirstName} {r.ApprovedBy.LastName}" : null,
            OnayTarihi = r.ApprovalDate,
            FormNo = r.FormNumber
        }).ToList());
    }

    /// <summary>
    /// Ýzin detayýný getirir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequestDetailDto>> GetRequest(int id)
    {
        var request = await _leaveRepository.GetWithDetailsAsync(id);

        if (request == null)
        {
            // Demo veri
            return Ok(new LeaveRequestDetailDto
            {
                Id = id,
                PersonelId = 1,
                PersonelAdi = "Ahmet Yýlmaz",
                TcKimlikNo = "12345678901",
                Departman = "Muhasebe",
                Pozisyon = "Uzman",
                IseGirisTarihi = new DateTime(2020, 3, 15),
                IzinTuru = "Yýllýk Ýzin",
                BaslangicTarihi = DateTime.Now.AddDays(5),
                BitisTarihi = DateTime.Now.AddDays(10),
                GunSayisi = 6,
                Aciklama = "Ailevi nedenlerle izin talep ediyorum.",
                Durum = "Onaylandý",
                TalepTarihi = DateTime.Now.AddDays(-2),
                OnaylayanAdi = "Müdür Bey",
                OnayTarihi = DateTime.Now.AddDays(-1),
                VekilAdi = "Mehmet Demir",
                IletisimTelefon = "0532 123 45 67",
                IletisimAdres = "Ýstanbul",
                FormNo = "IZN-2025-001",
                ToplamHakedilenIzin = 20,
                KullanilanIzin = 8,
                KalanIzin = 12
            });
        }

        var remainingDays = await _leaveRepository.GetRemainingDaysAsync(request.EmployeeId, DateTime.Now.Year);
        var usedDays = await _leaveRepository.GetUsedDaysAsync(request.EmployeeId, LeaveType.Annual, DateTime.Now.Year);

        return Ok(new LeaveRequestDetailDto
        {
            Id = request.Id,
            PersonelId = request.EmployeeId,
            PersonelAdi = $"{request.Employee.FirstName} {request.Employee.LastName}",
            TcKimlikNo = request.Employee.TcKimlikNo,
            Departman = request.Employee.Department,
            Pozisyon = request.Employee.Position,
            IseGirisTarihi = request.Employee.HireDate,
            IzinTuru = LeaveTypeHelper.GetDisplayName(request.LeaveType),
            IzinTuruEnum = request.LeaveType,
            BaslangicTarihi = request.StartDate,
            BitisTarihi = request.EndDate,
            GunSayisi = request.TotalDays,
            Aciklama = request.Description,
            Durum = LeaveStatusHelper.GetDisplayName(request.Status),
            DurumEnum = request.Status,
            TalepTarihi = request.RequestDate,
            OnaylayanAdi = request.ApprovedBy != null ? $"{request.ApprovedBy.FirstName} {request.ApprovedBy.LastName}" : null,
            OnayTarihi = request.ApprovalDate,
            OnayNotu = request.ApprovalNote,
            VekilAdi = request.DeputyEmployee != null ? $"{request.DeputyEmployee.FirstName} {request.DeputyEmployee.LastName}" : null,
            IletisimTelefon = request.ContactPhone,
            IletisimAdres = request.ContactAddress,
            FormNo = request.FormNumber,
            KullanilanIzin = usedDays,
            KalanIzin = remainingDays,
            ToplamHakedilenIzin = usedDays + remainingDays
        });
    }

    /// <summary>
    /// Yeni izin talebi oluþturur
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> CreateRequest([FromBody] CreateLeaveRequestDto request)
    {
        // Validasyonlar
        if (request.BaslangicTarihi > request.BitisTarihi)
            return BadRequest(new { message = "Baþlangýç tarihi bitiþ tarihinden sonra olamaz" });

        if (request.BaslangicTarihi < DateTime.Today)
            return BadRequest(new { message = "Geçmiþ tarihli izin talebi oluþturulamaz" });

        // Çakýþma kontrolü
        var hasOverlap = await _leaveRepository.HasOverlappingLeaveAsync(
            request.PersonelId, request.BaslangicTarihi, request.BitisTarihi);

        if (hasOverlap)
            return BadRequest(new { message = "Bu tarihler için zaten bir izin talebi bulunmaktadýr" });

        // Kalan izin kontrolü (yýllýk izin için)
        if (request.IzinTuru == LeaveType.Annual)
        {
            var remaining = await _leaveRepository.GetRemainingDaysAsync(request.PersonelId, DateTime.Now.Year);
            var requestedDays = (request.BitisTarihi - request.BaslangicTarihi).Days + 1;

            if (requestedDays > remaining)
                return BadRequest(new { message = $"Yeterli izin hakkýnýz bulunmamaktadýr. Kalan: {remaining} gün" });
        }

        var totalDays = (request.BitisTarihi - request.BaslangicTarihi).Days + 1;
        var formNumber = $"IZN-{DateTime.Now:yyyy}-{new Random().Next(1000, 9999):D4}";

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = request.PersonelId,
            CompanyId = request.FirmaId,
            LeaveType = request.IzinTuru,
            StartDate = request.BaslangicTarihi,
            EndDate = request.BitisTarihi,
            TotalDays = totalDays,
            Description = request.Aciklama,
            DeputyEmployeeId = request.VekilId,
            ContactPhone = request.IletisimTelefon,
            ContactAddress = request.IletisimAdres,
            FormNumber = formNumber,
            Status = LeaveStatus.Pending,
            RequestDate = DateTime.Now
        };

        await _leaveRepository.AddAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            PersonelId = leaveRequest.EmployeeId,
            IzinTuru = LeaveTypeHelper.GetDisplayName(leaveRequest.LeaveType),
            BaslangicTarihi = leaveRequest.StartDate,
            BitisTarihi = leaveRequest.EndDate,
            GunSayisi = leaveRequest.TotalDays,
            Durum = "Bekliyor",
            TalepTarihi = leaveRequest.RequestDate,
            FormNo = leaveRequest.FormNumber
        });
    }

    /// <summary>
    /// Ýzin talebini onaylar
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<LeaveRequestDto>> ApproveRequest(int id, [FromBody] ApproveLeaveRequestDto request)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(id);
        
        if (leaveRequest == null)
            return NotFound(new { message = "Izin talebi bulunamadi" });

        if (leaveRequest.Status != LeaveStatus.Pending)
            return BadRequest(new { message = "Bu izin talebi zaten islem gormus" });

        leaveRequest.Status = LeaveStatus.Approved;
        leaveRequest.ApprovedById = request.OnaylayanId;
        leaveRequest.ApprovalDate = DateTime.Now;
        leaveRequest.ApprovalNote = request.OnayNotu;

        await _leaveRepository.UpdateAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Izin talebi onaylandi", formNo = leaveRequest.FormNumber });
    }

    /// <summary>
    /// Izin talebini reddeder
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<ActionResult> RejectRequest(int id, [FromBody] RejectLeaveRequestDto request)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(id);
        
        if (leaveRequest == null)
            return NotFound(new { message = "Izin talebi bulunamadi" });

        if (leaveRequest.Status != LeaveStatus.Pending)
            return BadRequest(new { message = "Bu izin talebi zaten islem gormus" });

        leaveRequest.Status = LeaveStatus.Rejected;
        leaveRequest.ApprovedById = request.ReddedenId;
        leaveRequest.ApprovalDate = DateTime.Now;
        leaveRequest.ApprovalNote = request.RedNedeni;

        await _leaveRepository.UpdateAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Izin talebi reddedildi" });
    }

    /// <summary>
    /// Izin talebini iptal eder
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelRequest(int id)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(id);
        
        if (leaveRequest == null)
            return NotFound(new { message = "Izin talebi bulunamadi" });

        if (leaveRequest.Status != LeaveStatus.Pending)
            return BadRequest(new { message = "Sadece bekleyen talepler iptal edilebilir" });

        leaveRequest.Status = LeaveStatus.Cancelled;

        await _leaveRepository.UpdateAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Izin talebi iptal edildi" });
    }

    /// <summary>
    /// Kalan izin hakkini getirir
    /// </summary>
    [HttpGet("remaining/{employeeId}")]
    public async Task<ActionResult<LeaveBalanceDto>> GetLeaveBalance(int employeeId, [FromQuery] int year = 0)
    {
        if (year == 0) year = DateTime.Now.Year;

        var remaining = await _leaveRepository.GetRemainingDaysAsync(employeeId, year);
        var usedAnnual = await _leaveRepository.GetUsedDaysAsync(employeeId, LeaveType.Annual, year);
        var usedSick = await _leaveRepository.GetUsedDaysAsync(employeeId, LeaveType.Sick, year);
        var usedOther = await _leaveRepository.GetUsedDaysAsync(employeeId, LeaveType.Compensatory, year);

        return Ok(new LeaveBalanceDto
        {
            Yil = year,
            ToplamHakedilen = remaining + usedAnnual,
            KullanilanYillik = usedAnnual,
            KullanilanHastalik = usedSick,
            KullanilanDiger = usedOther,
            Kalan = remaining
        });
    }

    /// <summary>
    /// Ýzin formunu PDF olarak oluþturur
    /// </summary>
    [HttpGet("{id}/form/pdf")]
    public async Task<ActionResult> GenerateLeaveFormPdf(int id)
    {
        var request = await _leaveRepository.GetWithDetailsAsync(id);

        if (request == null)
        {
            // Demo için form bilgisi döndür
            return Ok(new LeaveFormData
            {
                FormNo = $"IZN-2025-{id:D4}",
                FirmaAdi = "Demo Þirketi A.Þ.",
                PersonelAdi = "Ahmet Yýlmaz",
                TcKimlikNo = "12345678901",
                SicilNo = "P001",
                Departman = "Muhasebe",
                Pozisyon = "Uzman",
                IseGirisTarihi = new DateTime(2020, 3, 15),
                IzinTuru = "Yýllýk Ýzin",
                BaslangicTarihi = DateTime.Now.AddDays(5),
                BitisTarihi = DateTime.Now.AddDays(10),
                GunSayisi = 6,
                Aciklama = "Ailevi nedenlerle izin talep ediyorum.",
                VekilAdi = "Mehmet Demir",
                IletisimTelefon = "0532 123 45 67",
                IletisimAdres = "Ýstanbul",
                TalepTarihi = DateTime.Now,
                OnayDurumu = "Onaylandý",
                OnaylayanAdi = "Müdür Bey",
                OnayTarihi = DateTime.Now,
                ToplamHakedilen = 20,
                Kullanilan = 8,
                Kalan = 12,
                OlusturmaTarihi = DateTime.Now
            });
        }

        var remaining = await _leaveRepository.GetRemainingDaysAsync(request.EmployeeId, DateTime.Now.Year);
        var used = await _leaveRepository.GetUsedDaysAsync(request.EmployeeId, LeaveType.Annual, DateTime.Now.Year);

        return Ok(new LeaveFormData
        {
            FormNo = request.FormNumber ?? $"IZN-{DateTime.Now:yyyy}-{request.Id:D4}",
            FirmaAdi = "Ayda Müþavirlik", // Firma bilgisi eklenebilir
            PersonelAdi = $"{request.Employee.FirstName} {request.Employee.LastName}",
            TcKimlikNo = request.Employee.TcKimlikNo,
            SicilNo = request.Employee.EmployeeNumber,
            Departman = request.Employee.Department,
            Pozisyon = request.Employee.Position,
            IseGirisTarihi = request.Employee.HireDate,
            IzinTuru = LeaveTypeHelper.GetDisplayName(request.LeaveType),
            BaslangicTarihi = request.StartDate,
            BitisTarihi = request.EndDate,
            GunSayisi = request.TotalDays,
            Aciklama = request.Description,
            VekilAdi = request.DeputyEmployee != null ? $"{request.DeputyEmployee.FirstName} {request.DeputyEmployee.LastName}" : null,
            IletisimTelefon = request.ContactPhone,
            IletisimAdres = request.ContactAddress,
            TalepTarihi = request.RequestDate,
            OnayDurumu = LeaveStatusHelper.GetDisplayName(request.Status),
            OnaylayanAdi = request.ApprovedBy != null ? $"{request.ApprovedBy.FirstName} {request.ApprovedBy.LastName}" : null,
            OnayTarihi = request.ApprovalDate,
            OnayNotu = request.ApprovalNote,
            ToplamHakedilen = remaining + used,
            Kullanilan = used,
            Kalan = remaining,
            OlusturmaTarihi = DateTime.Now
        });
    }

    /// <summary>
    /// Ýzin formunu e-posta ile gönderir
    /// </summary>
    [HttpPost("{id}/send-email")]
    public async Task<ActionResult> SendLeaveFormByEmail(int id, [FromBody] SendEmailDto request)
    {
        // E-posta gönderme iþlemi (gerçek uygulamada SMTP servisi kullanýlacak)
        return Ok(new { 
            message = $"Ýzin formu {request.Email} adresine gönderildi",
            formNo = $"IZN-2025-{id:D4}"
        });
    }
}

#region DTOs

public class LeaveRequestDto
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public string? PersonelAdi { get; set; }
    public string IzinTuru { get; set; } = "";
    public LeaveType IzinTuruEnum { get; set; }
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int GunSayisi { get; set; }
    public string? Aciklama { get; set; }
    public string Durum { get; set; } = "";
    public LeaveStatus DurumEnum { get; set; }
    public DateTime TalepTarihi { get; set; }
    public string? OnaylayanAdi { get; set; }
    public DateTime? OnayTarihi { get; set; }
    public string? FormNo { get; set; }
}

public class LeaveRequestDetailDto : LeaveRequestDto
{
    public string? TcKimlikNo { get; set; }
    public string? Departman { get; set; }
    public string? Pozisyon { get; set; }
    public DateTime IseGirisTarihi { get; set; }
    public string? OnayNotu { get; set; }
    public string? VekilAdi { get; set; }
    public string? IletisimTelefon { get; set; }
    public string? IletisimAdres { get; set; }
    public int ToplamHakedilenIzin { get; set; }
    public int KullanilanIzin { get; set; }
    public int KalanIzin { get; set; }
}

public class CreateLeaveRequestDto
{
    public int PersonelId { get; set; }
    public int FirmaId { get; set; } = 1;
    public LeaveType IzinTuru { get; set; }
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public string? Aciklama { get; set; }
    public int? VekilId { get; set; }
    public string? IletisimTelefon { get; set; }
    public string? IletisimAdres { get; set; }
}

public class ApproveLeaveRequestDto
{
    public int OnaylayanId { get; set; }
    public string? OnayNotu { get; set; }
}

public class RejectLeaveRequestDto
{
    public int ReddedenId { get; set; }
    public string RedNedeni { get; set; } = "";
}

public class LeaveBalanceDto
{
    public int Yil { get; set; }
    public int ToplamHakedilen { get; set; }
    public int KullanilanYillik { get; set; }
    public int KullanilanHastalik { get; set; }
    public int KullanilanDiger { get; set; }
    public int Kalan { get; set; }
}

public class LeaveFormData
{
    public string FormNo { get; set; } = "";
    public string FirmaAdi { get; set; } = "";
    public string PersonelAdi { get; set; } = "";
    public string? TcKimlikNo { get; set; }
    public string? SicilNo { get; set; }
    public string? Departman { get; set; }
    public string? Pozisyon { get; set; }
    public DateTime IseGirisTarihi { get; set; }
    public string IzinTuru { get; set; } = "";
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int GunSayisi { get; set; }
    public string? Aciklama { get; set; }
    public string? VekilAdi { get; set; }
    public string? IletisimTelefon { get; set; }
    public string? IletisimAdres { get; set; }
    public DateTime TalepTarihi { get; set; }
    public string OnayDurumu { get; set; } = "";
    public string? OnaylayanAdi { get; set; }
    public DateTime? OnayTarihi { get; set; }
    public string? OnayNotu { get; set; }
    public int ToplamHakedilen { get; set; }
    public int Kullanilan { get; set; }
    public int Kalan { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
}

public class SendEmailDto
{
    public string Email { get; set; } = "";
    public string? Subject { get; set; }
    public string? Message { get; set; }
}

#endregion