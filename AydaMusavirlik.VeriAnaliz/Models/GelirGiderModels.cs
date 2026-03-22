namespace AydaMusavirlik.VeriAnaliz.Models;

/// <summary>
/// Gelir/Gider kayit modeli
/// </summary>
public class GelirGiderKayit
{
    public int Id { get; set; }
    public int FirmaId { get; set; }
    public DateTime Tarih { get; set; }
    public string Tur { get; set; } = "Gelir"; // Gelir veya Gider
    public string Kategori { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public decimal Tutar { get; set; }
    public string Kasa { get; set; } = "Nakit";
    public string? BelgeNo { get; set; }
    public string? Notlar { get; set; }
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    public DateTime? GuncellemeTarihi { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Muhasebe ozet bilgileri
/// </summary>
public class MuhasebeOzet
{
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal Guncel => ToplamGelir - ToplamGider;
    public int KayitSayisi { get; set; }
    public DateTime? SonIslemTarihi { get; set; }
}

/// <summary>
/// Kategori bazli ozet
/// </summary>
public class KategoriOzet
{
    public string Kategori { get; set; } = string.Empty;
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public int IslemSayisi { get; set; }
    public decimal Net => ToplamGelir - ToplamGider;
}

/// <summary>
/// Kasa bazli ozet
/// </summary>
public class KasaOzet
{
    public string Kasa { get; set; } = string.Empty;
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public int IslemSayisi { get; set; }
    public decimal Bakiye => ToplamGelir - ToplamGider;
}

/// <summary>
/// Aylik ozet
/// </summary>
public class AylikOzet
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public string AyAdi { get; set; } = string.Empty;
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal Net => ToplamGelir - ToplamGider;
    public int IslemSayisi { get; set; }
}

/// <summary>
/// Analiz filtresi
/// </summary>
public class AnalizFiltre
{
    public int? FirmaId { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public string? Tur { get; set; }
    public string? Kategori { get; set; }
    public string? Kasa { get; set; }
}
