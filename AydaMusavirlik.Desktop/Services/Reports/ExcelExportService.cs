using ClosedXML.Excel;

namespace AydaMusavirlik.Desktop.Services.Reports;

/// <summary>
/// Excel Export Servisi
/// </summary>
public interface IExcelExportService
{
    Task<string> ExportBalanceSheetAsync(BalanceSheetReport report, string filePath);
    Task<string> ExportIncomeStatementAsync(IncomeStatementReport report, string filePath);
    Task<string> ExportTrialBalanceAsync(TrialBalanceDto trialBalance, string filePath);
    Task<string> ExportPayrollAsync(IEnumerable<PayrollRecordDto> payrolls, string filePath);
}

public class ExcelExportService : IExcelExportService
{
    public async Task<string> ExportBalanceSheetAsync(BalanceSheetReport report, string filePath)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Bilanço");

            // Baþlýk
            ws.Cell("A1").Value = "BÝLANÇO";
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 16;
            ws.Range("A1:D1").Merge();

            ws.Cell("A2").Value = $"Tarih: {report.ReportDate:dd.MM.yyyy}";
            ws.Range("A2:D2").Merge();

            int row = 4;

            // AKTÝF
            ws.Cell(row, 1).Value = "AKTÝF (VARLIKLAR)";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 4).Value = "TUTAR";
            ws.Cell(row, 4).Style.Font.Bold = true;
            row++;

            // Dönen Varlýklar
            row = AddSection(ws, row, report.DonenVarliklar);

            // Duran Varlýklar
            row = AddSection(ws, row, report.DuranVarliklar);

            // Toplam Aktif
            ws.Cell(row, 1).Value = "TOPLAM AKTÝF";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 4).Value = report.ToplamAktif;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Style.Font.Bold = true;
            row += 2;

            // PASÝF
            ws.Cell(row, 1).Value = "PASÝF (KAYNAKLAR)";
            ws.Cell(row, 1).Style.Font.Bold = true;
            row++;

            // KVYK
            row = AddSection(ws, row, report.KisaVadeliYabanciKaynaklar);

            // UVYK
            row = AddSection(ws, row, report.UzunVadeliYabanciKaynaklar);

            // Öz Kaynaklar
            row = AddSection(ws, row, report.OzKaynaklar);

            // Toplam Pasif
            ws.Cell(row, 1).Value = "TOPLAM PASÝF";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 4).Value = report.ToplamPasif;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Style.Font.Bold = true;

            // Sütun geniþlikleri
            ws.Column(1).Width = 15;
            ws.Column(2).Width = 40;
            ws.Column(3).Width = 20;
            ws.Column(4).Width = 20;

            workbook.SaveAs(filePath);
            return filePath;
        });
    }

    private int AddSection(IXLWorksheet ws, int startRow, BalanceSheetSection section)
    {
        int row = startRow;

        ws.Cell(row, 1).Value = section.Title;
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;

        foreach (var item in section.Items)
        {
            ws.Cell(row, 1).Value = item.Code;
            ws.Cell(row, 2).Value = item.Name;
            ws.Cell(row, 4).Value = item.Amount;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        // Alt Toplam
        ws.Cell(row, 2).Value = $"{section.Title} TOPLAMI";
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = section.Total;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 4).Style.Font.Bold = true;
        row++;

        return row;
    }

    public async Task<string> ExportIncomeStatementAsync(IncomeStatementReport report, string filePath)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Gelir Tablosu");

            ws.Cell("A1").Value = "GELÝR TABLOSU";
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 16;
            ws.Range("A1:C1").Merge();

            ws.Cell("A2").Value = $"Dönem: {report.Period}";
            ws.Range("A2:C2").Merge();

            int row = 4;

            // Baþlýklar
            ws.Cell(row, 1).Value = "HESAP KODU";
            ws.Cell(row, 2).Value = "HESAP ADI";
            ws.Cell(row, 3).Value = "TUTAR";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            row++;

            foreach (var item in report.Items)
            {
                ws.Cell(row, 1).Value = item.Code;
                ws.Cell(row, 2).Value = item.Name;
                ws.Cell(row, 3).Value = item.Amount;
                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";

                if (item.IsHeader || item.IsTotal)
                {
                    ws.Range(row, 1, row, 3).Style.Font.Bold = true;
                }
                row++;
            }

            row += 2;

            // Özet
            ws.Cell(row, 2).Value = "BRÜT SATIÞLAR";
            ws.Cell(row, 3).Value = report.BrutSatislar;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            row++;

            ws.Cell(row, 2).Value = "NET SATIÞLAR";
            ws.Cell(row, 3).Value = report.NetSatislar;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            row++;

            ws.Cell(row, 2).Value = "BRÜT SATIÞ KARI";
            ws.Cell(row, 3).Value = report.BrutSatisKari;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            row++;

            ws.Cell(row, 2).Value = "FAALÝYET KARI";
            ws.Cell(row, 3).Value = report.FaaliyetKari;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            row++;

            ws.Cell(row, 2).Value = "DÖNEM KARI/ZARARI";
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = report.DonemKariZarari;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 3).Style.Font.Bold = true;

            if (report.DonemKariZarari >= 0)
                ws.Cell(row, 3).Style.Font.FontColor = XLColor.Green;
            else
                ws.Cell(row, 3).Style.Font.FontColor = XLColor.Red;

            ws.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
            return filePath;
        });
    }

    public async Task<string> ExportTrialBalanceAsync(TrialBalanceDto trialBalance, string filePath)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Mizan");

            ws.Cell("A1").Value = "MÝZAN";
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 16;
            ws.Range("A1:F1").Merge();

            ws.Cell("A2").Value = $"Dönem: {trialBalance.StartDate:dd.MM.yyyy} - {trialBalance.EndDate:dd.MM.yyyy}";
            ws.Range("A2:F2").Merge();

            int row = 4;

            // Baþlýklar
            ws.Cell(row, 1).Value = "HESAP KODU";
            ws.Cell(row, 2).Value = "HESAP ADI";
            ws.Cell(row, 3).Value = "BORÇ";
            ws.Cell(row, 4).Value = "ALACAK";
            ws.Cell(row, 5).Value = "BORÇ BAKÝYE";
            ws.Cell(row, 6).Value = "ALACAK BAKÝYE";
            ws.Range(row, 1, row, 6).Style.Font.Bold = true;
            ws.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.LightGray;
            row++;

            foreach (var item in trialBalance.Items)
            {
                ws.Cell(row, 1).Value = item.AccountCode;
                ws.Cell(row, 2).Value = item.AccountName;
                ws.Cell(row, 3).Value = item.TotalDebit;
                ws.Cell(row, 4).Value = item.TotalCredit;
                ws.Cell(row, 5).Value = item.DebitBalance;
                ws.Cell(row, 6).Value = item.CreditBalance;

                for (int col = 3; col <= 6; col++)
                {
                    ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
                }
                row++;
            }

            // Toplamlar
            ws.Cell(row, 2).Value = "TOPLAM";
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = trialBalance.TotalDebit;
            ws.Cell(row, 4).Value = trialBalance.TotalCredit;
            ws.Cell(row, 5).Value = trialBalance.Items.Sum(i => i.DebitBalance);
            ws.Cell(row, 6).Value = trialBalance.Items.Sum(i => i.CreditBalance);

            for (int col = 3; col <= 6; col++)
            {
                ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Style.Font.Bold = true;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
            return filePath;
        });
    }

    public async Task<string> ExportPayrollAsync(IEnumerable<PayrollRecordDto> payrolls, string filePath)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Bordro");

            int row = 1;
            ws.Cell(row, 1).Value = "PERSONEL";
            ws.Cell(row, 2).Value = "BRÜT MAAÞ";
            ws.Cell(row, 3).Value = "SGK ÝÞÇÝ";
            ws.Cell(row, 4).Value = "GELÝR VERGÝSÝ";
            ws.Cell(row, 5).Value = "DAMGA VERGÝSÝ";
            ws.Cell(row, 6).Value = "KESÝNTÝ TOPLAM";
            ws.Cell(row, 7).Value = "NET MAAÞ";
            ws.Cell(row, 8).Value = "SGK ÝÞVEREN";
            ws.Cell(row, 9).Value = "TOPLAM MALÝYET";

            ws.Range(row, 1, row, 9).Style.Font.Bold = true;
            ws.Range(row, 1, row, 9).Style.Fill.BackgroundColor = XLColor.LightBlue;
            row++;

            foreach (var p in payrolls)
            {
                ws.Cell(row, 1).Value = p.EmployeeName;
                ws.Cell(row, 2).Value = p.GrossSalary;
                ws.Cell(row, 3).Value = p.SgkWorkerDeduction;
                ws.Cell(row, 4).Value = p.IncomeTax;
                ws.Cell(row, 5).Value = p.StampTax;
                ws.Cell(row, 6).Value = p.TotalDeductions;
                ws.Cell(row, 7).Value = p.NetSalary;
                ws.Cell(row, 8).Value = p.SgkEmployerCost;
                ws.Cell(row, 9).Value = p.TotalEmployerCost;

                for (int col = 2; col <= 9; col++)
                {
                    ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
                }
                row++;
            }

            // Toplamlar
            var list = payrolls.ToList();
            ws.Cell(row, 1).Value = "TOPLAM";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = list.Sum(p => p.GrossSalary);
            ws.Cell(row, 3).Value = list.Sum(p => p.SgkWorkerDeduction);
            ws.Cell(row, 4).Value = list.Sum(p => p.IncomeTax);
            ws.Cell(row, 5).Value = list.Sum(p => p.StampTax);
            ws.Cell(row, 6).Value = list.Sum(p => p.TotalDeductions);
            ws.Cell(row, 7).Value = list.Sum(p => p.NetSalary);
            ws.Cell(row, 8).Value = list.Sum(p => p.SgkEmployerCost);
            ws.Cell(row, 9).Value = list.Sum(p => p.TotalEmployerCost);

            for (int col = 2; col <= 9; col++)
            {
                ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Style.Font.Bold = true;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
            return filePath;
        });
    }
}