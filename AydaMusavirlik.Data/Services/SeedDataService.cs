using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Core.Models.Common;
using AydaMusavirlik.Core.Models.Accounting;
using AydaMusavirlik.Core.Models.Payroll;

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
        await SeedSgkBelgeTurleriAsync();
        await SeedKanuniKesintilerAsync();
        await SeedEmployeesAsync();
        await _context.SaveChangesAsync();
    }

    public async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync()) return;

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
            }
        };
        await _context.Users.AddRangeAsync(users);
    }

    public async Task SeedCompaniesAsync()
    {
        if (await _context.Companies.AnyAsync()) return;

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
            }
        };
        await _context.Companies.AddRangeAsync(companies);
        await _context.SaveChangesAsync();
    }

    public async Task SeedAccountPlanAsync()
    {
        if (await _context.Accounts.AnyAsync()) return;

        var company = await _context.Companies.FirstOrDefaultAsync();
        if (company == null) return;

        var now = DateTime.UtcNow;
        var accounts = new List<Account>
        {
            // 1 - DONEN VARLIKLAR
            new Account { CompanyId = company.Id, Code = "100", Name = "KASA", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, IsActive = true, CreatedAt = now },
            new Account { CompanyId = company.Id, Code = "102", Name = "BANKALAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, IsActive = true, CreatedAt = now },
            new Account { CompanyId = company.Id, Code = "120", Name = "ALICILAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, IsActive = true, CreatedAt = now },
            new Account { CompanyId = company.Id, Code = "153", Name = "TICARI MALLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, IsActive = true, CreatedAt = now },
            new Account { CompanyId = company.Id, Code = "191", Name = "INDIRILECEK KDV", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, IsActive = true, CreatedAt = now },
            // 2 - DURAN VARLIKLAR
            new Account { CompanyId = company.Id, Code = "254", Name = "TASITLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, IsActive = true, CreatedAt = now },
            new Account { CompanyId = company.Id, Code = "255", Name = "DEMIRBASLAR", AccountType = AccountType.Aktif, Nature = AccountNature.Debit, Level = 1, IsActive = true, CreatedAt = now },
            // 3 - KISA VADELI YABANCI KAYNAKLAR
            new Account { CompanyId = company.Id, Code = "320", Name = "SATICILAR", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, IsActive = true, CreatedAt = now },
            new Account { CompanyId = company.Id, Code = "335", Name = "PERSONELE BORCLAR", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, IsActive = true, CreatedAt = now },
            new Account { CompanyId = company.Id, Code = "360", Name = "ODENECEK VERGILER", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, IsActive = true, CreatedAt = now },
            new Account { CompanyId = company.Id, Code = "361", Name = "ODENECEK SGK", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, IsActive = true, CreatedAt = now },
            new Account { CompanyId = company.Id, Code = "391", Name = "HESAPLANAN KDV", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, IsActive = true, CreatedAt = now },
            // 5 - OZ KAYNAKLAR
            new Account { CompanyId = company.Id, Code = "500", Name = "SERMAYE", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, IsActive = true, CreatedAt = now },
            new Account { CompanyId = company.Id, Code = "590", Name = "DONEM NET KARI", AccountType = AccountType.Pasif, Nature = AccountNature.Credit, Level = 1, IsActive = true, CreatedAt = now },
            // 6 - GELIRLER
            new Account { CompanyId = company.Id, Code = "600", Name = "YURTICI SATISLAR", AccountType = AccountType.Gelir, Nature = AccountNature.Credit, Level = 1, IsActive = true, CreatedAt = now },
            // 7 - GIDERLER
            new Account { CompanyId = company.Id, Code = "720", Name = "DIREKT ISCILIK GIDERLERI", AccountType = AccountType.Maliyet, Nature = AccountNature.Debit, Level = 1, IsActive = true, CreatedAt = now },
            new Account { CompanyId = company.Id, Code = "770", Name = "GENEL YONETIM GIDERLERI", AccountType = AccountType.Maliyet, Nature = AccountNature.Debit, Level = 1, IsActive = true, CreatedAt = now },
        };
        await _context.Accounts.AddRangeAsync(accounts);
    }

    public async Task SeedSgkBelgeTurleriAsync()
    {
        if (await _context.SgkBelgeTurleri.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var belgeTurleri = new List<SgkBelgeTuru>
        {
            new SgkBelgeTuru
            {
                Kod = "01", Adi = "Normal", Aciklama = "Normal calisan (4/a)",
                IsverenHissesiTipi = IsverenHissesiTipi.Degisken,
                SigortaKesintiOrani = 0.14m, IssizlikIsciOrani = 0.01m, IssizlikIsverenOrani = 0.02m,
                DamgaVergisiOrani = 0.00759m, AnalikSigortasiOrani = 0.01m, HastalikSigortasiOrani = 0.0125m,
                MalullukYaslilikOrani = 0.11m, IsKazasiOrani = 0.02m,
                SgkTabanUcret = 22104.67m, SgkTavanUcret = 165785.00m,
                SigortaKesintisiVar = true, IssizlikKesintisiVar = true, DamgaVergisiVar = true, GelirVergisiVar = true,
                IsActive = true, CreatedAt = now
            },
            new SgkBelgeTuru
            {
                Kod = "02", Adi = "Emekli", Aciklama = "Emekli calisan (SGDP)",
                IsverenHissesiTipi = IsverenHissesiTipi.Sabit, IsverenHissesiSabitOran = 0.225m,
                SigortaKesintiOrani = 0.075m, IssizlikIsciOrani = 0m, IssizlikIsverenOrani = 0m,
                DamgaVergisiOrani = 0.00759m,
                SgkTabanUcret = 22104.67m, SgkTavanUcret = 165785.00m,
                SigortaKesintisiVar = true, IssizlikKesintisiVar = false, DamgaVergisiVar = true, GelirVergisiVar = true,
                IsActive = true, CreatedAt = now
            },
            new SgkBelgeTuru
            {
                Kod = "13", Adi = "Stajyer", Aciklama = "Stajyer ogrenci",
                IsverenHissesiTipi = IsverenHissesiTipi.IsKazasindenMuaf,
                SigortaKesintiOrani = 0m, IssizlikIsciOrani = 0m, IssizlikIsverenOrani = 0m, DamgaVergisiOrani = 0m,
                SgkTabanUcret = 22104.67m, SgkTavanUcret = 165785.00m,
                SigortaKesintisiVar = false, IssizlikKesintisiVar = false, DamgaVergisiVar = false, GelirVergisiVar = false,
                IsActive = true, CreatedAt = now
            }
        };
        await _context.SgkBelgeTurleri.AddRangeAsync(belgeTurleri);
    }

    public async Task SeedKanuniKesintilerAsync()
    {
        if (await _context.KanuniKesintiler.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var kesinti = new KanuniKesinti
        {
            Yil = 2025, Ay = 1,
            GecerlilikBaslangic = new DateTime(2025, 1, 1),
            GecerlilikBitis = new DateTime(2025, 6, 30),
            AsgariUcretBrut = 22104.67m, AsgariUcretNet = 17002.12m,
            SgkTabanUcret = 22104.67m, SgkTavanUcret = 165785.00m,
            GelirVergisiDilim1Limit = 158000m, GelirVergisiDilim1Oran = 0.15m,
            GelirVergisiDilim2Limit = 330000m, GelirVergisiDilim2Oran = 0.20m,
            GelirVergisiDilim3Limit = 800000m, GelirVergisiDilim3Oran = 0.27m,
            GelirVergisiDilim4Limit = 2000000m, GelirVergisiDilim4Oran = 0.35m,
            GelirVergisiDilim5Oran = 0.40m,
            DamgaVergisiOrani = 0.00759m,
            SgkIsciOrani = 0.14m, SgkIsverenOrani = 0.205m,
            IssizlikIsciOrani = 0.01m, IssizlikIsverenOrani = 0.02m,
            AylikCalismaSaati = 225, GunlukMesaiSaati = 7.5m,
            FazlaMesaiHaftaIciOrani = 1.5m, FazlaMesaiHaftaSonuOrani = 2.0m, FazlaMesaiTatilOrani = 2.0m,
            IsActive = true, CreatedAt = now
        };
        await _context.KanuniKesintiler.AddAsync(kesinti);
    }

    public async Task SeedEmployeesAsync()
    {
        if (await _context.Employees.AnyAsync()) return;

        var company = await _context.Companies.FirstOrDefaultAsync();
        var belgeTuru = await _context.SgkBelgeTurleri.FirstOrDefaultAsync(b => b.Kod == "01");
        if (company == null) return;

        var now = DateTime.UtcNow;
        var employees = new List<Employee>
        {
            new Employee
            {
                CompanyId = company.Id, SgkBelgeTuruId = belgeTuru?.Id,
                EmployeeNumber = "P2025-0001", FirstName = "Ahmet", LastName = "Yilmaz",
                TcKimlikNo = "12345678901", BirthDate = new DateTime(1985, 5, 15),
                Gender = Gender.Male, MaritalStatus = MaritalStatus.Married, NumberOfChildren = 2,
                Phone = "0532 123 4567", Email = "ahmet.yilmaz@abc.com", SgkNumber = "1234567890",
                Department = "Muhasebe", Position = "Muhasebe Muduru",
                HireDate = new DateTime(2020, 1, 15), SgkIseGirisTarihi = new DateTime(2020, 1, 15),
                EmploymentType = EmploymentType.Permanent, WorkType = WorkType.Office,
                GrossSalary = 45000, SalaryType = SalaryType.Monthly,
                IsActive = true, CreatedAt = now
            },
            new Employee
            {
                CompanyId = company.Id, SgkBelgeTuruId = belgeTuru?.Id,
                EmployeeNumber = "P2025-0002", FirstName = "Ayse", LastName = "Demir",
                TcKimlikNo = "98765432109", BirthDate = new DateTime(1990, 8, 20),
                Gender = Gender.Female, MaritalStatus = MaritalStatus.Single, NumberOfChildren = 0,
                Phone = "0533 234 5678", Email = "ayse.demir@abc.com", SgkNumber = "9876543210",
                Department = "IK", Position = "IK Uzmani",
                HireDate = new DateTime(2021, 3, 1), SgkIseGirisTarihi = new DateTime(2021, 3, 1),
                EmploymentType = EmploymentType.Permanent, WorkType = WorkType.Office,
                GrossSalary = 35000, SalaryType = SalaryType.Monthly,
                IsActive = true, CreatedAt = now
            },
            new Employee
            {
                CompanyId = company.Id, SgkBelgeTuruId = belgeTuru?.Id,
                EmployeeNumber = "P2025-0003", FirstName = "Ali", LastName = "Celik",
                TcKimlikNo = "44455566677", BirthDate = new DateTime(1998, 7, 25),
                Gender = Gender.Male, MaritalStatus = MaritalStatus.Single, NumberOfChildren = 0,
                Phone = "0536 567 8901", Email = "ali.celik@abc.com", SgkNumber = "4445556667",
                Department = "Uretim", Position = "Operator",
                HireDate = new DateTime(2024, 1, 10), SgkIseGirisTarihi = new DateTime(2024, 1, 10),
                EmploymentType = EmploymentType.Permanent, WorkType = WorkType.Office,
                GrossSalary = 22104.67m, SalaryType = SalaryType.Monthly,
                IsMinimumWageExempt = true, IsActive = true, CreatedAt = now
            }
        };
        await _context.Employees.AddRangeAsync(employees);
    }

    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password + "AydaSalt2024");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}