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
        await SeedEmployeesAsync();
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
    /// Ornek personel ekler
    /// </summary>
    public async Task SeedEmployeesAsync()
    {
        if (await _context.Employees.AnyAsync())
            return;

        var company = await _context.Companies.FirstOrDefaultAsync();
        if (company == null)
            return;

        var now = DateTime.UtcNow;
        var employees = new List<Employee>
        {
            new Employee
            {
                CompanyId = company.Id,
                EmployeeNumber = "P2025-0001",
                FirstName = "Ahmet",
                LastName = "Yilmaz",
                TcKimlikNo = "12345678901",
                BirthDate = new DateTime(1985, 5, 15),
                Gender = Gender.Male,
                MaritalStatus = MaritalStatus.Married,
                NumberOfChildren = 2,
                Phone = "0532 123 4567",
                Email = "ahmet.yilmaz@abcteknoloji.com",
                SgkNumber = "1234567890",
                Department = "Muhasebe",
                Position = "Muhasebe Muduru",
                HireDate = new DateTime(2020, 1, 15),
                EmploymentType = EmploymentType.Permanent,
                WorkType = WorkType.Office,
                GrossSalary = 45000,
                SalaryType = SalaryType.Monthly,
                IsActive = true,
                CreatedAt = now
            },
            new Employee
            {
                CompanyId = company.Id,
                EmployeeNumber = "P2025-0002",
                FirstName = "Ayse",
                LastName = "Demir",
                TcKimlikNo = "98765432109",
                BirthDate = new DateTime(1990, 8, 20),
                Gender = Gender.Female,
                MaritalStatus = MaritalStatus.Single,
                NumberOfChildren = 0,
                Phone = "0533 234 5678",
                Email = "ayse.demir@abcteknoloji.com",
                SgkNumber = "9876543210",
                Department = "Insan Kaynaklari",
                Position = "IK Uzmani",
                HireDate = new DateTime(2021, 3, 1),
                EmploymentType = EmploymentType.Permanent,
                WorkType = WorkType.Office,
                GrossSalary = 35000,
                SalaryType = SalaryType.Monthly,
                IsActive = true,
                CreatedAt = now
            },
            new Employee
            {
                CompanyId = company.Id,
                EmployeeNumber = "P2025-0003",
                FirstName = "Mehmet",
                LastName = "Kaya",
                TcKimlikNo = "55566677788",
                BirthDate = new DateTime(1992, 3, 10),
                Gender = Gender.Male,
                MaritalStatus = MaritalStatus.Married,
                NumberOfChildren = 1,
                Phone = "0534 345 6789",
                Email = "mehmet.kaya@abcteknoloji.com",
                SgkNumber = "5556667778",
                Department = "Yazilim",
                Position = "Yazilim Gelistirici",
                HireDate = new DateTime(2022, 6, 15),
                EmploymentType = EmploymentType.Permanent,
                WorkType = WorkType.Remote,
                GrossSalary = 55000,
                SalaryType = SalaryType.Monthly,
                IsActive = true,
                CreatedAt = now
            },
            new Employee
            {
                CompanyId = company.Id,
                EmployeeNumber = "P2025-0004",
                FirstName = "Fatma",
                LastName = "Ozturk",
                TcKimlikNo = "11122233344",
                BirthDate = new DateTime(1995, 11, 5),
                Gender = Gender.Female,
                MaritalStatus = MaritalStatus.Single,
                NumberOfChildren = 0,
                Phone = "0535 456 7890",
                Email = "fatma.ozturk@abcteknoloji.com",
                SgkNumber = "1112223334",
                Department = "Satis",
                Position = "Satis Temsilcisi",
                HireDate = new DateTime(2023, 9, 1),
                EmploymentType = EmploymentType.Permanent,
                WorkType = WorkType.Field,
                GrossSalary = 28000,
                SalaryType = SalaryType.Monthly,
                IsActive = true,
                CreatedAt = now
            },
            new Employee
            {
                CompanyId = company.Id,
                EmployeeNumber = "P2025-0005",
                FirstName = "Ali",
                LastName = "Celik",
                TcKimlikNo = "44455566677",
                BirthDate = new DateTime(1998, 7, 25),
                Gender = Gender.Male,
                MaritalStatus = MaritalStatus.Single,
                NumberOfChildren = 0,
                Phone = "0536 567 8901",
                Email = "ali.celik@abcteknoloji.com",
                SgkNumber = "4445556667",
                Department = "Uretim",
                Position = "Uretim Operatoru",
                HireDate = new DateTime(2024, 1, 10),
                EmploymentType = EmploymentType.Permanent,
                WorkType = WorkType.Office,
                GrossSalary = 22104.67m, // 2025 Asgari ucret
                SalaryType = SalaryType.Monthly,
                IsMinimumWageExempt = true,
                IsActive = true,
                CreatedAt = now
            }
        };

        await _context.Employees.AddRangeAsync(employees);
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
            ("120", "ALICILAR"),
            ("120.01", "Yurtici Alicilar"),
            ("121", "ALACAK SENETLERI"),
            ("150", "ILK MADDE VE MALZEME"),
            ("153", "TICARI MALLAR"),
            ("180", "GELECEK AYLARA AIT GIDERLER"),
            ("190", "DEVREDEN KDV"),
            ("191", "INDIRILECEK KDV"),
            ("193", "PESIN ODENEN VERGILER"),
            ("196", "PERSONEL AVANSLARI"),
        });

        // 2 - DURAN VARLIKLAR
        AddAccountGroup(accounts, companyId, now, AccountType.Aktif, AccountNature.Debit, new[]
        {
            ("252", "BINALAR"),
            ("253", "TESIS MAKINA VE CIHAZLAR"),
            ("254", "TASITLAR"),
            ("255", "DEMIRBASLAR"),
            ("257", "BIRIKMIS AMORTISMANLAR (-)"),
        });

        // 3 - KISA VADELI YABANCI KAYNAKLAR
        AddAccountGroup(accounts, companyId, now, AccountType.Pasif, AccountNature.Credit, new[]
        {
            ("300", "BANKA KREDILERI"),
            ("320", "SATICILAR"),
            ("320.01", "Yurtici Saticilar"),
            ("321", "BORC SENETLERI"),
            ("335", "PERSONELE BORCLAR"),
            ("360", "ODENECEK VERGILER VE FONLAR"),
            ("361", "ODENECEK SOSYAL GUVENLIK KESINTILERI"),
            ("391", "HESAPLANAN KDV"),
        });

        // 5 - OZ KAYNAKLAR
        AddAccountGroup(accounts, companyId, now, AccountType.Pasif, AccountNature.Credit, new[]
        {
            ("500", "SERMAYE"),
            ("540", "YASAL YEDEKLER"),
            ("570", "GECMIS YILLAR KARLARI"),
            ("580", "GECMIS YILLAR ZARARLARI (-)"),
            ("590", "DONEM NET KARI"),
        });

        // 6 - GELIR TABLOSU HESAPLARI
        AddAccountGroup(accounts, companyId, now, AccountType.Gelir, AccountNature.Credit, new[]
        {
            ("600", "YURTICI SATISLAR"),
            ("601", "YURTDISI SATISLAR"),
            ("602", "DIGER GELIRLER"),
            ("642", "FAIZ GELIRLERI"),
            ("649", "DIGER OLAGAN GELIR VE KARLAR"),
        });

        // GIDERLER
        AddAccountGroup(accounts, companyId, now, AccountType.Gider, AccountNature.Debit, new[]
        {
            ("620", "SATILAN MAMULLER MALIYETI (-)"),
            ("621", "SATILAN TICARI MALLAR MALIYETI (-)"),
            ("631", "PAZARLAMA SATIS DAGITIM GIDERLERI (-)"),
            ("632", "GENEL YONETIM GIDERLERI (-)"),
            ("660", "KISA VADELI BORCLANMA GIDERLERI (-)"),
        });

        // 7 - MALIYET HESAPLARI
        AddAccountGroup(accounts, companyId, now, AccountType.Maliyet, AccountNature.Debit, new[]
        {
            ("720", "DIREKT ISCILIK GIDERLERI"),
            ("730", "GENEL URETIM GIDERLERI"),
            ("770", "GENEL YONETIM GIDERLERI"),
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
                AllowPosting = true,
                IsActive = true,
                CreatedAt = now
            });
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password + "AydaSalt2024");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}