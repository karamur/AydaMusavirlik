using AydaMusavirlik.Application.Common.Interfaces;
using AydaMusavirlik.Core.Models.FinancialAnalysis;

namespace AydaMusavirlik.Infrastructure.Services;

/// <summary>
/// Mali Analiz Servisi - Rasyo, DuPont, SWOT, Trend analizleri
/// </summary>
public class FinancialAnalysisService : IFinancialAnalysisService
{
    private readonly IUnitOfWork _unitOfWork;

    public FinancialAnalysisService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RasyoAnaliziResult> CalculateRasyoAnaliziAsync(int companyId, int yil, CancellationToken cancellationToken = default)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId, cancellationToken);

        // Muhasebe kayitlarindan bilanço verilerini al
        var records = await _unitOfWork.AccountingRecords.GetByCompanyAsync(
            companyId, 
            new DateTime(yil, 1, 1), 
            new DateTime(yil, 12, 31), 
            cancellationToken);

        // Hesap bakiyelerini hesapla (gercek uygulamada daha detayli)
        var donenVarliklar = 500000m; // Ornek
        var stoklar = 100000m;
        var nakit = 50000m;
        var kisaVadeliBorclar = 200000m;
        var toplamAktif = 1000000m;
        var toplamBorc = 400000m;
        var ozsermaye = 600000m;
        var netSatislar = 1200000m;
        var netKar = 120000m;
        var brutKar = 300000m;
        var faaliyetKari = 180000m;
        var faizGiderleri = 20000m;

        var result = new RasyoAnaliziResult
        {
            Yil = yil,
            FirmaAdi = company?.Name ?? "Bilinmeyen",

            Likidite = new LikiditeRasyolari
            {
                CariOran = kisaVadeliBorclar > 0 ? Math.Round(donenVarliklar / kisaVadeliBorclar, 2) : 0,
                AsitTestOrani = kisaVadeliBorclar > 0 ? Math.Round((donenVarliklar - stoklar) / kisaVadeliBorclar, 2) : 0,
                NakitOrani = kisaVadeliBorclar > 0 ? Math.Round(nakit / kisaVadeliBorclar, 2) : 0,
                NetIsletmeSermayesi = donenVarliklar - kisaVadeliBorclar
            },

            Karlilik = new KarlilikRasyolari
            {
                BrutKarMarji = netSatislar > 0 ? Math.Round((brutKar / netSatislar) * 100, 2) : 0,
                NetKarMarji = netSatislar > 0 ? Math.Round((netKar / netSatislar) * 100, 2) : 0,
                FaaliyetKarMarji = netSatislar > 0 ? Math.Round((faaliyetKari / netSatislar) * 100, 2) : 0,
                AktifKarliligi = toplamAktif > 0 ? Math.Round((netKar / toplamAktif) * 100, 2) : 0,
                OzsermayeKarliligi = ozsermaye > 0 ? Math.Round((netKar / ozsermaye) * 100, 2) : 0
            },

            Faaliyet = new FaaliyetRasyolari
            {
                StokDevirHizi = stoklar > 0 ? Math.Round(netSatislar * 0.6m / stoklar, 2) : 0,
                VarlikDevirHizi = toplamAktif > 0 ? Math.Round(netSatislar / toplamAktif, 2) : 0,
                OzsermayeDevirHizi = ozsermaye > 0 ? Math.Round(netSatislar / ozsermaye, 2) : 0
            },

            FinansalYapi = new FinansalYapiRasyolari
            {
                BorcOrani = toplamAktif > 0 ? Math.Round((toplamBorc / toplamAktif) * 100, 2) : 0,
                OzsermayeOrani = toplamAktif > 0 ? Math.Round((ozsermaye / toplamAktif) * 100, 2) : 0,
                FinansalKaldirac = ozsermaye > 0 ? Math.Round(toplamAktif / ozsermaye, 2) : 0,
                FaizKarsilamaOrani = faizGiderleri > 0 ? Math.Round(faaliyetKari / faizGiderleri, 2) : 0
            }
        };

        result.MaliSaglikPuani = CalculateMaliSaglikPuaniInternal(result);
        result.GenelDegerlendirme = GetGenelDegerlendirme(result.MaliSaglikPuani);

        return result;
    }

    public async Task<DuPontAnaliziResult> CalculateDuPontAnaliziAsync(int companyId, int yil, CancellationToken cancellationToken = default)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId, cancellationToken);

        // Ornek veriler (gercek uygulamada muhasebe kayitlarindan cekilecek)
        var netKar = 120000m;
        var netSatislar = 1200000m;
        var toplamVarliklar = 1000000m;
        var ozsermaye = 600000m;

        var netKarMarji = netSatislar > 0 ? (netKar / netSatislar) * 100 : 0;
        var varlikDevirHizi = toplamVarliklar > 0 ? netSatislar / toplamVarliklar : 0;
        var finansalKaldirac = ozsermaye > 0 ? toplamVarliklar / ozsermaye : 0;

        var roa = (netKarMarji / 100) * varlikDevirHizi * 100;
        var roe = roa * finansalKaldirac;

        return new DuPontAnaliziResult
        {
            Yil = yil,
            FirmaAdi = company?.Name ?? "Bilinmeyen",
            NetKar = netKar,
            NetSatislar = netSatislar,
            ToplamVarliklar = toplamVarliklar,
            Ozsermaye = ozsermaye,
            NetKarMarji = Math.Round(netKarMarji, 2),
            VarlikDevirHizi = Math.Round(varlikDevirHizi, 2),
            FinansalKaldirac = Math.Round(finansalKaldirac, 2),
            ROA = Math.Round(roa, 2),
            ROE = Math.Round(roe, 2)
        };
    }

    public async Task<SwotAnaliziResult> CalculateSwotAnaliziAsync(int companyId, int yil, CancellationToken cancellationToken = default)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId, cancellationToken);
        var rasyolar = await CalculateRasyoAnaliziAsync(companyId, yil, cancellationToken);

        var result = new SwotAnaliziResult
        {
            Yil = yil,
            FirmaAdi = company?.Name ?? "Bilinmeyen"
        };

        // Guclu Yonler
        if (rasyolar.Likidite.CariOran >= 1.5m)
            result.GucluYonler.Add(new SwotItem { Baslik = "Guclu Likidite", Aciklama = $"Cari oran {rasyolar.Likidite.CariOran:N2} ile saglam", OnemDerecesi = 4, Kaynak = "Likidite Analizi" });

        if (rasyolar.Karlilik.NetKarMarji >= 10)
            result.GucluYonler.Add(new SwotItem { Baslik = "Yuksek Karlilik", Aciklama = $"Net kar marji %{rasyolar.Karlilik.NetKarMarji:N1}", OnemDerecesi = 5, Kaynak = "Karlilik Analizi" });

        if (rasyolar.FinansalYapi.BorcOrani <= 50)
            result.GucluYonler.Add(new SwotItem { Baslik = "Dusuk Borc Orani", Aciklama = $"Borc orani %{rasyolar.FinansalYapi.BorcOrani:N1}", OnemDerecesi = 4, Kaynak = "Finansal Yapi" });

        // Zayif Yonler
        if (rasyolar.Likidite.CariOran < 1)
            result.ZayifYonler.Add(new SwotItem { Baslik = "Likidite Sorunu", Aciklama = "Kisa vadeli borc odeme guclugu", OnemDerecesi = 5, Kaynak = "Likidite Analizi" });

        if (rasyolar.Karlilik.NetKarMarji < 5)
            result.ZayifYonler.Add(new SwotItem { Baslik = "Dusuk Karlilik", Aciklama = "Kar marji iyilestirmeli", OnemDerecesi = 4, Kaynak = "Karlilik Analizi" });

        if (rasyolar.FinansalYapi.BorcOrani > 70)
            result.ZayifYonler.Add(new SwotItem { Baslik = "Yuksek Borclanma", Aciklama = "Finansal risk yuksek", OnemDerecesi = 5, Kaynak = "Finansal Yapi" });

        // Firsatlar
        if (rasyolar.Likidite.NakitOrani > 0.3m)
            result.Firsatlar.Add(new SwotItem { Baslik = "Yatirim Kapasitesi", Aciklama = "Mevcut nakit ile yeni yatirimlar yapilabilir", OnemDerecesi = 4, Kaynak = "Nakit Analizi" });

        result.Firsatlar.Add(new SwotItem { Baslik = "Dijitallesme", Aciklama = "Sureclerin otomasyonu ile verimlilik artisi", OnemDerecesi = 3, Kaynak = "Genel" });

        // Tehditler
        result.Tehditler.Add(new SwotItem { Baslik = "Enflasyon Riski", Aciklama = "Yuksek enflasyon maliyetleri artirabilir", OnemDerecesi = 4, Kaynak = "Makro Ekonomi" });
        result.Tehditler.Add(new SwotItem { Baslik = "Rekabet", Aciklama = "Sektor rekabeti kar marjlarini dusurabilir", OnemDerecesi = 3, Kaynak = "Sektor Analizi" });

        // Stratejik Oneriler
        result.StratejikOneriler = GenerateStrategicRecommendations(rasyolar);
        result.GenelDegerlendirme = GetSwotDegerlendirme(result);

        return result;
    }

    public async Task<TrendAnaliziResult> CalculateTrendAnaliziAsync(int companyId, int baslangicYil, int bitisYil, CancellationToken cancellationToken = default)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId, cancellationToken);

        var result = new TrendAnaliziResult
        {
            FirmaAdi = company?.Name ?? "Bilinmeyen",
            BaslangicYil = baslangicYil,
            BitisYil = bitisYil
        };

        // Ornek veri (gercek uygulamada yillik veriler cekilecek)
        decimal bazGelir = 1000000;
        decimal bazGider = 800000;

        for (int yil = baslangicYil; yil <= bitisYil; yil++)
        {
            var faktor = 1 + (yil - baslangicYil) * 0.1m;
            var gelir = bazGelir * faktor;
            var gider = bazGider * faktor * 0.95m;
            var kar = gelir - gider;

            result.GelirTrendi.Add(new TrendVeri
            {
                Yil = yil,
                Deger = gelir,
                DegisimOrani = yil == baslangicYil ? 0 : 10,
                Endeks = (int)((gelir / bazGelir) * 100)
            });

            result.GiderTrendi.Add(new TrendVeri
            {
                Yil = yil,
                Deger = gider,
                DegisimOrani = yil == baslangicYil ? 0 : 8,
                Endeks = (int)((gider / bazGider) * 100)
            });

            result.KarTrendi.Add(new TrendVeri
            {
                Yil = yil,
                Deger = kar,
                DegisimOrani = yil == baslangicYil ? 0 : 15,
                Endeks = (int)((kar / (bazGelir - bazGider)) * 100)
            });

            result.KarMarjiTrendi.Add(new TrendVeri
            {
                Yil = yil,
                Deger = (kar / gelir) * 100,
                DegisimOrani = 0,
                Endeks = 100
            });
        }

        result.GelirBuyumeOrtalamasý = 10;
        result.GiderBuyumeOrtalamasi = 8;
        result.KarBuyumeOrtalamasi = 15;
        result.GelirTrendYonu = TrendYonu.Yukselis;
        result.KarTrendYonu = TrendYonu.Yukselis;
        result.Degerlendirme = "Firma buyume trendinde. Gelir ve kar artisi suruyor.";

        return result;
    }

    public async Task<int> CalculateMaliSaglikPuaniAsync(int companyId, int yil, CancellationToken cancellationToken = default)
    {
        var rasyolar = await CalculateRasyoAnaliziAsync(companyId, yil, cancellationToken);
        return CalculateMaliSaglikPuaniInternal(rasyolar);
    }

    private int CalculateMaliSaglikPuaniInternal(RasyoAnaliziResult rasyolar)
    {
        int puan = 50; // Baz puan

        // Likidite (max 20 puan)
        if (rasyolar.Likidite.CariOran >= 2) puan += 20;
        else if (rasyolar.Likidite.CariOran >= 1.5m) puan += 15;
        else if (rasyolar.Likidite.CariOran >= 1) puan += 10;
        else if (rasyolar.Likidite.CariOran >= 0.5m) puan += 5;
        else puan -= 10;

        // Karlilik (max 20 puan)
        if (rasyolar.Karlilik.NetKarMarji >= 20) puan += 20;
        else if (rasyolar.Karlilik.NetKarMarji >= 10) puan += 15;
        else if (rasyolar.Karlilik.NetKarMarji >= 5) puan += 10;
        else if (rasyolar.Karlilik.NetKarMarji > 0) puan += 5;
        else puan -= 15;

        // Borc Orani (max 10 puan)
        if (rasyolar.FinansalYapi.BorcOrani <= 40) puan += 10;
        else if (rasyolar.FinansalYapi.BorcOrani <= 60) puan += 5;
        else if (rasyolar.FinansalYapi.BorcOrani > 80) puan -= 10;

        return Math.Clamp(puan, 0, 100);
    }

    private string GetGenelDegerlendirme(int puan)
    {
        if (puan >= 80) return "Mukemmel mali saglik. Firma finansal aciidan cok guclu.";
        if (puan >= 60) return "Iyi mali saglik. Kucuk iyilestirmeler yapilabilir.";
        if (puan >= 40) return "Orta mali saglik. Bazi alanlarda iyilestirme gerekli.";
        if (puan >= 20) return "Zayif mali saglik. Ciddi onlemler alinmali.";
        return "Kritik mali saglik. Acil mudahale gerekli!";
    }

    private string GetSwotDegerlendirme(SwotAnaliziResult swot)
    {
        var gucSayisi = swot.GucluYonler.Count;
        var zayifSayisi = swot.ZayifYonler.Count;

        if (gucSayisi > zayifSayisi * 2)
            return "Firma guclu konumda. Buyume stratejileri uygulanabilir.";
        if (gucSayisi > zayifSayisi)
            return "Pozitif goruntum. Zayif yonler uzerinde calisilmali.";
        if (gucSayisi == zayifSayisi)
            return "Dengeli durum. Stratejik planlama onemli.";
        return "Dikkatli olunmali. Zayif yonler oncelikle ele alinmali.";
    }

    private List<string> GenerateStrategicRecommendations(RasyoAnaliziResult rasyolar)
    {
        var oneriler = new List<string>();

        if (rasyolar.Likidite.CariOran < 1.5m)
            oneriler.Add("Likiditeyi artirmak icin alacak tahsilat surecini hizlandirin");

        if (rasyolar.Karlilik.NetKarMarji < 10)
            oneriler.Add("Kar marjini artirmak icin maliyet optimizasyonu yapin");

        if (rasyolar.FinansalYapi.BorcOrani > 60)
            oneriler.Add("Borc yapisini yeniden duzenleyin, uzun vadeli finansman tercih edin");

        if (rasyolar.Faaliyet.VarlikDevirHizi < 1)
            oneriler.Add("Atil varliklari degerlendirin veya elden cikarin");

        oneriler.Add("Dijital donusum yatirimlari ile verimliligi artirin");
        oneriler.Add("Duzennli mali analiz ile performansi takip edin");

        return oneriler;
    }
}