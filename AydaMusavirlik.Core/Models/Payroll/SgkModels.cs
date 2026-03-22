using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Core.Models.Payroll;

/// <summary>
/// SGK Belge Turleri (RaptinBordro uyumlu)
/// </summary>
public class SgkBelgeTuru : BaseEntity
{
    public string Kod { get; set; } = string.Empty;           // 01, 02, 04, 07, vb.
    public string Adi { get; set; } = string.Empty;           // Normal, Emekli, Tarim, vb.
    public string? Aciklama { get; set; }

    // Isveren Hissesi Hesaplama Yontemi
    public IsverenHissesiTipi IsverenHissesiTipi { get; set; } = IsverenHissesiTipi.Degisken;
    public decimal IsverenHissesiSabitOran { get; set; }      // Sabit secilirse kullanilir

    // Kesinti Oranlari
    public decimal SigortaKesintiOrani { get; set; }          // Isci SGK orani (orn: 0.14)
    public decimal IssizlikIsciOrani { get; set; }            // Issizlik isci (orn: 0.01)
    public decimal IssizlikIsverenOrani { get; set; }         // Issizlik isveren (orn: 0.02)
    public decimal DamgaVergisiOrani { get; set; }            // Damga vergisi (orn: 0.00759)

    // SGK Isveren Paylari (Degisken hesaplama icin)
    public decimal AnalikSigortasiOrani { get; set; }         // Analik (orn: 0.01)
    public decimal HastalikSigortasiOrani { get; set; }       // Hastalik (orn: 0.0125)
    public decimal MalullukYaslilikOrani { get; set; }        // Malulluk-Yaslilik (orn: 0.11)
    public decimal IsKazasiOrani { get; set; }                // Is kazasi (tehlike sinifina gore)

    // Ucret Limitleri
    public decimal SgkTabanUcret { get; set; }                // SGK taban
    public decimal SgkTavanUcret { get; set; }                // SGK tavan

    // Kesinti Tercihleri
    public bool SigortaKesintisiVar { get; set; } = true;
    public bool IssizlikKesintisiVar { get; set; } = true;
    public bool DamgaVergisiVar { get; set; } = true;
    public bool GelirVergisiVar { get; set; } = true;
    public bool IsKazasindenMuaf { get; set; }

    // Navigation
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

/// <summary>
/// Isveren hissesi hesaplama tipi
/// </summary>
public enum IsverenHissesiTipi
{
    Degisken = 1,         // Analik + Hastalik + Malulluk + Is Kazasi
    Sabit = 2,            // Sabit oran (emekliler icin)
    IsKazasindenMuaf = 3  // Is kazasi haric degisken
}

/// <summary>
/// Kanuni Kesinti Parametreleri (Aylik/Yillik)
/// </summary>
public class KanuniKesinti : BaseEntity
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public DateTime GecerlilikBaslangic { get; set; }
    public DateTime? GecerlilikBitis { get; set; }

    // Asgari Ucret
    public decimal AsgariUcretBrut { get; set; }
    public decimal AsgariUcretNet { get; set; }

    // SGK Limitleri
    public decimal SgkTabanUcret { get; set; }
    public decimal SgkTavanUcret { get; set; }

    // Gelir Vergisi Dilimleri (JSON olarak saklanabilir)
    public decimal GelirVergisiDilim1Limit { get; set; }
    public decimal GelirVergisiDilim1Oran { get; set; }
    public decimal GelirVergisiDilim2Limit { get; set; }
    public decimal GelirVergisiDilim2Oran { get; set; }
    public decimal GelirVergisiDilim3Limit { get; set; }
    public decimal GelirVergisiDilim3Oran { get; set; }
    public decimal GelirVergisiDilim4Limit { get; set; }
    public decimal GelirVergisiDilim4Oran { get; set; }
    public decimal GelirVergisiDilim5Oran { get; set; }

    // Damga Vergisi
    public decimal DamgaVergisiOrani { get; set; }

    // SGK Oranlari
    public decimal SgkIsciOrani { get; set; }
    public decimal SgkIsverenOrani { get; set; }
    public decimal IssizlikIsciOrani { get; set; }
    public decimal IssizlikIsverenOrani { get; set; }

    // Calisma Bilgileri
    public int AylikCalismaSaati { get; set; } = 225;
    public decimal GunlukMesaiSaati { get; set; } = 7.5m;
    public decimal FazlaMesaiHaftaIciOrani { get; set; } = 1.5m;
    public decimal FazlaMesaiHaftaSonuOrani { get; set; } = 2.0m;
    public decimal FazlaMesaiTatilOrani { get; set; } = 2.0m;

    // AGI (Asgari Gecim Indirimi) - Kaldirildi ama eski kayitlar icin
    public decimal AgiBekarOran { get; set; }
    public decimal AgiEvliEsCalismiyorOran { get; set; }
    public decimal AgiCocukOran1 { get; set; }
    public decimal AgiCocukOran2 { get; set; }
    public decimal AgiCocukOran3 { get; set; }
}

/// <summary>
/// Puantaj Kaydi (Gunluk/Aylik calisma takibi)
/// </summary>
public class Puantaj : BaseEntity
{
    public int EmployeeId { get; set; }
    public int Yil { get; set; }
    public int Ay { get; set; }

    // Calisma Gunleri
    public int ToplamCalisilanGun { get; set; }
    public int HaftaSonuCalisilanGun { get; set; }
    public int ResmiTatilCalisilanGun { get; set; }
    public int UcretsizIzinGun { get; set; }
    public int UcretliIzinGun { get; set; }
    public int RaporluGun { get; set; }
    public int DevamsizlikGun { get; set; }

    // Mesai Saatleri
    public decimal NormalMesaiSaat { get; set; }
    public decimal FazlaMesaiHaftaIci { get; set; }
    public decimal FazlaMesaiHaftaSonu { get; set; }
    public decimal FazlaMesaiTatil { get; set; }

    // Hesaplanan Degerler
    public int SgkPrimGunu { get; set; }                      // SGK'ya bildirilecek gun
    public int GelirVergisiGunu { get; set; }                 // Gelir vergisi hesaplanacak gun

    // Notlar
    public string? Aciklama { get; set; }

    // Navigation
    public virtual Employee Employee { get; set; } = null!;
}

/// <summary>
/// Detayli Tahakkuk Kaydi
/// </summary>
public class TahakkukDetay : BaseEntity
{
    public int PayrollRecordId { get; set; }

    // Kazanclar
    public decimal BrutUcret { get; set; }
    public decimal FazlaMesaiUcreti { get; set; }
    public decimal PrimIkramiye { get; set; }
    public decimal YolYemekYardimi { get; set; }
    public decimal DigerKazanclar { get; set; }
    public decimal ToplamKazanc => BrutUcret + FazlaMesaiUcreti + PrimIkramiye + YolYemekYardimi + DigerKazanclar;

    // SGK Matrah
    public decimal SgkMatrah { get; set; }                    // SGK primine esas kazanc

    // Isci Kesintileri
    public decimal SgkIsciPayi { get; set; }
    public decimal IssizlikIsciPayi { get; set; }
    public decimal GelirVergisiMatrahi { get; set; }
    public decimal GelirVergisi { get; set; }
    public decimal DamgaVergisi { get; set; }
    public decimal SendikaAidati { get; set; }
    public decimal IcraKesintisi { get; set; }
    public decimal AvansKesintisi { get; set; }
    public decimal DigerKesintiler { get; set; }
    public decimal ToplamKesinti => SgkIsciPayi + IssizlikIsciPayi + GelirVergisi + DamgaVergisi + 
                                     SendikaAidati + IcraKesintisi + AvansKesintisi + DigerKesintiler;

    // Net Ucret
    public decimal NetUcret => ToplamKazanc - ToplamKesinti;

    // Isveren Maliyeti
    public decimal SgkIsverenPayi { get; set; }
    public decimal IssizlikIsverenPayi { get; set; }
    public decimal ToplamIsverenMaliyeti => ToplamKazanc + SgkIsverenPayi + IssizlikIsverenPayi;

    // Kumulatif Degerler (Yillik toplam - vergi hesabi icin)
    public decimal KumulatifGelirVergisiMatrahi { get; set; }
    public decimal KumulatifGelirVergisi { get; set; }

    // Navigation
    public virtual PayrollRecord PayrollRecord { get; set; } = null!;
}