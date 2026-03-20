using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Common;
using AydaMusavirlik.Core.Models.Accounting;

namespace AydaMusavirlik.Data.Services;

/// <summary>
/// Veritabanina ornek veri ekleyen servis
/// </summary>
public class SeedDataService
{
    private readonly AppDbContext _context;

    public SeedDataService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tum seed verilerini ekler
    /// </summary>
    public async Task SeedAllAsync()
    {
        await SeedUsersAsync();
        await SeedCompaniesAsync();
        await SeedAccountPlanAsync();
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Varsayilan kullanicilari ekler
    /// </summary>
    public async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync())
            return;

        var users = new List<User>
        {
            new User
            {
                Username = "admin",
                Email = "admin@aydamusavirlik.com",
                PasswordHash = HashPassword("Admin123!"),
                FirstName = "Sistem",
                LastName = "Yoneticisi",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Username = "muhasebeci",
                Email = "muhasebe@aydamusavirlik.com",
                PasswordHash = HashPassword("Muhasebe123!"),
                FirstName = "Ahmet",
                LastName = "Yilmaz",
                Role = UserRole.Accountant,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Username = "kullanici",
                Email = "kullanici@aydamusavirlik.com",
                PasswordHash = HashPassword("Kullanici123!"),
                FirstName = "Mehmet",
                LastName = "Demir",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Users.AddRangeAsync(users);
    }

    /// <summary>
    /// Ornek firmalari ekler
    /// </summary>
    public async Task SeedCompaniesAsync()
    {
        if (await _context.Companies.AnyAsync())
            return;

        var companies = new List<Company>
        {
            new Company
            {
                Name = "ABC Teknoloji A.S.",
                TaxNumber = "1234567890",
                TaxOffice = "Kadikoy",
                Address = "Bagdat Cad. No:123 Kadikoy/Istanbul",
                Phone = "0216 555 1234",
                Email = "info@abcteknoloji.com",
                CompanyType = CompanyType.AnonimSirket,
                Capital = 1000000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Company
            {
                Name = "XYZ Danismanlik Ltd. Sti.",
                TaxNumber = "9876543210",
                TaxOffice = "Besiktas",
                Address = "Levent Mah. Is Merkezi Besiktas/Istanbul",
                Phone = "0212 555 5678",
                Email = "info@xyzdanismanlik.com",
                CompanyType = CompanyType.LimitedSirketi,
                Capital = 500000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Company
            {
                Name = "Demo Ticaret",
                TaxNumber = "5555555555",
                TaxOffice = "Uskudar",
                Address = "Uskudar Cad. No:45 Uskudar/Istanbul",
                Phone = "0216 555 9999",
                Email = "info@demoticaret.com",
                CompanyType = CompanyType.SahisFirmasi,
                Capital = 100000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Companies.AddRangeAsync(companies);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Tekduzen Hesap Plani ekler
    /// </summary>
    public async Task SeedAccountPlanAsync()
    {
        if (await _context.Accounts.AnyAsync())
            return;

        var company = await _context.Companies.FirstOrDefaultAsync();
        if (company == null)
            return;

        var accounts = GetTekduzenHesapPlani(company.Id);
        await _context.Accounts.AddRangeAsync(accounts);
    }

    /// <summary>
    /// Tekduzen Hesap Plani
    /// </summary>
    private List<Account> GetTekduzenHesapPlani(int companyId)
    {
        var accounts = new List<Account>();
        var now = DateTime.UtcNow;

        // 1 - DONEN VARLIKLAR
        AddAccountGroup(accounts, companyId, now, AccountType.Aktif, AccountNature.Debit, new[]
        {
            ("100", "KASA"),
            ("100.01", "TL Kasa"),
            ("101", "ALINAN CEKLER"),
            ("102", "BANKALAR"),
            ("102.01", "Vadesiz TL Hesap"),
            ("102.02", "Vadeli TL Hesap"),
            ("103", "VERILEN CEKLER VE ODEME EMIRLERI"),
            ("108", "DIGER HAZIR DEGERLER"),
            ("120", "ALICILAR"),
            ("120.01", "Yurtici Alicilar"),
            ("121", "ALACAK SENETLERI"),
            ("126", "VERILEN DEPOZITO VE TEMINATLAR"),
            ("150", "ILK MADDE VE MALZEME"),
            ("151", "YARI MAMULLER - URETIM"),
            ("152", "MAMULLER"),
            ("153", "TICARI MALLAR"),
            ("180", "GELECEK AYLARA AIT GIDERLER"),
            ("190", "DEVREDEN KDV"),
            ("191", "INDIRILECEK KDV"),
            ("193", "PESIN ODENEN VERGILER VE FONLAR"),
            ("195", "IS AVANSLARI"),
            ("196", "PERSONEL AVANSLARI"),
        });

        // 2 - DURAN VARLIKLAR
        AddAccountGroup(accounts, companyId, now, AccountType.Aktif, AccountNature.Debit, new[]
        {
            ("250", "ARAZILER VE ARSALAR"),
            ("252", "BINALAR"),
            ("253", "TESIS MAKINA VE CIHAZLAR"),
            ("254", "TASITLAR"),
            ("255", "DEMIRBASLAR"),
            ("257", "BIRIKMIS AMORTISMANLAR (-)"),
            ("260", "HAKLAR"),
            ("263", "ARASTIRMA VE GELISTIRME GIDERLERI"),
            ("268", "BIRIKMIS AMORTISMANLAR (-)"),
        });

        // 3 - KISA VADELI YABANCI KAYNAKLAR
        AddAccountGroup(accounts, companyId, now, AccountType.Pasif, AccountNature.Credit, new[]
        {
            ("300", "BANKA KREDILERI"),
            ("320", "SATICILAR"),
            ("320.01", "Yurtici Saticilar"),
            ("321", "BORC SENETLERI"),
            ("326", "ALINAN DEPOZITO VE TEMINATLAR"),
            ("335", "PERSONELE BORCLAR"),
            ("336", "DIGER CESITLI BORCLAR"),
            ("340", "ALINAN SIPARIS AVANSLARI"),
            ("360", "ODENECEK VERGILER VE FONLAR"),
            ("361", "ODENECEK SOSYAL GUVENLIK KESINTILERI"),
            ("370", "DONEM KARI VERGI KARSILIKLARI"),
            ("372", "KIDEM TAZMINATI KARSILIGI"),
            ("380", "GELECEK AYLARA AIT GELIRLER"),
            ("381", "GIDER TAHAKKUKLARI"),
            ("391", "HESAPLANAN KDV"),
        });

        // 4 - UZUN VADELI YABANCI KAYNAKLAR
        AddAccountGroup(accounts, companyId, now, AccountType.Pasif, AccountNature.Credit, new[]
        {
            ("400", "BANKA KREDILERI"),
            ("420", "SATICILAR"),
            ("421", "BORC SENETLERI"),
            ("472", "KIDEM TAZMINATI KARSILIGI"),
            ("480", "GELECEK YILLARA AIT GELIRLER"),
        });

        // 5 - OZ KAYNAKLAR
        AddAccountGroup(accounts, companyId, now, AccountType.Pasif, AccountNature.Credit, new[]
        {
            ("500", "SERMAYE"),
            ("501", "ODENMEMIS SERMAYE (-)"),
            ("520", "HISSE SENETLERI IHRAC PRIMLERI"),
            ("540", "YASAL YEDEKLER"),
            ("542", "OLAGANUSTU YEDEKLER"),
            ("570", "GECMIS YILLAR KARLARI"),
            ("580", "GECMIS YILLAR ZARARLARI (-)"),
            ("590", "DONEM NET KARI"),
            ("591", "DONEM NET ZARARI (-)"),
        });

        // 6 - GELIR TABLOSU HESAPLARI
        AddAccountGroup(accounts, companyId, now, AccountType.Gelir, AccountNature.Credit, new[]
        {
            ("600", "YURTICI SATISLAR"),
            ("601", "YURTDISI SATISLAR"),
            ("602", "DIGER GELIRLER"),
            ("610", "SATISTAN IADELER (-)"),
            ("611", "SATIS ISKONTALARI (-)"),
            ("642", "FAIZ GELIRLERI"),
            ("646", "KAMBIYO KARLARI"),
            ("649", "DIGER OLAGAN GELIR VE KARLAR"),
        });

        // GIDERLER
        AddAccountGroup(accounts, companyId, now, AccountType.Gider, AccountNature.Debit, new[]
        {
            ("620", "SATILAN MAMULLER MALIYETI (-)"),
            ("621", "SATILAN TICARI MALLAR MALIYETI (-)"),
            ("622", "SATILAN HIZMET MALIYETI (-)"),
            ("630", "ARASTIRMA VE GELISTIRME GIDERLERI (-)"),
            ("631", "PAZARLAMA SATIS VE DAGITIM GIDERLERI (-)"),
            ("632", "GENEL YONETIM GIDERLERI (-)"),
            ("654", "KARSILIK GIDERLERI (-)"),
            ("656", "KAMBIYO ZARARLARI (-)"),
            ("659", "DIGER OLAGAN GIDER VE ZARARLAR (-)"),
            ("660", "KISA VADELI BORCLANMA GIDERLERI (-)"),
            ("661", "UZUN VADELI BORCLANMA GIDERLERI (-)"),
            ("689", "DIGER OLAGANDISI GIDER VE ZARARLAR (-)"),
            ("691", "DONEM KARI VERGI VE DIGER YASAL YUKUMLULUK KARSILIKLARI (-)"),
        });

        // 7 - MALIYET HESAPLARI
        AddAccountGroup(accounts, companyId, now, AccountType.Maliyet, AccountNature.Debit, new[]
        {
            ("700", "MALIYET MUHASEBESI BAGLANTI HESABI"),
            ("710", "DIREKT ILK MADDE VE MALZEME GIDERLERI"),
            ("720", "DIREKT ISCILIK GIDERLERI"),
            ("730", "GENEL URETIM GIDERLERI"),
            ("750", "ARASTIRMA VE GELISTIRME GIDERLERI"),
            ("760", "PAZARLAMA SATIS VE DAGITIM GIDERLERI"),
            ("770", "GENEL YONETIM GIDERLERI"),
            ("780", "FINANSMAN GIDERLERI"),
        });

        return accounts;
    }

    private void AddAccountGroup(List<Account> accounts, int companyId, DateTime now, 
        AccountType accountType, AccountNature nature, (string Code, string Name)[] items)
    {
        foreach (var (code, name) in items)
        {
            var level = code.Contains('.') ? 2 : 1;
            accounts.Add(new Account
            {
                CompanyId = companyId,
                Code = code,
                Name = name,
                AccountType = accountType,
                Nature = nature,
                Level = level,
                IsHeader = level == 1,
                AllowPosting = level > 1 || !code.Contains('.'),
                IsActive = true,
                CreatedAt = now
            });
        }
    }

    private string HashPassword(string password)
    {
        // Basit hash - production'da BCrypt kullanilmali
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password + "AydaSalt2024");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}