using Microsoft.AspNetCore.Mvc;
using AydaMusavirlik.Infrastructure.Services;

namespace AydaMusavirlik.Api.Controllers;

/// <summary>
/// Raporlama API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Bordro raporu (Excel)
    /// </summary>
    [HttpGet("payroll/excel")]
    public ActionResult GetPayrollExcel([FromQuery] int year, [FromQuery] int month)
    {
        var period = $"{year}/{month:D2}";

        // Demo veri
        var items = GenerateDemoPayrollData();

        var excelBytes = _reportService.GeneratePayrollExcelReport(items, period);

        return File(excelBytes, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Bordro_{period.Replace("/", "-")}.xlsx");
    }

    /// <summary>
    /// Bordro raporu (PDF)
    /// </summary>
    [HttpGet("payroll/pdf")]
    public ActionResult GetPayrollPdf([FromQuery] int year, [FromQuery] int month, [FromQuery] string? companyName = "Ayda Musavirlik")
    {
        var period = $"{year}/{month:D2}";

        var items = GenerateDemoPayrollData();

        var pdfBytes = _reportService.GeneratePayrollPdfReport(items, period, companyName ?? "Ayda Musavirlik");

        return File(pdfBytes, "application/pdf", $"Bordro_{period.Replace("/", "-")}.pdf");
    }

    /// <summary>
    /// Personel listesi (Excel)
    /// </summary>
    [HttpGet("employees/excel")]
    public ActionResult GetEmployeeListExcel()
    {
        var employees = GenerateDemoEmployeeData();

        var excelBytes = _reportService.GenerateEmployeeListExcel(employees);

        return File(excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"PersonelListesi_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    /// <summary>
    /// Personel listesi (PDF)
    /// </summary>
    [HttpGet("employees/pdf")]
    public ActionResult GetEmployeeListPdf([FromQuery] string? companyName = "Ayda Musavirlik")
    {
        var employees = GenerateDemoEmployeeData();

        var pdfBytes = _reportService.GenerateEmployeeListPdf(employees, companyName ?? "Ayda Musavirlik");

        return File(pdfBytes, "application/pdf", $"PersonelListesi_{DateTime.Now:yyyyMMdd}.pdf");
    }

    /// <summary>
    /// Hesap ekstresi (Excel)
    /// </summary>
    [HttpGet("account-statement/excel")]
    public ActionResult GetAccountStatementExcel(
        [FromQuery] string accountName,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var items = GenerateDemoAccountStatementData();

        var excelBytes = _reportService.GenerateAccountStatementExcel(items, accountName, startDate, endDate);

        return File(excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"HesapEkstresi_{accountName}_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.xlsx");
    }

    /// <summary>
    /// Mali ozet raporu (PDF)
    /// </summary>
    [HttpGet("financial-summary/pdf")]
    public ActionResult GetFinancialSummaryPdf([FromQuery] string? companyName = "Ayda Musavirlik", [FromQuery] string? period = null)
    {
        period ??= $"{DateTime.Now.Year}";

        var report = new FinancialSummaryReport
        {
            CompanyName = companyName ?? "Ayda Musavirlik",
            Period = period,
            ToplamGelir = 1250000,
            ToplamGider = 980000,
            Ratios = new List<FinancialRatio>
            {
                new() { Name = "Cari Oran", Value = 1.85m, Status = "Iyi" },
                new() { Name = "Likidite Orani", Value = 1.42m, Status = "Iyi" },
                new() { Name = "Borc/Ozkaynak", Value = 0.65m, Status = "Normal" },
                new() { Name = "Kar Marji", Value = 0.216m, Status = "Iyi" }
            }
        };

        var pdfBytes = _reportService.GenerateFinancialSummaryPdf(report);

        return File(pdfBytes, "application/pdf", $"MaliOzet_{period}.pdf");
    }

    /// <summary>
    /// Aylik rapor (PDF)
    /// </summary>
    [HttpGet("monthly/pdf")]
    public ActionResult GetMonthlyReportPdf(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string? companyName = "Ayda Musavirlik")
    {
        var report = new MonthlyReport
        {
            CompanyName = companyName ?? "Ayda Musavirlik",
            Year = year,
            Month = month,
            FisSayisi = 127,
            Tahsilat = 485000,
            Odeme = 312000,
            PersonelSayisi = 25,
            BordroToplam = 187500,
            Notlar = "Aylik mali islemler basariyla tamamlanmistir."
        };

        var pdfBytes = _reportService.GenerateMonthlyReportPdf(report);

        return File(pdfBytes, "application/pdf", $"AylikRapor_{year}-{month:D2}.pdf");
    }

    /// <summary>
    /// Mevcut rapor turleri
    /// </summary>
    [HttpGet("types")]
    public ActionResult GetReportTypes()
    {
        return Ok(new[]
        {
            new { Id = "payroll-excel", Name = "Bordro Raporu (Excel)", Category = "Bordro" },
            new { Id = "payroll-pdf", Name = "Bordro Raporu (PDF)", Category = "Bordro" },
            new { Id = "employees-excel", Name = "Personel Listesi (Excel)", Category = "Personel" },
            new { Id = "employees-pdf", Name = "Personel Listesi (PDF)", Category = "Personel" },
            new { Id = "account-statement", Name = "Hesap Ekstresi", Category = "Muhasebe" },
            new { Id = "financial-summary", Name = "Mali Ozet", Category = "Mali" },
            new { Id = "monthly-report", Name = "Aylik Rapor", Category = "Genel" }
        });
    }

    #region Demo Data Generators

    private List<PayrollReportItem> GenerateDemoPayrollData()
    {
        return new List<PayrollReportItem>
        {
            new() { SicilNo = "P001", AdSoyad = "Ahmet Yilmaz", Departman = "Muhasebe", BrutMaas = 35000, SgkIsci = 4900, Issizlik = 350, GelirVergisi = 4500, DamgaVergisi = 266, ToplamKesinti = 10016, NetMaas = 24984, SgkIsveren = 7175, ToplamMaliyet = 42175 },
            new() { SicilNo = "P002", AdSoyad = "Mehmet Demir", Departman = "IT", BrutMaas = 45000, SgkIsci = 6300, Issizlik = 450, GelirVergisi = 6750, DamgaVergisi = 342, ToplamKesinti = 13842, NetMaas = 31158, SgkIsveren = 9225, ToplamMaliyet = 54225 },
            new() { SicilNo = "P003", AdSoyad = "Ayse Kaya", Departman = "IK", BrutMaas = 30000, SgkIsci = 4200, Issizlik = 300, GelirVergisi = 3600, DamgaVergisi = 228, ToplamKesinti = 8328, NetMaas = 21672, SgkIsveren = 6150, ToplamMaliyet = 36150 },
            new() { SicilNo = "P004", AdSoyad = "Fatma Celik", Departman = "Satis", BrutMaas = 28000, SgkIsci = 3920, Issizlik = 280, GelirVergisi = 3200, DamgaVergisi = 213, ToplamKesinti = 7613, NetMaas = 20387, SgkIsveren = 5740, ToplamMaliyet = 33740 },
            new() { SicilNo = "P005", AdSoyad = "Ali Yildiz", Departman = "Uretim", BrutMaas = 25000, SgkIsci = 3500, Issizlik = 250, GelirVergisi = 2750, DamgaVergisi = 190, ToplamKesinti = 6690, NetMaas = 18310, SgkIsveren = 5125, ToplamMaliyet = 30125 }
        };
    }

    private List<EmployeeReportItem> GenerateDemoEmployeeData()
    {
        return new List<EmployeeReportItem>
        {
            new() { SicilNo = "P001", TcKimlik = "12345678901", AdSoyad = "Ahmet Yilmaz", Departman = "Muhasebe", Pozisyon = "Uzman", IseGiris = new DateTime(2020, 3, 15), BrutMaas = 35000, Aktif = true, Telefon = "0532 123 4567", Email = "ahmet@firma.com" },
            new() { SicilNo = "P002", TcKimlik = "23456789012", AdSoyad = "Mehmet Demir", Departman = "IT", Pozisyon = "Yazilimci", IseGiris = new DateTime(2019, 6, 1), BrutMaas = 45000, Aktif = true, Telefon = "0533 234 5678", Email = "mehmet@firma.com" },
            new() { SicilNo = "P003", TcKimlik = "34567890123", AdSoyad = "Ayse Kaya", Departman = "IK", Pozisyon = "Uzman", IseGiris = new DateTime(2021, 1, 10), BrutMaas = 30000, Aktif = true, Telefon = "0534 345 6789", Email = "ayse@firma.com" },
            new() { SicilNo = "P004", TcKimlik = "45678901234", AdSoyad = "Fatma Celik", Departman = "Satis", Pozisyon = "Temsilci", IseGiris = new DateTime(2022, 4, 20), BrutMaas = 28000, Aktif = true, Telefon = "0535 456 7890", Email = "fatma@firma.com" },
            new() { SicilNo = "P005", TcKimlik = "56789012345", AdSoyad = "Ali Yildiz", Departman = "Uretim", Pozisyon = "Operator", IseGiris = new DateTime(2018, 9, 5), BrutMaas = 25000, Aktif = true, Telefon = "0536 567 8901", Email = "ali@firma.com" }
        };
    }

    private List<AccountStatementItem> GenerateDemoAccountStatementData()
    {
        return new List<AccountStatementItem>
        {
            new() { Tarih = DateTime.Now.AddDays(-30), FisNo = "MF-001", Aciklama = "Acilis bakiyesi", Borc = 50000, Alacak = 0, BelgeNo = "AB-001" },
            new() { Tarih = DateTime.Now.AddDays(-25), FisNo = "MF-002", Aciklama = "Mal alimi", Borc = 25000, Alacak = 0, BelgeNo = "FAT-001" },
            new() { Tarih = DateTime.Now.AddDays(-20), FisNo = "TF-001", Aciklama = "Tahsilat", Borc = 0, Alacak = 30000, BelgeNo = "MAK-001" },
            new() { Tarih = DateTime.Now.AddDays(-15), FisNo = "MF-003", Aciklama = "Hizmet alimi", Borc = 8000, Alacak = 0, BelgeNo = "FAT-002" },
            new() { Tarih = DateTime.Now.AddDays(-10), FisNo = "TF-002", Aciklama = "Tahsilat", Borc = 0, Alacak = 25000, BelgeNo = "MAK-002" },
            new() { Tarih = DateTime.Now.AddDays(-5), FisNo = "MF-004", Aciklama = "Mal alimi", Borc = 15000, Alacak = 0, BelgeNo = "FAT-003" }
        };
    }

    #endregion
}