namespace AydaMusavirlik.Core.Models.FinancialAnalysis;

/// <summary>
/// Rasyo Analizi Sonuclari
/// </summary>
public class RasyoAnaliziResult
{
    public int Yil { get; set; }
    public string FirmaAdi { get; set; } = string.Empty;

    // Likidite Rasyolari
    public LikiditeRasyolari Likidite { get; set; } = new();

    // Karlilik Rasyolari
    public KarlilikRasyolari Karlilik { get; set; } = new();

    // Faaliyet Rasyolari
    public FaaliyetRasyolari Faaliyet { get; set; } = new();

    // Finansal Yapi Rasyolari
    public FinansalYapiRasyolari FinansalYapi { get; set; } = new();

    // Genel Degerlendirme
    public string GenelDegerlendirme { get; set; } = string.Empty;
    public int MaliSaglikPuani { get; set; } // 0-100
}

public class LikiditeRasyolari
{
    public decimal CariOran { get; set; }              // Donen Varliklar / Kisa Vadeli Borclar
    public decimal AsitTestOrani { get; set; }         // (Donen Varliklar - Stoklar) / Kisa Vadeli Borclar
    public decimal NakitOrani { get; set; }            // Nakit / Kisa Vadeli Borclar
    public decimal NetIsletmeSermayesi { get; set; }   // Donen Varliklar - Kisa Vadeli Borclar

    public string CariOranYorum => GetCariOranYorum();
    public string AsitTestYorum => GetAsitTestYorum();

    private string GetCariOranYorum()
    {
        if (CariOran >= 2) return "Cok iyi - Kisa vadeli borclar rahatlikla odenebilir";
        if (CariOran >= 1.5m) return "Iyi - Guvenli likidite seviyesi";
        if (CariOran >= 1) return "Dikkat - Likidite sikisilabilir";
        return "Riskli - Kisa vadeli borc odeme guclugu yasanabilir";
    }

    private string GetAsitTestYorum()
    {
        if (AsitTestOrani >= 1) return "Iyi - Stoklar haric borclar karsilanabilir";
        if (AsitTestOrani >= 0.7m) return "Kabul edilebilir";
        return "Riskli - Likidite sorunu olabilir";
    }
}

public class KarlilikRasyolari
{
    public decimal BrutKarMarji { get; set; }          // Brut Kar / Net Satislar
    public decimal NetKarMarji { get; set; }           // Net Kar / Net Satislar
    public decimal FaaliyetKarMarji { get; set; }      // Faaliyet Kari / Net Satislar
    public decimal AktifKarliligi { get; set; }        // ROA - Net Kar / Toplam Aktif
    public decimal OzsermayeKarliligi { get; set; }    // ROE - Net Kar / Ozsermaye

    public string NetKarMarjiYorum => GetNetKarMarjiYorum();
    public string ROEYorum => GetROEYorum();

    private string GetNetKarMarjiYorum()
    {
        if (NetKarMarji >= 20) return "Cok iyi - Her 100 TL satistan 20+ TL kar";
        if (NetKarMarji >= 10) return "Iyi - Saglikli kar marji";
        if (NetKarMarji >= 5) return "Orta - Iyilestirilebilir";
        if (NetKarMarji > 0) return "Dusuk kar marji";
        return "Zarar durumu!";
    }

    private string GetROEYorum()
    {
        if (OzsermayeKarliligi >= 20) return "Mukemmel - Yatirimcilar icin caziplik yuksek";
        if (OzsermayeKarliligi >= 15) return "Cok iyi";
        if (OzsermayeKarliligi >= 10) return "Iyi";
        if (OzsermayeKarliligi > 0) return "Dusuk";
        return "Negatif - Sermaye erimesi";
    }
}

public class FaaliyetRasyolari
{
    public decimal StokDevirHizi { get; set; }         // SMM / Ortalama Stok
    public decimal StokDevirSuresi { get; set; }       // 365 / Stok Devir Hizi
    public decimal AlacakDevirHizi { get; set; }       // Net Satislar / Ortalama Ticari Alacaklar
    public decimal AlacakTahsilSuresi { get; set; }    // 365 / Alacak Devir Hizi
    public decimal VarlikDevirHizi { get; set; }       // Net Satislar / Toplam Aktif
    public decimal OzsermayeDevirHizi { get; set; }    // Net Satislar / Ozsermaye
}

public class FinansalYapiRasyolari
{
    public decimal BorcOrani { get; set; }             // Toplam Borc / Toplam Aktif
    public decimal OzsermayeOrani { get; set; }        // Ozsermaye / Toplam Aktif
    public decimal FinansalKaldirac { get; set; }      // Toplam Aktif / Ozsermaye
    public decimal KisaVadeBorcOrani { get; set; }     // Kisa Vadeli Borc / Toplam Borc
    public decimal FaizKarsilamaOrani { get; set; }    // FVOK / Faiz Giderleri

    public string BorcOraniYorum => GetBorcOraniYorum();
    public string FinansalKaldiracYorum => GetFinansalKaldiracYorum();

    private string GetBorcOraniYorum()
    {
        if (BorcOrani <= 40) return "Muhafazakar - Dusuk risk";
        if (BorcOrani <= 60) return "Dengeli - Kabul edilebilir";
        if (BorcOrani <= 80) return "Yuksek borc - Dikkatli yonetim gerekli";
        return "Cok yuksek risk!";
    }

    private string GetFinansalKaldiracYorum()
    {
        if (FinansalKaldirac <= 1.5m) return "Muhafazakar yapi - Buyume potansiyeli var";
        if (FinansalKaldirac <= 2) return "Dengeli sermaye yapisi";
        if (FinansalKaldirac <= 3) return "Orta-yuksek borc orani";
        return "Yuksek borc! Risk seviyesi yuksek";
    }
}

/// <summary>
/// DuPont Analizi Sonuclari
/// </summary>
public class DuPontAnaliziResult
{
    public int Yil { get; set; }
    public string FirmaAdi { get; set; } = string.Empty;

    // Ana Bilesenler
    public decimal NetKarMarji { get; set; }           // Net Kar / Net Satislar
    public decimal VarlikDevirHizi { get; set; }       // Net Satislar / Toplam Varliklar
    public decimal FinansalKaldirac { get; set; }      // Toplam Varliklar / Ozsermaye

    // Sonuc
    public decimal ROE { get; set; }                   // Net Kar Marji x Varlik Devir Hizi x Finansal Kaldirac
    public decimal ROA { get; set; }                   // Net Kar Marji x Varlik Devir Hizi

    // Detay Bilgiler
    public decimal NetKar { get; set; }
    public decimal NetSatislar { get; set; }
    public decimal ToplamVarliklar { get; set; }
    public decimal Ozsermaye { get; set; }

    // Yorumlar
    public string NetKarMarjiYorum => GetNetKarMarjiYorum();
    public string VarlikDevirHiziYorum => GetVarlikDevirHiziYorum();
    public string FinansalKaldiracYorum => GetFinansalKaldiracYorum();
    public string ROEYorum => GetROEYorum();

    private string GetNetKarMarjiYorum()
    {
        if (NetKarMarji > 20) return "Cok iyi! Her 100 TL satistan 20+ TL kar";
        if (NetKarMarji > 10) return "Iyi - Saglikli kar marji";
        if (NetKarMarji > 5) return "Orta - Iyilestirilebilir";
        if (NetKarMarji > 0) return "Dusuk kar marji - Maliyet kontrolu gerekli";
        return "Zarar durumu! Acil onlem alinmali";
    }

    private string GetVarlikDevirHiziYorum()
    {
        if (VarlikDevirHizi > 2) return "Cok verimli! Varliklar etkin kullaniliyor";
        if (VarlikDevirHizi > 1) return "Iyi verimlilik";
        if (VarlikDevirHizi > 0.5m) return "Orta verimlilik - Atil kapasite olabilir";
        return "Dusuk verimlilik - Varliklar yeterince degerlendirilemiyor";
    }

    private string GetFinansalKaldiracYorum()
    {
        if (FinansalKaldirac > 3) return "Yuksek borc! Risk seviyesi yuksek";
        if (FinansalKaldirac > 2) return "Orta-yuksek borc orani - Dikkatli yonetim gerekli";
        if (FinansalKaldirac > 1.5m) return "Dengeli sermaye yapisi - Saglikli finansal yapi";
        return "Muhafazakar yapi - Dusuk risk, buyume potansiyeli var";
    }

    private string GetROEYorum()
    {
        if (ROE > 25) return "Mukemmel! Sermaye cok verimli kullaniliyor";
        if (ROE > 15) return "Cok iyi - Yatirimcilar icin caziplik yuksek";
        if (ROE > 10) return "Iyi - Sektore gore degerlendirmeli";
        if (ROE > 0) return "Dusuk - Iyilestirme gerektiriyor";
        return "Negatif ROE - Sermaye erimesi yasaniyor";
    }
}

/// <summary>
/// SWOT Analizi Sonuclari
/// </summary>
public class SwotAnaliziResult
{
    public int Yil { get; set; }
    public string FirmaAdi { get; set; } = string.Empty;

    public List<SwotItem> GucluYonler { get; set; } = new();      // Strengths
    public List<SwotItem> ZayifYonler { get; set; } = new();      // Weaknesses
    public List<SwotItem> Firsatlar { get; set; } = new();        // Opportunities
    public List<SwotItem> Tehditler { get; set; } = new();        // Threats

    public string GenelDegerlendirme { get; set; } = string.Empty;
    public List<string> StratejikOneriler { get; set; } = new();
}

public class SwotItem
{
    public string Baslik { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public int OnemDerecesi { get; set; } // 1-5
    public string Kaynak { get; set; } = string.Empty; // Hangi veriden cikarildi
}

/// <summary>
/// Trend Analizi Sonuclari
/// </summary>
public class TrendAnaliziResult
{
    public string FirmaAdi { get; set; } = string.Empty;
    public int BaslangicYil { get; set; }
    public int BitisYil { get; set; }

    public List<TrendVeri> GelirTrendi { get; set; } = new();
    public List<TrendVeri> GiderTrendi { get; set; } = new();
    public List<TrendVeri> KarTrendi { get; set; } = new();
    public List<TrendVeri> KarMarjiTrendi { get; set; } = new();

    public decimal GelirBuyumeOrtalamasý { get; set; }
    public decimal GiderBuyumeOrtalamasi { get; set; }
    public decimal KarBuyumeOrtalamasi { get; set; }

    public TrendYonu GelirTrendYonu { get; set; }
    public TrendYonu KarTrendYonu { get; set; }

    public string Degerlendirme { get; set; } = string.Empty;
}

public class TrendVeri
{
    public int Yil { get; set; }
    public decimal Deger { get; set; }
    public decimal DegisimOrani { get; set; } // Onceki yila gore %
    public int Endeks { get; set; } // Baz yil = 100
}

public enum TrendYonu
{
    Yukselis = 1,
    Dusus = 2,
    Yatay = 3,
    Dalgali = 4
}