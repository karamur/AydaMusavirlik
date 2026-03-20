namespace AydaMusavirlik.Desktop.Services.Reports;

/// <summary>
/// Rapor Üretim Servisi - Bilanço, Gelir Tablosu, Nakit Akýþ
/// </summary>
public interface IReportGeneratorService
{
    Task<BalanceSheetReport> GenerateBalanceSheetAsync(int companyId, DateTime reportDate);
    Task<IncomeStatementReport> GenerateIncomeStatementAsync(int companyId, DateTime startDate, DateTime endDate);
    Task<CashFlowReport> GenerateCashFlowAsync(int companyId, DateTime startDate, DateTime endDate);
}

public class ReportGeneratorService : IReportGeneratorService
{
    private readonly IAccountService _accountService;

    public ReportGeneratorService(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<BalanceSheetReport> GenerateBalanceSheetAsync(int companyId, DateTime reportDate)
    {
        var accounts = await _accountService.GetByCompanyAsync(companyId);
        var trialBalance = await _accountService.GetTrialBalanceAsync(companyId, 
            new DateTime(reportDate.Year, 1, 1), reportDate);

        var report = new BalanceSheetReport
        {
            CompanyId = companyId,
            ReportDate = reportDate,
            Period = reportDate.ToString("yyyy")
        };

        if (trialBalance?.Items == null) return report;

        // DÖNEN VARLIKLAR (1xx hesaplar)
        report.DonenVarliklar.Items = trialBalance.Items
            .Where(i => i.AccountCode.StartsWith("1"))
            .Select(i => new BalanceSheetItem
            {
                Code = i.AccountCode,
                Name = i.AccountName,
                Amount = i.DebitBalance - i.CreditBalance,
                Level = i.AccountCode.Length
            }).ToList();

        // DURAN VARLIKLAR (2xx hesaplar)
        report.DuranVarliklar.Items = trialBalance.Items
            .Where(i => i.AccountCode.StartsWith("2"))
            .Select(i => new BalanceSheetItem
            {
                Code = i.AccountCode,
                Name = i.AccountName,
                Amount = i.DebitBalance - i.CreditBalance,
                Level = i.AccountCode.Length
            }).ToList();

        // KISA VADELÝ YABANCI KAYNAKLAR (3xx hesaplar)
        report.KisaVadeliYabanciKaynaklar.Items = trialBalance.Items
            .Where(i => i.AccountCode.StartsWith("3"))
            .Select(i => new BalanceSheetItem
            {
                Code = i.AccountCode,
                Name = i.AccountName,
                Amount = i.CreditBalance - i.DebitBalance,
                Level = i.AccountCode.Length
            }).ToList();

        // UZUN VADELÝ YABANCI KAYNAKLAR (4xx hesaplar)
        report.UzunVadeliYabanciKaynaklar.Items = trialBalance.Items
            .Where(i => i.AccountCode.StartsWith("4"))
            .Select(i => new BalanceSheetItem
            {
                Code = i.AccountCode,
                Name = i.AccountName,
                Amount = i.CreditBalance - i.DebitBalance,
                Level = i.AccountCode.Length
            }).ToList();

        // ÖZ KAYNAKLAR (5xx hesaplar)
        report.OzKaynaklar.Items = trialBalance.Items
            .Where(i => i.AccountCode.StartsWith("5"))
            .Select(i => new BalanceSheetItem
            {
                Code = i.AccountCode,
                Name = i.AccountName,
                Amount = i.CreditBalance - i.DebitBalance,
                Level = i.AccountCode.Length
            }).ToList();

        return report;
    }

    public async Task<IncomeStatementReport> GenerateIncomeStatementAsync(int companyId, DateTime startDate, DateTime endDate)
    {
        var trialBalance = await _accountService.GetTrialBalanceAsync(companyId, startDate, endDate);

        var report = new IncomeStatementReport
        {
            CompanyId = companyId,
            StartDate = startDate,
            EndDate = endDate,
            Period = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}"
        };

        if (trialBalance?.Items == null) return report;

        // GELÝR TABLOSU HESAPLARI (6xx hesaplar)
        report.Items = trialBalance.Items
            .Where(i => i.AccountCode.StartsWith("6"))
            .Select(i => new IncomeStatementItem
            {
                Code = i.AccountCode,
                Name = i.AccountName,
                Amount = i.AccountCode.StartsWith("60") || i.AccountCode.StartsWith("64") 
                    ? i.CreditBalance - i.DebitBalance  // Gelir hesaplarý
                    : i.DebitBalance - i.CreditBalance,  // Gider hesaplarý
                Level = i.AccountCode.Length
            }).ToList();

        return report;
    }

    public async Task<CashFlowReport> GenerateCashFlowAsync(int companyId, DateTime startDate, DateTime endDate)
    {
        // Basitleþtirilmiþ nakit akýþ hesaplama
        var trialBalance = await _accountService.GetTrialBalanceAsync(companyId, startDate, endDate);

        var report = new CashFlowReport
        {
            CompanyId = companyId,
            StartDate = startDate,
            EndDate = endDate
        };

        if (trialBalance?.Items == null) return report;

        // Dönem baþý nakit
        var kasaBakiyesi = trialBalance.Items
            .Where(i => i.AccountCode.StartsWith("10"))
            .Sum(i => i.DebitBalance - i.CreditBalance);

        report.DonemBasiNakit = 0; // Önceki dönemden

        // Ýþletme faaliyetleri
        report.IsletmeFaaliyetleri.Items.Add(new CashFlowItem 
        { 
            Name = "Net Kar/Zarar", 
            Amount = trialBalance.Items.Where(i => i.AccountCode.StartsWith("6"))
                .Sum(i => i.CreditBalance - i.DebitBalance)
        });

        report.IsletmeFaaliyetleri.Items.Add(new CashFlowItem 
        { 
            Name = "Amortisman Giderleri (+)", 
            Amount = Math.Abs(trialBalance.Items
                .FirstOrDefault(i => i.AccountCode == "257")?.CreditBalance ?? 0)
        });

        // Yatýrým faaliyetleri
        report.YatirimFaaliyetleri.Items.Add(new CashFlowItem 
        { 
            Name = "Maddi Duran Varlýk Alýmlarý (-)", 
            Amount = -trialBalance.Items
                .Where(i => i.AccountCode.StartsWith("25") && i.AccountCode != "257")
                .Sum(i => i.DebitBalance)
        });

        // Finansman faaliyetleri
        report.FinansmanFaaliyetleri.Items.Add(new CashFlowItem 
        { 
            Name = "Banka Kredileri Deðiþimi", 
            Amount = trialBalance.Items
                .Where(i => i.AccountCode.StartsWith("30"))
                .Sum(i => i.CreditBalance - i.DebitBalance)
        });

        return report;
    }
}