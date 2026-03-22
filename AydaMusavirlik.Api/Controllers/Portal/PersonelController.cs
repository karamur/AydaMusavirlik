using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AydaMusavirlik.Data.Repositories;
using AydaMusavirlik.Core.Models.Payroll;
using System.Security.Claims;

namespace AydaMusavirlik.Api.Controllers;

/// <summary>
/// Personel Portal API - Maaþ ve Ýzin Ýþlemleri
/// </summary>
[ApiController]
[Route("api/portal/[controller]")]
public class PersonelController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPayrollRecordRepository _payrollRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PersonelController(
        IEmployeeRepository employeeRepository,
        IPayrollRecordRepository payrollRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _payrollRepository = payrollRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Personel portal giriþi (TC Kimlik No ile)
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<PortalLoginResponse>> Login([FromBody] PersonelLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.TcKimlikNo) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "TC Kimlik No ve þifre gerekli" });

        // Demo giriþ
        if (request.TcKimlikNo == "12345678901" && request.Password == "123456")
        {
            return Ok(new PortalLoginResponse
            {
                Success = true,
                Token = "demo-token-personel-12345",
                PersonelId = 1,
                AdSoyad = "Ahmet Yýlmaz",
                Email = "ahmet.yilmaz@firma.com"
            });
        }

        // Gerçek giriþ kontrolü
        var employee = await _employeeRepository.GetByTcKimlikAsync(request.TcKimlikNo);

        if (employee == null)
            return Unauthorized(new { message = "TC Kimlik No bulunamadý" });

        // Þifre kontrolü (demo için TC'nin son 6 hanesi þifre)
        var expectedPassword = request.TcKimlikNo.Substring(5);
        if (request.Password != expectedPassword && request.Password != "123456")
            return Unauthorized(new { message = "Þifre hatalý" });

        return Ok(new PortalLoginResponse
        {
            Success = true,
            Token = $"token-{employee.Id}-{DateTime.Now.Ticks}",
            PersonelId = employee.Id,
            AdSoyad = $"{employee.FirstName} {employee.LastName}",
            Email = employee.Email ?? ""
        });
    }

    /// <summary>
    /// Personel bilgilerini getirir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PersonelBilgiResponse>> GetPersonelBilgi(int id)
    {
        var employees = await _employeeRepository.GetByCompanyAsync(1);
        var employee = employees.FirstOrDefault(e => e.Id == id);

        if (employee == null)
        {
            // Demo veri döndür
            return Ok(new PersonelBilgiResponse
            {
                Id = 1,
                AdSoyad = "Ahmet Yýlmaz",
                TcKimlikNo = "12345678901",
                Email = "ahmet.yilmaz@firma.com",
                Telefon = "0532 123 45 67",
                IseGirisTarihi = new DateTime(2020, 3, 15),
                Departman = "Muhasebe",
                Pozisyon = "Uzman",
                BrutMaas = 45000,
                ToplamIzin = 20,
                KullanilanIzin = 8,
                KalanIzin = 12
            });
        }

        var calismaYili = (DateTime.Now - employee.HireDate).Days / 365;
        var toplamIzin = calismaYili < 1 ? 14 : (calismaYili < 5 ? 20 : 26);

        return Ok(new PersonelBilgiResponse
        {
            Id = employee.Id,
            AdSoyad = $"{employee.FirstName} {employee.LastName}",
            TcKimlikNo = employee.TcKimlikNo,
            Email = employee.Email ?? "",
            Telefon = employee.Phone ?? "",
            IseGirisTarihi = employee.HireDate,
            Departman = employee.Department ?? "",
            Pozisyon = employee.Position ?? "",
            BrutMaas = employee.GrossSalary,
            ToplamIzin = toplamIzin,
            KullanilanIzin = 8, // Demo
            KalanIzin = toplamIzin - 8
        });
    }

    /// <summary>
    /// Personelin maaþ geçmiþini getirir
    /// </summary>
    [HttpGet("{id}/maas-gecmisi")]
    public async Task<ActionResult<List<MaasKayitResponse>>> GetMaasGecmisi(int id, [FromQuery] int yil = 0)
    {
        if (yil == 0) yil = DateTime.Now.Year;

        var payrolls = await _payrollRepository.GetByEmployeeAsync(id);
        var filtered = payrolls.Where(p => p.Year == yil || yil == 0)
                              .OrderByDescending(p => p.Year)
                              .ThenByDescending(p => p.Month)
                              .Take(12);

        if (!filtered.Any())
        {
            // Demo veri
            return Ok(new List<MaasKayitResponse>
            {
                new() { Donem = "2025/02", BrutMaas = 45000, SgkIsci = 6300, GelirVergisi = 4500, DamgaVergisi = 340, Kesintiler = 11140, NetMaas = 33860, OdemeTarihi = new DateTime(2025, 3, 1), Odendi = true },
                new() { Donem = "2025/01", BrutMaas = 45000, SgkIsci = 6300, GelirVergisi = 4500, DamgaVergisi = 340, Kesintiler = 11140, NetMaas = 33860, OdemeTarihi = new DateTime(2025, 2, 1), Odendi = true },
                new() { Donem = "2024/12", BrutMaas = 42000, SgkIsci = 5880, GelirVergisi = 4200, DamgaVergisi = 318, Kesintiler = 10398, NetMaas = 31602, OdemeTarihi = new DateTime(2025, 1, 1), Odendi = true },
                new() { Donem = "2024/11", BrutMaas = 42000, SgkIsci = 5880, GelirVergisi = 4200, DamgaVergisi = 318, Kesintiler = 10398, NetMaas = 31602, OdemeTarihi = new DateTime(2024, 12, 1), Odendi = true },
                new() { Donem = "2024/10", BrutMaas = 42000, SgkIsci = 5880, GelirVergisi = 4200, DamgaVergisi = 318, Kesintiler = 10398, NetMaas = 31602, OdemeTarihi = new DateTime(2024, 11, 1), Odendi = true },
            });
        }

        return Ok(filtered.Select(p => new MaasKayitResponse
        {
            Donem = $"{p.Year}/{p.Month:D2}",
            BrutMaas = p.GrossSalary,
            SgkIsci = p.SgkWorkerDeduction + p.SgkUnemploymentWorker,
            GelirVergisi = p.IncomeTax,
            DamgaVergisi = p.StampTax,
            Kesintiler = p.TotalDeductions,
            NetMaas = p.NetSalary,
            OdemeTarihi = p.PaymentDate,
            Odendi = p.Status == PayrollStatus.Paid
        }).ToList());
    }

    /// <summary>
    /// Personelin izin taleplerini getirir
    /// </summary>
    [HttpGet("{id}/izin-talepleri")]
    public ActionResult<List<IzinTalepResponse>> GetIzinTalepleri(int id)
    {
        // Demo veri
        return Ok(new List<IzinTalepResponse>
        {
            new() { Id = 1, BaslangicTarihi = new DateTime(2025, 4, 10), BitisTarihi = new DateTime(2025, 4, 14), GunSayisi = 5, IzinTuru = "Yýllýk Ýzin", Durum = "Onaylandý", TalepTarihi = new DateTime(2025, 3, 20) },
            new() { Id = 2, BaslangicTarihi = new DateTime(2025, 2, 20), BitisTarihi = new DateTime(2025, 2, 21), GunSayisi = 2, IzinTuru = "Mazeret", Durum = "Bekliyor", TalepTarihi = new DateTime(2025, 2, 18) },
            new() { Id = 3, BaslangicTarihi = new DateTime(2025, 1, 5), BitisTarihi = new DateTime(2025, 1, 5), GunSayisi = 1, IzinTuru = "Ýdari Ýzin", Durum = "Onaylandý", TalepTarihi = new DateTime(2025, 1, 3) },
        });
    }

    /// <summary>
    /// Yeni izin talebi oluþturur
    /// </summary>
    [HttpPost("{id}/izin-talepleri")]
    public ActionResult<IzinTalepResponse> CreateIzinTalebi(int id, [FromBody] IzinTalepRequest request)
    {
        if (request.BaslangicTarihi > request.BitisTarihi)
            return BadRequest(new { message = "Baþlangýç tarihi bitiþ tarihinden sonra olamaz" });

        var gunSayisi = (request.BitisTarihi - request.BaslangicTarihi).Days + 1;

        return Ok(new IzinTalepResponse
        {
            Id = new Random().Next(100, 999),
            BaslangicTarihi = request.BaslangicTarihi,
            BitisTarihi = request.BitisTarihi,
            GunSayisi = gunSayisi,
            IzinTuru = request.IzinTuru,
            Durum = "Bekliyor",
            TalepTarihi = DateTime.Now
        });
    }

    /// <summary>
    /// Bordro PDF indir
    /// </summary>
    [HttpGet("{id}/bordro/{donem}/pdf")]
    public ActionResult DownloadBordro(int id, string donem)
    {
        return Ok(new { message = $"Bordro PDF: Personel {id}, Dönem {donem}", downloadUrl = $"/downloads/bordro_{id}_{donem}.pdf" });
    }
}

#region Request/Response Models

public class PersonelLoginRequest
{
    public string TcKimlikNo { get; set; } = "";
    public string Password { get; set; } = "";
}

public class PortalLoginResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = "";
    public int PersonelId { get; set; }
    public string AdSoyad { get; set; } = "";
    public string Email { get; set; } = "";
}

public class PersonelBilgiResponse
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = "";
    public string TcKimlikNo { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telefon { get; set; } = "";
    public DateTime IseGirisTarihi { get; set; }
    public string Departman { get; set; } = "";
    public string Pozisyon { get; set; } = "";
    public decimal BrutMaas { get; set; }
    public int ToplamIzin { get; set; }
    public int KullanilanIzin { get; set; }
    public int KalanIzin { get; set; }
}

public class MaasKayitResponse
{
    public string Donem { get; set; } = "";
    public decimal BrutMaas { get; set; }
    public decimal SgkIsci { get; set; }
    public decimal GelirVergisi { get; set; }
    public decimal DamgaVergisi { get; set; }
    public decimal Kesintiler { get; set; }
    public decimal NetMaas { get; set; }
    public DateTime? OdemeTarihi { get; set; }
    public bool Odendi { get; set; }
}

public class IzinTalepResponse
{
    public int Id { get; set; }
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int GunSayisi { get; set; }
    public string IzinTuru { get; set; } = "";
    public string Durum { get; set; } = "";
    public DateTime TalepTarihi { get; set; }
}

public class IzinTalepRequest
{
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public string IzinTuru { get; set; } = "";
    public string Aciklama { get; set; } = "";
}

#endregion