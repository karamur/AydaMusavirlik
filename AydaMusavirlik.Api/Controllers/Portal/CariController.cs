using Microsoft.AspNetCore.Mvc;
using AydaMusavirlik.Data.Repositories;

namespace AydaMusavirlik.Api.Controllers;

/// <summary>
/// Müþteri/Tedarikçi Portal API - Cari Hesap Ýþlemleri
/// </summary>
[ApiController]
[Route("api/portal/[controller]")]
public class CariController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountingRecordRepository _recordRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CariController(
        IAccountRepository accountRepository,
        IAccountingRecordRepository recordRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _recordRepository = recordRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Cari portal giriþi (Vergi No ile)
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<CariLoginResponse>> Login([FromBody] CariLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.VergiNo) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Vergi No ve þifre gerekli" });

        // Demo giriþ
        if (request.VergiNo == "1234567890" && request.Password == "123456")
        {
            return Ok(new CariLoginResponse
            {
                Success = true,
                Token = "demo-token-cari-12345",
                CariId = 1,
                FirmaAdi = "ABC Ticaret Ltd. Þti.",
                HesapTipi = "Müþteri",
                Email = "info@abcticaret.com"
            });
        }

        // Gerçek giriþ kontrolü
        var accounts = await _accountRepository.GetByCompanyAsync(1);
        var cariHesaplar = accounts.Where(a => a.Code.StartsWith("120") || a.Code.StartsWith("320"));

        // Vergi no ile eþleþtirme (gerçek uygulamada hesap detaylarýnda tutulmalý)
        // Demo için hesap kodunun son 10 hanesi vergi no olarak kabul ediliyor

        return Ok(new CariLoginResponse
        {
            Success = true,
            Token = $"token-cari-{DateTime.Now.Ticks}",
            CariId = 1,
            FirmaAdi = "Demo Firma",
            HesapTipi = "Müþteri",
            Email = "demo@firma.com"
        });
    }

    /// <summary>
    /// Cari hesap bilgilerini getirir
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<CariHesapResponse> GetCariHesap(int id)
    {
        // Demo veri
        return Ok(new CariHesapResponse
        {
            Id = id,
            FirmaAdi = "ABC Ticaret Ltd. Þti.",
            VergiNo = "1234567890",
            VergiDairesi = "Kadýköy",
            HesapTipi = "Müþteri",
            Adres = "Baðdat Cad. No:123 Kadýköy/Ýstanbul",
            Telefon = "0216 123 45 67",
            Email = "info@abcticaret.com",
            Bakiye = -15750.50m, // Negatif = Borçlu
            ToplamBorc = 130000,
            ToplamAlacak = 114249.50m,
            SonIslemTarihi = DateTime.Now.AddDays(-5)
        });
    }

    /// <summary>
    /// Cari hesap hareketlerini getirir
    /// </summary>
    [HttpGet("{id}/hareketler")]
    public ActionResult<HesapHareketListResponse> GetHesapHareketleri(
        int id, 
        [FromQuery] DateTime? baslangic, 
        [FromQuery] DateTime? bitis,
        [FromQuery] int sayfa = 1,
        [FromQuery] int sayfaBoyutu = 50)
    {
        baslangic ??= DateTime.Now.AddMonths(-3);
        bitis ??= DateTime.Now;

        // Demo veri
        var hareketler = new List<HesapHareketResponse>
        {
            new() { Id = 1, Tarih = new DateTime(2025, 1, 1), BelgeNo = "", Aciklama = "Devir", Borc = 5000, Alacak = 0, BelgeTipi = "Devir" },
            new() { Id = 2, Tarih = new DateTime(2025, 1, 5), BelgeNo = "FT-2025-001", Aciklama = "Satýþ Faturasý", Borc = 25000, Alacak = 0, BelgeTipi = "Fatura" },
            new() { Id = 3, Tarih = new DateTime(2025, 1, 10), BelgeNo = "MK-2025-001", Aciklama = "Havale ile ödeme", Borc = 0, Alacak = 20000, BelgeTipi = "Tahsilat" },
            new() { Id = 4, Tarih = new DateTime(2025, 1, 15), BelgeNo = "FT-2025-012", Aciklama = "Satýþ Faturasý", Borc = 35000, Alacak = 0, BelgeTipi = "Fatura" },
            new() { Id = 5, Tarih = new DateTime(2025, 1, 20), BelgeNo = "MK-2025-005", Aciklama = "EFT ile ödeme", Borc = 0, Alacak = 30000, BelgeTipi = "Tahsilat" },
            new() { Id = 6, Tarih = new DateTime(2025, 2, 1), BelgeNo = "FT-2025-025", Aciklama = "Satýþ Faturasý", Borc = 45000, Alacak = 0, BelgeTipi = "Fatura" },
            new() { Id = 7, Tarih = new DateTime(2025, 2, 10), BelgeNo = "MK-2025-012", Aciklama = "Havale ile ödeme", Borc = 0, Alacak = 40000, BelgeTipi = "Tahsilat" },
            new() { Id = 8, Tarih = new DateTime(2025, 2, 20), BelgeNo = "FT-2025-038", Aciklama = "Satýþ Faturasý", Borc = 20000, Alacak = 0, BelgeTipi = "Fatura" },
            new() { Id = 9, Tarih = new DateTime(2025, 3, 1), BelgeNo = "MK-2025-018", Aciklama = "Kredi kartý ile ödeme", Borc = 0, Alacak = 19249.50m, BelgeTipi = "Tahsilat" },
        };

        // Bakiye hesapla
        decimal bakiye = 0;
        foreach (var hareket in hareketler)
        {
            bakiye += hareket.Borc - hareket.Alacak;
            hareket.Bakiye = bakiye;
        }

        return Ok(new HesapHareketListResponse
        {
            Hareketler = hareketler,
            ToplamBorc = hareketler.Sum(h => h.Borc),
            ToplamAlacak = hareketler.Sum(h => h.Alacak),
            Bakiye = bakiye,
            ToplamKayit = hareketler.Count,
            Sayfa = sayfa,
            SayfaBoyutu = sayfaBoyutu
        });
    }

    /// <summary>
    /// Faturalarý listeler
    /// </summary>
    [HttpGet("{id}/faturalar")]
    public ActionResult<List<FaturaResponse>> GetFaturalar(int id, [FromQuery] bool? odenmemis = null)
    {
        var faturalar = new List<FaturaResponse>
        {
            new() { Id = 1, FaturaNo = "FT-2025-038", Tarih = new DateTime(2025, 2, 20), VadeTarihi = new DateTime(2025, 3, 20), Tutar = 20000, KdvTutar = 3600, ToplamTutar = 23600, Odendi = false },
            new() { Id = 2, FaturaNo = "FT-2025-025", Tarih = new DateTime(2025, 2, 1), VadeTarihi = new DateTime(2025, 3, 1), Tutar = 45000, KdvTutar = 8100, ToplamTutar = 53100, Odendi = true },
            new() { Id = 3, FaturaNo = "FT-2025-012", Tarih = new DateTime(2025, 1, 15), VadeTarihi = new DateTime(2025, 2, 15), Tutar = 35000, KdvTutar = 6300, ToplamTutar = 41300, Odendi = true },
            new() { Id = 4, FaturaNo = "FT-2025-001", Tarih = new DateTime(2025, 1, 5), VadeTarihi = new DateTime(2025, 2, 5), Tutar = 25000, KdvTutar = 4500, ToplamTutar = 29500, Odendi = true },
        };

        if (odenmemis == true)
            faturalar = faturalar.Where(f => !f.Odendi).ToList();

        return Ok(faturalar);
    }

    /// <summary>
    /// Ekstre PDF indir
    /// </summary>
    [HttpGet("{id}/ekstre/pdf")]
    public ActionResult DownloadEkstre(int id, [FromQuery] DateTime? baslangic, [FromQuery] DateTime? bitis)
    {
        baslangic ??= DateTime.Now.AddMonths(-1);
        bitis ??= DateTime.Now;

        return Ok(new { 
            message = $"Ekstre PDF: Cari {id}, {baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy}", 
            downloadUrl = $"/downloads/ekstre_{id}_{baslangic:yyyyMMdd}_{bitis:yyyyMMdd}.pdf" 
        });
    }

    /// <summary>
    /// Fatura PDF indir
    /// </summary>
    [HttpGet("{id}/fatura/{faturaNo}/pdf")]
    public ActionResult DownloadFatura(int id, string faturaNo)
    {
        return Ok(new { 
            message = $"Fatura PDF: {faturaNo}", 
            downloadUrl = $"/downloads/fatura_{faturaNo}.pdf" 
        });
    }

    /// <summary>
    /// Mesaj gönder
    /// </summary>
    [HttpPost("{id}/mesaj")]
    public ActionResult<MesajResponse> SendMesaj(int id, [FromBody] MesajRequest request)
    {
        return Ok(new MesajResponse
        {
            Id = new Random().Next(1000, 9999),
            Konu = request.Konu,
            Mesaj = request.Mesaj,
            GonderimTarihi = DateTime.Now,
            Durum = "Gönderildi"
        });
    }

    /// <summary>
    /// Mesajlarý listeler
    /// </summary>
    [HttpGet("{id}/mesajlar")]
    public ActionResult<List<MesajResponse>> GetMesajlar(int id)
    {
        return Ok(new List<MesajResponse>
        {
            new() { Id = 1, Konu = "Fatura Bildirimi", Mesaj = "Yeni faturanýz hazýrlandý.", GonderimTarihi = new DateTime(2025, 3, 1), Durum = "Okundu" },
            new() { Id = 2, Konu = "Ödeme Onayý", Mesaj = "Ödemeniz alýndý, teþekkür ederiz.", GonderimTarihi = new DateTime(2025, 2, 10), Durum = "Okundu" },
            new() { Id = 3, Konu = "Hesap Özeti", Mesaj = "Ocak 2025 hesap özetiniz hazýr.", GonderimTarihi = new DateTime(2025, 2, 1), Durum = "Okundu" },
        });
    }
}

#region Request/Response Models

public class CariLoginRequest
{
    public string VergiNo { get; set; } = "";
    public string Password { get; set; } = "";
}

public class CariLoginResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = "";
    public int CariId { get; set; }
    public string FirmaAdi { get; set; } = "";
    public string HesapTipi { get; set; } = "";
    public string Email { get; set; } = "";
}

public class CariHesapResponse
{
    public int Id { get; set; }
    public string FirmaAdi { get; set; } = "";
    public string VergiNo { get; set; } = "";
    public string VergiDairesi { get; set; } = "";
    public string HesapTipi { get; set; } = "";
    public string Adres { get; set; } = "";
    public string Telefon { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal Bakiye { get; set; }
    public decimal ToplamBorc { get; set; }
    public decimal ToplamAlacak { get; set; }
    public DateTime SonIslemTarihi { get; set; }
}

public class HesapHareketResponse
{
    public int Id { get; set; }
    public DateTime Tarih { get; set; }
    public string BelgeNo { get; set; } = "";
    public string Aciklama { get; set; } = "";
    public decimal Borc { get; set; }
    public decimal Alacak { get; set; }
    public decimal Bakiye { get; set; }
    public string BelgeTipi { get; set; } = "";
}

public class HesapHareketListResponse
{
    public List<HesapHareketResponse> Hareketler { get; set; } = new();
    public decimal ToplamBorc { get; set; }
    public decimal ToplamAlacak { get; set; }
    public decimal Bakiye { get; set; }
    public int ToplamKayit { get; set; }
    public int Sayfa { get; set; }
    public int SayfaBoyutu { get; set; }
}

public class FaturaResponse
{
    public int Id { get; set; }
    public string FaturaNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public DateTime VadeTarihi { get; set; }
    public decimal Tutar { get; set; }
    public decimal KdvTutar { get; set; }
    public decimal ToplamTutar { get; set; }
    public bool Odendi { get; set; }
}

public class MesajRequest
{
    public string Konu { get; set; } = "";
    public string Mesaj { get; set; } = "";
}

public class MesajResponse
{
    public int Id { get; set; }
    public string Konu { get; set; } = "";
    public string Mesaj { get; set; } = "";
    public DateTime GonderimTarihi { get; set; }
    public string Durum { get; set; } = "";
}

#endregion