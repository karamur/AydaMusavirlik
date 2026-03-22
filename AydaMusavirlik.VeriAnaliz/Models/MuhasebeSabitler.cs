namespace AydaMusavirlik.VeriAnaliz.Models;

/// <summary>
/// Muhasebe sabit degerleri
/// </summary>
public static class MuhasebeSabitler
{
    /// <summary>
    /// Gelir kategorileri
    /// </summary>
    public static readonly string[] GelirKategorileri =
    {
        "Satis Geliri",
        "Hizmet Geliri",
        "Faiz Geliri",
        "Kira Geliri",
        "Komisyon Geliri",
        "Diger Gelirler"
    };

    /// <summary>
    /// Gider kategorileri
    /// </summary>
    public static readonly string[] GiderKategorileri =
    {
        "Personel Gideri",
        "Kira Gideri",
        "Elektrik/Su/Dogalgaz",
        "Telefon/Internet",
        "Ulasim/Akaryakit",
        "Vergi/Harç",
        "Sigorta",
        "Bakim/Onarim",
        "Kirtasiye",
        "Reklam/Pazarlama",
        "Banka Masraflari",
        "Diger Giderler"
    };

    /// <summary>
    /// Kasa tipleri
    /// </summary>
    public static readonly string[] KasaTipleri =
    {
        "Nakit",
        "Banka",
        "Kredi Karti",
        "Cek/Senet",
        "Diger"
    };

    /// <summary>
    /// Islem turleri
    /// </summary>
    public static readonly string[] IslemTurleri =
    {
        "Gelir",
        "Gider"
    };
}
