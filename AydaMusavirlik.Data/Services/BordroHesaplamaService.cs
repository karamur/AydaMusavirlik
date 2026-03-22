using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Payroll;
using AydaMusavirlik.Data;

namespace AydaMusavirlik.Data.Services;

/// <summary>
/// Gelismis Bordro Hesaplama Servisi (RaptinBordro uyumlu)
/// </summary>
public class BordroHesaplamaService
{
    private readonly AppDbContext _context;
    private KanuniKesinti? _gecerliKesinti;

    public BordroHesaplamaService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Belirli ay icin kanuni kesinti parametrelerini yukler
    /// </summary>
    public async Task<KanuniKesinti?> GetKanuniKesintiAsync(int yil, int ay)
    {
        return await _context.KanuniKesintiler
            .Where(k => k.Yil == yil && k.Ay <= ay && k.IsActive)
            .OrderByDescending(k => k.Ay)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Tek calisan icin bordro hesaplar
    /// </summary>
    public async Task<TahakkukSonuc> HesaplaBordroAsync(int employeeId, int yil, int ay, Puantaj? puantaj = null)
    {
        var employee = await _context.Employees
            .Include(e => e.SgkBelgeTuru)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null)
            throw new ArgumentException("Calisan bulunamadi");

        _gecerliKesinti = await GetKanuniKesintiAsync(yil, ay);
        if (_gecerliKesinti == null)
            throw new InvalidOperationException($"{yil}/{ay} donemi icin kanuni kesinti parametreleri bulunamadi");

        var belgeTuru = employee.SgkBelgeTuru ?? await GetVarsayilanBelgeTuruAsync();

        // Puantaj yoksa 30 gun tam calisma varsay
        var calismaGunu = puantaj?.ToplamCalisilanGun ?? 30;
        var sgkPrimGunu = puantaj?.SgkPrimGunu ?? 30;

        return await HesaplaTahakkukAsync(employee, belgeTuru, _gecerliKesinti, yil, ay, calismaGunu, sgkPrimGunu, puantaj);
    }

    /// <summary>
    /// Tum calisanlar icin bordro hesaplar
    /// </summary>
    public async Task<List<TahakkukSonuc>> HesaplaTumBordroAsync(int companyId, int yil, int ay)
    {
        var employees = await _context.Employees
            .Include(e => e.SgkBelgeTuru)
            .Where(e => e.CompanyId == companyId && e.IsActive && e.TerminationDate == null)
            .ToListAsync();

        _gecerliKesinti = await GetKanuniKesintiAsync(yil, ay);
        if (_gecerliKesinti == null)
            throw new InvalidOperationException($"{yil}/{ay} donemi icin kanuni kesinti parametreleri bulunamadi");

        var sonuclar = new List<TahakkukSonuc>();
        var varsayilanBelgeTuru = await GetVarsayilanBelgeTuruAsync();

        foreach (var employee in employees)
        {
            var belgeTuru = employee.SgkBelgeTuru ?? varsayilanBelgeTuru;
            var puantaj = await _context.Puantajlar
                .FirstOrDefaultAsync(p => p.EmployeeId == employee.Id && p.Yil == yil && p.Ay == ay);

            var calismaGunu = puantaj?.ToplamCalisilanGun ?? 30;
            var sgkPrimGunu = puantaj?.SgkPrimGunu ?? 30;

            var sonuc = await HesaplaTahakkukAsync(employee, belgeTuru, _gecerliKesinti, yil, ay, calismaGunu, sgkPrimGunu, puantaj);
            sonuclar.Add(sonuc);
        }

        return sonuclar;
    }

    /// <summary>
    /// Tahakkuk hesaplama - Ana metod
    /// </summary>
    private async Task<TahakkukSonuc> HesaplaTahakkukAsync(Employee employee, SgkBelgeTuru belgeTuru, 
        KanuniKesinti kesinti, int yil, int ay, int calismaGunu, int sgkPrimGunu, Puantaj? puantaj)
    {
        var sonuc = new TahakkukSonuc
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            TcKimlikNo = employee.TcKimlikNo,
            SgkSicilNo = employee.SgkNumber ?? "",
            BelgeTuruKod = belgeTuru.Kod,
            Yil = yil,
            Ay = ay,
            CalismaGunu = calismaGunu,
            SgkPrimGunu = sgkPrimGunu
        };

        // 1. Brut Ucret Hesaplama
        decimal brutUcret = employee.GrossSalary;
        if (calismaGunu < 30)
        {
            brutUcret = Math.Round(employee.GrossSalary * calismaGunu / 30m, 2);
        }
        sonuc.BrutUcret = brutUcret;

        // 2. Fazla Mesai (varsa)
        if (puantaj != null)
        {
            var saatUcret = employee.GrossSalary / kesinti.AylikCalismaSaati;
            sonuc.FazlaMesaiUcreti = Math.Round(
                (puantaj.FazlaMesaiHaftaIci * saatUcret * kesinti.FazlaMesaiHaftaIciOrani) +
                (puantaj.FazlaMesaiHaftaSonu * saatUcret * kesinti.FazlaMesaiHaftaSonuOrani) +
                (puantaj.FazlaMesaiTatil * saatUcret * kesinti.FazlaMesaiTatilOrani), 2);
        }

        // 3. Toplam Kazanc
        sonuc.ToplamKazanc = sonuc.BrutUcret + sonuc.FazlaMesaiUcreti + sonuc.PrimIkramiye + sonuc.DigerKazanclar;

        // 4. SGK Matrahi (tavan kontrolu)
        sonuc.SgkMatrah = Math.Min(sonuc.ToplamKazanc, kesinti.SgkTavanUcret);
        sonuc.SgkMatrah = Math.Max(sonuc.SgkMatrah, kesinti.SgkTabanUcret * sgkPrimGunu / 30m);

        // 5. SGK Isci Payi
        if (belgeTuru.SigortaKesintisiVar)
        {
            sonuc.SgkIsciPayi = Math.Round(sonuc.SgkMatrah * belgeTuru.SigortaKesintiOrani, 2);
        }

        // 6. Issizlik Isci Payi
        if (belgeTuru.IssizlikKesintisiVar)
        {
            sonuc.IssizlikIsciPayi = Math.Round(sonuc.SgkMatrah * belgeTuru.IssizlikIsciOrani, 2);
        }

        // 7. Gelir Vergisi Matrahi
        sonuc.GelirVergisiMatrahi = sonuc.ToplamKazanc - sonuc.SgkIsciPayi - sonuc.IssizlikIsciPayi;

        // 8. Gelir Vergisi (kumulatif hesaplama)
        if (belgeTuru.GelirVergisiVar)
        {
            // Onceki aylarin kumulatif matrahi
            var oncekiMatrah = await GetKumulatifMatrahAsync(employee.Id, yil, ay);
            sonuc.KumulatifGelirVergisiMatrahi = oncekiMatrah + sonuc.GelirVergisiMatrahi;
            sonuc.GelirVergisi = HesaplaGelirVergisi(sonuc.GelirVergisiMatrahi, sonuc.KumulatifGelirVergisiMatrahi, kesinti);
        }

        // 9. Damga Vergisi
        if (belgeTuru.DamgaVergisiVar)
        {
            sonuc.DamgaVergisi = Math.Round(sonuc.ToplamKazanc * belgeTuru.DamgaVergisiOrani, 2);
        }

        // 10. Diger Kesintiler
        if (employee.SendikaUyesi && employee.SendikaAidatOrani.HasValue)
        {
            sonuc.SendikaAidati = Math.Round(sonuc.BrutUcret * employee.SendikaAidatOrani.Value, 2);
        }
        if (employee.IcraKesintisiVar && employee.IcraKesintisiTutari.HasValue)
        {
            sonuc.IcraKesintisi = employee.IcraKesintisiTutari.Value;
        }

        // 11. Toplam Kesinti
        sonuc.ToplamKesinti = sonuc.SgkIsciPayi + sonuc.IssizlikIsciPayi + sonuc.GelirVergisi + 
                              sonuc.DamgaVergisi + sonuc.SendikaAidati + sonuc.IcraKesintisi + sonuc.AvansKesintisi;

        // 12. Net Ucret
        sonuc.NetUcret = sonuc.ToplamKazanc - sonuc.ToplamKesinti;

        // 13. Isveren SGK Payi
        sonuc.SgkIsverenPayi = HesaplaIsverenSgkPayi(sonuc.SgkMatrah, belgeTuru, kesinti);

        // 14. Issizlik Isveren Payi
        if (belgeTuru.IssizlikKesintisiVar)
        {
            sonuc.IssizlikIsverenPayi = Math.Round(sonuc.SgkMatrah * belgeTuru.IssizlikIsverenOrani, 2);
        }

        // 15. Toplam Isveren Maliyeti
        sonuc.ToplamIsverenMaliyeti = sonuc.ToplamKazanc + sonuc.SgkIsverenPayi + sonuc.IssizlikIsverenPayi;

        return sonuc;
    }

    /// <summary>
    /// Gelir vergisi hesaplama (kumulatif dilim sistemi)
    /// </summary>
    private decimal HesaplaGelirVergisi(decimal aylikMatrah, decimal kumulatifMatrah, KanuniKesinti kesinti)
    {
        // Onceki kumulatif matrah
        decimal oncekiMatrah = kumulatifMatrah - aylikMatrah;

        // Bu ayin vergisi = Kumulatif vergi - Onceki aylarin vergisi
        decimal kumulatifVergi = HesaplaKumulatifVergi(kumulatifMatrah, kesinti);
        decimal oncekiVergi = HesaplaKumulatifVergi(oncekiMatrah, kesinti);

        return Math.Round(kumulatifVergi - oncekiVergi, 2);
    }

    private decimal HesaplaKumulatifVergi(decimal matrah, KanuniKesinti kesinti)
    {
        if (matrah <= 0) return 0;

        decimal vergi = 0;
        decimal kalanMatrah = matrah;

        // Dilim 1
        if (kalanMatrah > 0)
        {
            var dilimTutar = Math.Min(kalanMatrah, kesinti.GelirVergisiDilim1Limit);
            vergi += dilimTutar * kesinti.GelirVergisiDilim1Oran;
            kalanMatrah -= dilimTutar;
        }

        // Dilim 2
        if (kalanMatrah > 0)
        {
            var dilimTutar = Math.Min(kalanMatrah, kesinti.GelirVergisiDilim2Limit - kesinti.GelirVergisiDilim1Limit);
            vergi += dilimTutar * kesinti.GelirVergisiDilim2Oran;
            kalanMatrah -= dilimTutar;
        }

        // Dilim 3
        if (kalanMatrah > 0)
        {
            var dilimTutar = Math.Min(kalanMatrah, kesinti.GelirVergisiDilim3Limit - kesinti.GelirVergisiDilim2Limit);
            vergi += dilimTutar * kesinti.GelirVergisiDilim3Oran;
            kalanMatrah -= dilimTutar;
        }

        // Dilim 4
        if (kalanMatrah > 0)
        {
            var dilimTutar = Math.Min(kalanMatrah, kesinti.GelirVergisiDilim4Limit - kesinti.GelirVergisiDilim3Limit);
            vergi += dilimTutar * kesinti.GelirVergisiDilim4Oran;
            kalanMatrah -= dilimTutar;
        }

        // Dilim 5
        if (kalanMatrah > 0)
        {
            vergi += kalanMatrah * kesinti.GelirVergisiDilim5Oran;
        }

        return vergi;
    }

    /// <summary>
    /// Isveren SGK payi hesaplama
    /// </summary>
    private decimal HesaplaIsverenSgkPayi(decimal sgkMatrah, SgkBelgeTuru belgeTuru, KanuniKesinti kesinti)
    {
        decimal oran;

        switch (belgeTuru.IsverenHissesiTipi)
        {
            case IsverenHissesiTipi.Sabit:
                oran = belgeTuru.IsverenHissesiSabitOran;
                break;
            case IsverenHissesiTipi.IsKazasindenMuaf:
                oran = belgeTuru.AnalikSigortasiOrani + belgeTuru.HastalikSigortasiOrani + belgeTuru.MalullukYaslilikOrani;
                break;
            case IsverenHissesiTipi.Degisken:
            default:
                oran = belgeTuru.AnalikSigortasiOrani + belgeTuru.HastalikSigortasiOrani + 
                       belgeTuru.MalullukYaslilikOrani + belgeTuru.IsKazasiOrani;
                break;
        }

        return Math.Round(sgkMatrah * oran, 2);
    }

    /// <summary>
    /// Onceki aylarin kumulatif gelir vergisi matrahini getirir
    /// </summary>
    private async Task<decimal> GetKumulatifMatrahAsync(int employeeId, int yil, int ay)
    {
        var oncekiKayitlar = await _context.TahakkukDetaylari
            .Include(t => t.PayrollRecord)
            .Where(t => t.PayrollRecord.EmployeeId == employeeId && 
                        t.PayrollRecord.Year == yil && 
                        t.PayrollRecord.Month < ay)
            .ToListAsync();

        return oncekiKayitlar.Sum(t => t.GelirVergisiMatrahi);
    }

    /// <summary>
    /// Varsayilan belge turu (01-Normal)
    /// </summary>
    private async Task<SgkBelgeTuru> GetVarsayilanBelgeTuruAsync()
    {
        return await _context.SgkBelgeTurleri.FirstOrDefaultAsync(b => b.Kod == "01") 
            ?? throw new InvalidOperationException("Varsayilan belge turu (01) bulunamadi");
    }
}

/// <summary>
/// Tahakkuk Sonuc Modeli
/// </summary>
public class TahakkukSonuc
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public string SgkSicilNo { get; set; } = string.Empty;
    public string BelgeTuruKod { get; set; } = string.Empty;
    public int Yil { get; set; }
    public int Ay { get; set; }
    public int CalismaGunu { get; set; }
    public int SgkPrimGunu { get; set; }

    // Kazanclar
    public decimal BrutUcret { get; set; }
    public decimal FazlaMesaiUcreti { get; set; }
    public decimal PrimIkramiye { get; set; }
    public decimal DigerKazanclar { get; set; }
    public decimal ToplamKazanc { get; set; }

    // SGK
    public decimal SgkMatrah { get; set; }
    public decimal SgkIsciPayi { get; set; }
    public decimal IssizlikIsciPayi { get; set; }
    public decimal SgkIsverenPayi { get; set; }
    public decimal IssizlikIsverenPayi { get; set; }

    // Vergiler
    public decimal GelirVergisiMatrahi { get; set; }
    public decimal GelirVergisi { get; set; }
    public decimal DamgaVergisi { get; set; }
    public decimal KumulatifGelirVergisiMatrahi { get; set; }

    // Diger Kesintiler
    public decimal SendikaAidati { get; set; }
    public decimal IcraKesintisi { get; set; }
    public decimal AvansKesintisi { get; set; }

    // Toplamlar
    public decimal ToplamKesinti { get; set; }
    public decimal NetUcret { get; set; }
    public decimal ToplamIsverenMaliyeti { get; set; }
}