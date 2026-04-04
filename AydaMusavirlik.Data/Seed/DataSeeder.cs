using AydaMusavirlik.Core.Models.Accounting;
using AydaMusavirlik.Core.Models.Payroll;
using AydaMusavirlik.Core.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace AydaMusavirlik.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedUsersAsync(context);
        await SeedAccountsAsync(context);
        await SeedPayrollParametersAsync(context);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var users = new List<User>
        {
            new User
            {
                Username = "admin",
                PasswordHash = "admin123", // TODO: Hash this
                FirstName = "Sistem",
                LastName = "Yoneticisi",
                Email = "admin@ayda.com",
                Role = UserRole.Admin,
                IsActive = true
            },
            new User
            {
                Username = "muhasebeci",
                PasswordHash = "muh123",
                FirstName = "Ahmet",
                LastName = "Yilmaz",
                Email = "ahmet@ayda.com",
                Role = UserRole.Accountant,
                IsActive = true
            }
        };

        await context.Users.AddRangeAsync(users);
    }

    private static async Task SeedAccountsAsync(AppDbContext context)
    {
        if (await context.Accounts.AnyAsync()) return;

        var accounts = new List<Account>
        {
            // 1 - DONEN VARLIKLAR
            new Account { Code = "1", Name = "DONEN VARLIKLAR", Level = 1, IsHeader = true, AllowPosting = false, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            
            // 10 - HAZIR DEGERLER
            new Account { Code = "10", Name = "HAZIR DEGERLER", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "100", Name = "KASA", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "100.01", Name = "MERKEZ KASA", Level = 4, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "101", Name = "ALINAN CEKLER", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "102", Name = "BANKALAR", Level = 3, IsHeader = true, AllowPosting = false, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "102.01", Name = "VADESIZ MEVDUAT", Level = 4, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "102.02", Name = "VADELI MEVDUAT", Level = 4, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "103", Name = "VERILEN CEKLER VE ODEME EMIRLERI (-)", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Credit },
            
            // 11 - MENKUL KIYMETLER
            new Account { Code = "11", Name = "MENKUL KIYMETLER", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "110", Name = "HISSE SENETLERI", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            
            // 12 - TICARI ALACAKLAR
            new Account { Code = "12", Name = "TICARI ALACAKLAR", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "120", Name = "ALICILAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "121", Name = "ALACAK SENETLERI", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "126", Name = "VERILEN DEPOZITO VE TEMINATLAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "127", Name = "DIGER TICARI ALACAKLAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            
            // 15 - STOKLAR
            new Account { Code = "15", Name = "STOKLAR", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "150", Name = "ILK MADDE VE MALZEME", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "152", Name = "MAMULLER", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "153", Name = "TICARI MALLAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            
            // 19 - DIGER DONEN VARLIKLAR
            new Account { Code = "19", Name = "DIGER DONEN VARLIKLAR", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "190", Name = "DEVREDEN KDV", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "191", Name = "INDIRILECEK KDV", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            
            // 2 - DURAN VARLIKLAR
            new Account { Code = "2", Name = "DURAN VARLIKLAR", Level = 1, IsHeader = true, AllowPosting = false, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "25", Name = "MADDI DURAN VARLIKLAR", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "252", Name = "BINALAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "253", Name = "TESIS, MAKINE VE CIHAZLAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "254", Name = "TASITLAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "255", Name = "DEMIRBASLAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Debit },
            new Account { Code = "257", Name = "BIRIKMMIS AMORTISMANLAR (-)", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Aktif, Nature = AccountNature.Credit },
            
            // 3 - KISA VADELI YABANCI KAYNAKLAR
            new Account { Code = "3", Name = "KISA VADELI YABANCI KAYNAKLAR", Level = 1, IsHeader = true, AllowPosting = false, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "32", Name = "TICARI BORCLAR", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "320", Name = "SATICILAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "321", Name = "BORC SENETLERI", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            
            new Account { Code = "36", Name = "ODENECEK VERGI VE DIGER YUKUMLULUKLER", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "360", Name = "ODENECEK VERGI VE FONLAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "361", Name = "ODENECEK SOSYAL GUVENLIK KESINTILERI", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            
            new Account { Code = "39", Name = "DIGER KISA VADELI YABANCI KAYNAKLAR", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "391", Name = "HESAPLANAN KDV", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            
            // 5 - OZKAYNAKLAR
            new Account { Code = "5", Name = "OZKAYNAKLAR", Level = 1, IsHeader = true, AllowPosting = false, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "50", Name = "ODENMIS SERMAYE", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "500", Name = "SERMAYE", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "57", Name = "GECMIS YILLAR KARLARI", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "570", Name = "GECMIS YILLAR KARLARI", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "58", Name = "GECMIS YILLAR ZARARLARI", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "580", Name = "GECMIS YILLAR ZARARLARI (-)", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Pasif, Nature = AccountNature.Debit },
            new Account { Code = "59", Name = "DONEM NET KARI (ZARARI)", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "590", Name = "DONEM NET KARI", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Pasif, Nature = AccountNature.Credit },
            new Account { Code = "591", Name = "DONEM NET ZARARI (-)", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Pasif, Nature = AccountNature.Debit },
            
            // 6 - GELIR TABLOSU HESAPLARI
            new Account { Code = "6", Name = "GELIR TABLOSU HESAPLARI", Level = 1, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gelir, Nature = AccountNature.Credit },
            new Account { Code = "60", Name = "BRUT SATISLAR", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gelir, Nature = AccountNature.Credit },
            new Account { Code = "600", Name = "YURTICI SATISLAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Gelir, Nature = AccountNature.Credit },
            new Account { Code = "601", Name = "YURTDISI SATISLAR", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Gelir, Nature = AccountNature.Credit },
            new Account { Code = "602", Name = "DIGER GELIRLER", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Gelir, Nature = AccountNature.Credit },
            
            new Account { Code = "61", Name = "SATIS INDIRIMLERI (-)", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gelir, Nature = AccountNature.Debit },
            new Account { Code = "610", Name = "SATISTAN IADELER (-)", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Gelir, Nature = AccountNature.Debit },
            new Account { Code = "611", Name = "SATIS ISKONTOLARI (-)", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Gelir, Nature = AccountNature.Debit },
            
            // 7 - MALIYET HESAPLARI
            new Account { Code = "7", Name = "MALIYET HESAPLARI", Level = 1, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "70", Name = "MALIYET MUHASEBESI BAGLANTI HESAPLARI", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "71", Name = "DIREKT ILK MADDE VE MALZEME GIDERLERI", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "72", Name = "DIREKT ISCILIK GIDERLERI", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "73", Name = "GENEL URETIM GIDERLERI", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "74", Name = "HIZMET URETIM MALIYETI", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "75", Name = "ARASTIRMA VE GELISTIRME GIDERLERI", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "76", Name = "PAZARLAMA SATIS VE DAGITIM GIDERLERI", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "77", Name = "GENEL YONETIM GIDERLERI", Level = 2, IsHeader = true, AllowPosting = false, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "770", Name = "GENEL YONETIM GIDERLERI", Level = 3, IsHeader = false, AllowPosting = true, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "770.01", Name = "PERSONEL GIDERLERI", Level = 4, IsHeader = false, AllowPosting = true, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "770.02", Name = "KIRA GIDERLERI", Level = 4, IsHeader = false, AllowPosting = true, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
            new Account { Code = "770.03", Name = "ELEKTRIK-SU-GAZ GIDERLERI", Level = 4, IsHeader = false, AllowPosting = true, AccountType = AccountType.Gider, Nature = AccountNature.Debit },
        };

        await context.Accounts.AddRangeAsync(accounts);
    }

    private static async Task SeedPayrollParametersAsync(AppDbContext context)
    {
        if (await context.PayrollParameters.AnyAsync()) return;

        var parameters = new List<PayrollParameter>
        {
            new PayrollParameter
            {
                Year = 2024,
                MinimumWage = 17002m,
                SgkEmployeeRate = 14m,
                SgkEmployerRate = 20.5m,
                UnemploymentEmployeeRate = 1m,
                UnemploymentEmployerRate = 2m,
                StampTaxRate = 0.759m,
                SgkCeiling = 127575m,
                IncomeTaxBrackets = "[{\"Limit\":110000,\"Rate\":15},{\"Limit\":230000,\"Rate\":20},{\"Limit\":580000,\"Rate\":27},{\"Limit\":3000000,\"Rate\":35},{\"Limit\":null,\"Rate\":40}]",
                IsActive = true
            },
            new PayrollParameter
            {
                Year = 2025,
                MinimumWage = 22104m,
                SgkEmployeeRate = 14m,
                SgkEmployerRate = 20.5m,
                UnemploymentEmployeeRate = 1m,
                UnemploymentEmployerRate = 2m,
                StampTaxRate = 0.759m,
                SgkCeiling = 165780m,
                IncomeTaxBrackets = "[{\"Limit\":158000,\"Rate\":15},{\"Limit\":330000,\"Rate\":20},{\"Limit\":800000,\"Rate\":27},{\"Limit\":4300000,\"Rate\":35},{\"Limit\":null,\"Rate\":40}]",
                IsActive = true
            }
        };

        await context.PayrollParameters.AddRangeAsync(parameters);
    }
}
