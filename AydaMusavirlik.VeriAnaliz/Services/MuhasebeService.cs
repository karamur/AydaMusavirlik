using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.VeriAnaliz.Data;
using AydaMusavirlik.VeriAnaliz.Models;

namespace AydaMusavirlik.VeriAnaliz.Services;

/// <summary>
/// Muhasebe islemleri servisi
/// </summary>
public interface IMuhasebeService
{
    Task<List<GelirGiderKayit>> GetAllAsync(int firmaId);
    Task<List<GelirGiderKayit>> GetByFilterAsync(AnalizFiltre filtre);
    Task<GelirGiderKayit?> GetByIdAsync(int id);
    Task<GelirGiderKayit> CreateAsync(GelirGiderKayit kayit);
    Task<GelirGiderKayit> UpdateAsync(GelirGiderKayit kayit);
    Task<bool> DeleteAsync(int id);
    Task<MuhasebeOzet> GetOzetAsync(int firmaId, DateTime? baslangic = null, DateTime? bitis = null);
    Task<List<KategoriOzet>> GetKategoriOzetAsync(int firmaId, DateTime? baslangic = null, DateTime? bitis = null);
    Task<List<KasaOzet>> GetKasaOzetAsync(int firmaId, DateTime? baslangic = null, DateTime? bitis = null);
    Task<List<AylikOzet>> GetAylikOzetAsync(int firmaId, int yil);
}

public class MuhasebeService : IMuhasebeService
{
    private readonly VeriAnalizDbContext _context;

    public MuhasebeService(VeriAnalizDbContext context)
    {
        _context = context;
    }

    public async Task<List<GelirGiderKayit>> GetAllAsync(int firmaId)
    {
        return await _context.GelirGiderKayitlari
            .Where(k => k.FirmaId == firmaId)
            .OrderByDescending(k => k.Tarih)
            .ThenByDescending(k => k.Id)
            .ToListAsync();
    }

    public async Task<List<GelirGiderKayit>> GetByFilterAsync(AnalizFiltre filtre)
    {
        var query = _context.GelirGiderKayitlari.AsQueryable();

        if (filtre.FirmaId.HasValue)
            query = query.Where(k => k.FirmaId == filtre.FirmaId.Value);

        if (filtre.BaslangicTarihi.HasValue)
            query = query.Where(k => k.Tarih >= filtre.BaslangicTarihi.Value);

        if (filtre.BitisTarihi.HasValue)
            query = query.Where(k => k.Tarih <= filtre.BitisTarihi.Value);

        if (!string.IsNullOrEmpty(filtre.Tur))
            query = query.Where(k => k.Tur == filtre.Tur);

        if (!string.IsNullOrEmpty(filtre.Kategori))
            query = query.Where(k => k.Kategori == filtre.Kategori);

        if (!string.IsNullOrEmpty(filtre.Kasa))
            query = query.Where(k => k.Kasa == filtre.Kasa);

        return await query
            .OrderByDescending(k => k.Tarih)
            .ThenByDescending(k => k.Id)
            .ToListAsync();
    }

    public async Task<GelirGiderKayit?> GetByIdAsync(int id)
    {
        return await _context.GelirGiderKayitlari.FindAsync(id);
    }

    public async Task<GelirGiderKayit> CreateAsync(GelirGiderKayit kayit)
    {
        kayit.OlusturmaTarihi = DateTime.Now;
        _context.GelirGiderKayitlari.Add(kayit);
        await _context.SaveChangesAsync();
        return kayit;
    }

    public async Task<GelirGiderKayit> UpdateAsync(GelirGiderKayit kayit)
    {
        kayit.GuncellemeTarihi = DateTime.Now;
        _context.GelirGiderKayitlari.Update(kayit);
        await _context.SaveChangesAsync();
        return kayit;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var kayit = await _context.GelirGiderKayitlari.FindAsync(id);
        if (kayit == null) return false;

        kayit.IsDeleted = true;
        kayit.GuncellemeTarihi = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MuhasebeOzet> GetOzetAsync(int firmaId, DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _context.GelirGiderKayitlari.Where(k => k.FirmaId == firmaId);

        if (baslangic.HasValue)
            query = query.Where(k => k.Tarih >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(k => k.Tarih <= bitis.Value);

        var kayitlar = await query.ToListAsync();

        return new MuhasebeOzet
        {
            ToplamGelir = kayitlar.Where(k => k.Tur == "Gelir").Sum(k => k.Tutar),
            ToplamGider = kayitlar.Where(k => k.Tur == "Gider").Sum(k => k.Tutar),
            KayitSayisi = kayitlar.Count,
            SonIslemTarihi = kayitlar.MaxBy(k => k.Tarih)?.Tarih
        };
    }

    public async Task<List<KategoriOzet>> GetKategoriOzetAsync(int firmaId, DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _context.GelirGiderKayitlari.Where(k => k.FirmaId == firmaId);

        if (baslangic.HasValue)
            query = query.Where(k => k.Tarih >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(k => k.Tarih <= bitis.Value);

        var kayitlar = await query.ToListAsync();

        return kayitlar
            .GroupBy(k => k.Kategori)
            .Select(g => new KategoriOzet
            {
                Kategori = g.Key,
                ToplamGelir = g.Where(k => k.Tur == "Gelir").Sum(k => k.Tutar),
                ToplamGider = g.Where(k => k.Tur == "Gider").Sum(k => k.Tutar),
                IslemSayisi = g.Count()
            })
            .OrderByDescending(k => k.Net)
            .ToList();
    }

    public async Task<List<KasaOzet>> GetKasaOzetAsync(int firmaId, DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _context.GelirGiderKayitlari.Where(k => k.FirmaId == firmaId);

        if (baslangic.HasValue)
            query = query.Where(k => k.Tarih >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(k => k.Tarih <= bitis.Value);

        var kayitlar = await query.ToListAsync();

        return kayitlar
            .GroupBy(k => k.Kasa)
            .Select(g => new KasaOzet
            {
                Kasa = g.Key,
                ToplamGelir = g.Where(k => k.Tur == "Gelir").Sum(k => k.Tutar),
                ToplamGider = g.Where(k => k.Tur == "Gider").Sum(k => k.Tutar),
                IslemSayisi = g.Count()
            })
            .OrderByDescending(k => k.Bakiye)
            .ToList();
    }

    public async Task<List<AylikOzet>> GetAylikOzetAsync(int firmaId, int yil)
    {
        var kayitlar = await _context.GelirGiderKayitlari
            .Where(k => k.FirmaId == firmaId && k.Tarih.Year == yil)
            .ToListAsync();

        var aylar = new[] { "Ocak", "Subat", "Mart", "Nisan", "Mayis", "Haziran",
                           "Temmuz", "Agustos", "Eylul", "Ekim", "Kasim", "Aralik" };

        return Enumerable.Range(1, 12)
            .Select(ay => new AylikOzet
            {
                Yil = yil,
                Ay = ay,
                AyAdi = aylar[ay - 1],
                ToplamGelir = kayitlar.Where(k => k.Tarih.Month == ay && k.Tur == "Gelir").Sum(k => k.Tutar),
                ToplamGider = kayitlar.Where(k => k.Tarih.Month == ay && k.Tur == "Gider").Sum(k => k.Tutar),
                IslemSayisi = kayitlar.Count(k => k.Tarih.Month == ay)
            })
            .ToList();
    }
}
