using Microsoft.AspNetCore.Mvc;
using AydaMusavirlik.Data.Repositories;

namespace AydaMusavirlik.Api.Controllers;

/// <summary>
/// Dashboard API - Özet veriler ve grafikler
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountingRecordRepository _recordRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPayrollRecordRepository _payrollRepository;

    public DashboardController(
        ICompanyRepository companyRepository,
        IAccountRepository accountRepository,
        IAccountingRecordRepository recordRepository,
        IEmployeeRepository employeeRepository,
        IPayrollRecordRepository payrollRepository)
    {
        _companyRepository = companyRepository;
        _accountRepository = accountRepository;
        _recordRepository = recordRepository;
        _employeeRepository = employeeRepository;
        _payrollRepository = payrollRepository;
    }

    /// <summary>
    /// Dashboard özet verilerini getirir
    /// </summary>
    [HttpGet("ozet")]
    public async Task<ActionResult<DashboardOzetResponse>> GetOzet([FromQuery] int firmaId = 1, [FromQuery] int yil = 0, [FromQuery] int ay = 0)
    {
        if (yil == 0) yil = DateTime.Now.Year;
        if (ay == 0) ay = DateTime.Now.Month;

        try
        {
            var employees = await _employeeRepository.GetActiveEmployeesAsync(firmaId);
            var payrolls = await _payrollRepository.GetByPeriodAsync(firmaId, yil, ay);

            // Demo veriler
            var toplamGelir = 1250000m;
            var toplamGider = 980000m;
            var netKar = toplamGelir - toplamGider;
            var oncekiAyGelir = 1150000m;
            var oncekiAyGider = 920000m;

            var gelirDegisim = oncekiAyGelir > 0 ? ((toplamGelir - oncekiAyGelir) / oncekiAyGelir) * 100 : 0;
            var giderDegisim = oncekiAyGider > 0 ? ((toplamGider - oncekiAyGider) / oncekiAyGider) * 100 : 0;
            var karMarji = toplamGelir > 0 ? (netKar / toplamGelir) * 100 : 0;

            var maliSaglik = HesaplaMaliSaglik(karMarji, 2.5m, 0.4m);

            return Ok(new DashboardOzetResponse
            {
                ToplamGelir = toplamGelir,
                ToplamGider = toplamGider,
                NetKar = netKar,
                KarMarji = karMarji,
                GelirDegisimYuzdesi = gelirDegisim,
                GiderDegisimYuzdesi = giderDegisim,
                PersonelSayisi = employees.Count(),
                ToplamBordro = payrolls.Sum(p => p.NetSalary),
                MaliSaglikPuani = maliSaglik,
                MaliSaglikSeviye = GetMaliSaglikSeviye(maliSaglik),
                Donem = $"{yil}/{ay:D2}"
            });
        }
        catch
        {
            return Ok(new DashboardOzetResponse
            {
                ToplamGelir = 1250000,
                ToplamGider = 980000,
                NetKar = 270000,
                KarMarji = 21.6m,
                GelirDegisimYuzdesi = 8.7m,
                GiderDegisimYuzdesi = 6.5m,
                PersonelSayisi = 12,
                ToplamBordro = 185000,
                MaliSaglikPuani = 78,
                MaliSaglikSeviye = "Ýyi",
                Donem = $"{yil}/{ay:D2}"
            });
        }
    }

    /// <summary>
    /// Mali oranlarý getirir
    /// </summary>
    [HttpGet("mali-oranlar")]
    public ActionResult<MaliOranlarResponse> GetMaliOranlar([FromQuery] int firmaId = 1)
    {
        return Ok(new MaliOranlarResponse
        {
            CariOran = 2.50m,
            AsitTestOrani = 1.80m,
            BorcOzkaynakOrani = 0.40m,
            ROE = 20.0m,
            ROA = 12.0m,
            BrutKarMarji = 35.0m,
            NetKarMarji = 21.6m,
            StokDevirHizi = 8.5m,
            AlacakTahsilSuresi = 45,
            BorcOdemeSuresi = 30
        });
    }

    /// <summary>
    /// Aylýk gelir/gider trend verilerini getirir
    /// </summary>
    [HttpGet("trend")]
    public ActionResult<TrendResponse> GetTrend([FromQuery] int firmaId = 1, [FromQuery] int yil = 0)
    {
        if (yil == 0) yil = DateTime.Now.Year;

        return Ok(new TrendResponse
        {
            Yil = yil,
            Aylar = new[] { "Oca", "Þub", "Mar", "Nis", "May", "Haz", "Tem", "Aðu", "Eyl", "Eki", "Kas", "Ara" },
            Gelirler = new[] { 95000m, 102000m, 98000m, 110000m, 105000m, 115000m, 108000m, 120000m, 118000m, 125000m, 130000m, 128000m },
            Giderler = new[] { 75000m, 80000m, 78000m, 85000m, 82000m, 88000m, 84000m, 92000m, 90000m, 95000m, 98000m, 96000m }
        });
    }

    /// <summary>
    /// Gider daðýlýmýný getirir
    /// </summary>
    [HttpGet("gider-dagilimi")]
    public ActionResult<List<GiderDagilimiItem>> GetGiderDagilimi([FromQuery] int firmaId = 1)
    {
        return Ok(new List<GiderDagilimiItem>
        {
            new() { Kategori = "Personel", Tutar = 350000, Yuzde = 35 },
            new() { Kategori = "Kira", Tutar = 200000, Yuzde = 20 },
            new() { Kategori = "Enerji", Tutar = 150000, Yuzde = 15 },
            new() { Kategori = "Hammadde", Tutar = 120000, Yuzde = 12 },
            new() { Kategori = "Pazarlama", Tutar = 100000, Yuzde = 10 },
            new() { Kategori = "Diðer", Tutar = 80000, Yuzde = 8 }
        });
    }

    /// <summary>
    /// Son iþlemleri getirir
    /// </summary>
    [HttpGet("son-islemler")]
    public ActionResult<List<SonIslemItem>> GetSonIslemler([FromQuery] int firmaId = 1, [FromQuery] int adet = 10)
    {
        return Ok(new List<SonIslemItem>
        {
            new() { Aciklama = "Personel maaþ ödemesi", Tarih = DateTime.Now.AddHours(-2), Tutar = -125000, Tip = "Ödeme" },
            new() { Aciklama = "ABC Ltd. tahsilat", Tarih = DateTime.Now.AddDays(-1), Tutar = 45000, Tip = "Tahsilat" },
            new() { Aciklama = "Kira ödemesi", Tarih = DateTime.Now.AddDays(-3), Tutar = -35000, Tip = "Ödeme" },
            new() { Aciklama = "XYZ A.Þ. fatura", Tarih = DateTime.Now.AddDays(-4), Tutar = 78500, Tip = "Tahsilat" },
            new() { Aciklama = "SGK ödemesi", Tarih = DateTime.Now.AddDays(-5), Tutar = -42000, Tip = "Ödeme" },
        });
    }

    /// <summary>
    /// Hatýrlatmalarý getirir
    /// </summary>
    [HttpGet("hatirlatmalar")]
    public ActionResult<List<HatirlatmaItem>> GetHatirlatmalar([FromQuery] int firmaId = 1)
    {
        return Ok(new List<HatirlatmaItem>
        {
            new() { Baslik = "KDV Beyannamesi", Aciklama = "Mart 2025 KDV beyannamesi son gün", Tarih = DateTime.Now.AddDays(5), Oncelik = "Yüksek", Tip = "Vergi" },
            new() { Baslik = "SGK Bildirge", Aciklama = "SGK aylýk bildirge son gün", Tarih = DateTime.Now.AddDays(10), Oncelik = "Yüksek", Tip = "SGK" },
            new() { Baslik = "Muhtasar", Aciklama = "Muhtasar beyanname son gün", Tarih = DateTime.Now.AddDays(15), Oncelik = "Normal", Tip = "Vergi" },
        });
    }

    private int HesaplaMaliSaglik(decimal karMarji, decimal cariOran, decimal borcOzkaynak)
    {
        var puan = 0;
        if (karMarji >= 20) puan += 40;
        else if (karMarji >= 10) puan += 30;
        else if (karMarji >= 5) puan += 20;
        else if (karMarji >= 0) puan += 10;

        if (cariOran >= 2) puan += 30;
        else if (cariOran >= 1.5m) puan += 25;
        else if (cariOran >= 1) puan += 15;
        else puan += 5;

        if (borcOzkaynak <= 0.5m) puan += 30;
        else if (borcOzkaynak <= 1) puan += 20;
        else if (borcOzkaynak <= 1.5m) puan += 10;

        return Math.Min(100, puan);
    }

    private string GetMaliSaglikSeviye(int puan) => puan switch
    {
        >= 80 => "Mükemmel",
        >= 60 => "Ýyi",
        >= 40 => "Orta",
        >= 20 => "Zayýf",
        _ => "Kritik"
    };
}

#region Response Models

public class DashboardOzetResponse
{
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetKar { get; set; }
    public decimal KarMarji { get; set; }
    public decimal GelirDegisimYuzdesi { get; set; }
    public decimal GiderDegisimYuzdesi { get; set; }
    public int PersonelSayisi { get; set; }
    public decimal ToplamBordro { get; set; }
    public int MaliSaglikPuani { get; set; }
    public string MaliSaglikSeviye { get; set; } = "";
    public string Donem { get; set; } = "";
}

public class MaliOranlarResponse
{
    public decimal CariOran { get; set; }
    public decimal AsitTestOrani { get; set; }
    public decimal BorcOzkaynakOrani { get; set; }
    public decimal ROE { get; set; }
    public decimal ROA { get; set; }
    public decimal BrutKarMarji { get; set; }
    public decimal NetKarMarji { get; set; }
    public decimal StokDevirHizi { get; set; }
    public int AlacakTahsilSuresi { get; set; }
    public int BorcOdemeSuresi { get; set; }
}

public class TrendResponse
{
    public int Yil { get; set; }
    public string[] Aylar { get; set; } = Array.Empty<string>();
    public decimal[] Gelirler { get; set; } = Array.Empty<decimal>();
    public decimal[] Giderler { get; set; } = Array.Empty<decimal>();
}

public class GiderDagilimiItem
{
    public string Kategori { get; set; } = "";
    public decimal Tutar { get; set; }
    public decimal Yuzde { get; set; }
}

public class SonIslemItem
{
    public string Aciklama { get; set; } = "";
    public DateTime Tarih { get; set; }
    public decimal Tutar { get; set; }
    public string Tip { get; set; } = "";
}

public class HatirlatmaItem
{
    public string Baslik { get; set; } = "";
    public string Aciklama { get; set; } = "";
    public DateTime Tarih { get; set; }
    public string Oncelik { get; set; } = "";
    public string Tip { get; set; } = "";
}

#endregion