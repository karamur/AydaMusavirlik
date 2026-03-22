using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AydaMusavirlik.Infrastructure.Services;

/// <summary>
/// Raporlama servisi - Excel ve PDF rapor oluþturma
/// </summary>
public interface IReportService
{
    // Excel Raporlarý
    byte[] GenerateExcelReport<T>(IEnumerable<T> data, string sheetName, Dictionary<string, string>? columnMappings = null);
    byte[] GeneratePayrollExcelReport(IEnumerable<PayrollReportItem> items, string period);
    byte[] GenerateEmployeeListExcel(IEnumerable<EmployeeReportItem> employees);
    byte[] GenerateAccountStatementExcel(IEnumerable<AccountStatementItem> items, string accountName, DateTime startDate, DateTime endDate);

    // PDF Raporlarý
    byte[] GeneratePayrollPdfReport(IEnumerable<PayrollReportItem> items, string period, string companyName);
    byte[] GenerateEmployeeListPdf(IEnumerable<EmployeeReportItem> employees, string companyName);
    byte[] GenerateFinancialSummaryPdf(FinancialSummaryReport report);
    byte[] GenerateMonthlyReportPdf(MonthlyReport report);
}

public class ReportService : IReportService
{
    static ReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    #region Excel Reports

    public byte[] GenerateExcelReport<T>(IEnumerable<T> data, string sheetName, Dictionary<string, string>? columnMappings = null)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        var properties = typeof(T).GetProperties();
        var dataList = data.ToList();

        // Header
        for (int i = 0; i < properties.Length; i++)
        {
            var propName = properties[i].Name;
            var displayName = columnMappings?.GetValueOrDefault(propName) ?? propName;
            worksheet.Cell(1, i + 1).Value = displayName;
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1976D2");
            worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        // Data
        for (int row = 0; row < dataList.Count; row++)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(dataList[row]);
                worksheet.Cell(row + 2, col + 1).Value = value?.ToString() ?? "";
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GeneratePayrollExcelReport(IEnumerable<PayrollReportItem> items, string period)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Bordro");

        // Baþlýk
        ws.Cell("A1").Value = $"BORDRO RAPORU - {period}";
        ws.Range("A1:L1").Merge();
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Header satýrý
        var headers = new[] { "Sicil No", "Ad Soyad", "Departman", "Brüt Maaþ", "SGK Ýþçi", "Ýþsizlik", "Gelir V.", "Damga V.", "Kesintiler", "Net Maaþ", "SGK Ýþveren", "Toplam Maliyet" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(3, i + 1).Value = headers[i];
            ws.Cell(3, i + 1).Style.Font.Bold = true;
            ws.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1976D2");
            ws.Cell(3, i + 1).Style.Font.FontColor = XLColor.White;
            ws.Cell(3, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Veri satýrlarý
        int row = 4;
        var itemList = items.ToList();
        foreach (var item in itemList)
        {
            ws.Cell(row, 1).Value = item.SicilNo;
            ws.Cell(row, 2).Value = item.AdSoyad;
            ws.Cell(row, 3).Value = item.Departman;
            ws.Cell(row, 4).Value = item.BrutMaas;
            ws.Cell(row, 5).Value = item.SgkIsci;
            ws.Cell(row, 6).Value = item.Issizlik;
            ws.Cell(row, 7).Value = item.GelirVergisi;
            ws.Cell(row, 8).Value = item.DamgaVergisi;
            ws.Cell(row, 9).Value = item.ToplamKesinti;
            ws.Cell(row, 10).Value = item.NetMaas;
            ws.Cell(row, 11).Value = item.SgkIsveren;
            ws.Cell(row, 12).Value = item.ToplamMaliyet;

            // Para formatý
            for (int c = 4; c <= 12; c++)
            {
                ws.Cell(row, c).Style.NumberFormat.Format = "#,##0.00 ?";
            }

            row++;
        }

        // Toplam satýrý
        ws.Cell(row, 3).Value = "TOPLAM";
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 4).FormulaA1 = $"=SUM(D4:D{row - 1})";
        ws.Cell(row, 9).FormulaA1 = $"=SUM(I4:I{row - 1})";
        ws.Cell(row, 10).FormulaA1 = $"=SUM(J4:J{row - 1})";
        ws.Cell(row, 11).FormulaA1 = $"=SUM(K4:K{row - 1})";
        ws.Cell(row, 12).FormulaA1 = $"=SUM(L4:L{row - 1})";

        for (int c = 4; c <= 12; c++)
        {
            ws.Cell(row, c).Style.NumberFormat.Format = "#,##0.00 ?";
            ws.Cell(row, c).Style.Font.Bold = true;
            ws.Cell(row, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#E3F2FD");
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateEmployeeListExcel(IEnumerable<EmployeeReportItem> employees)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Personel Listesi");

        var headers = new[] { "Sicil No", "TC Kimlik", "Ad Soyad", "Departman", "Pozisyon", "Ýþe Giriþ", "Brüt Maaþ", "Durum", "Telefon", "E-posta" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4CAF50");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var emp in employees)
        {
            ws.Cell(row, 1).Value = emp.SicilNo;
            ws.Cell(row, 2).Value = emp.TcKimlik;
            ws.Cell(row, 3).Value = emp.AdSoyad;
            ws.Cell(row, 4).Value = emp.Departman;
            ws.Cell(row, 5).Value = emp.Pozisyon;
            ws.Cell(row, 6).Value = emp.IseGiris.ToString("dd.MM.yyyy");
            ws.Cell(row, 7).Value = emp.BrutMaas;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00 ?";
            ws.Cell(row, 8).Value = emp.Aktif ? "Aktif" : "Pasif";
            ws.Cell(row, 9).Value = emp.Telefon;
            ws.Cell(row, 10).Value = emp.Email;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateAccountStatementExcel(IEnumerable<AccountStatementItem> items, string accountName, DateTime startDate, DateTime endDate)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Hesap Ekstresi");

        // Baþlýk
        ws.Cell("A1").Value = $"HESAP EKSTRESÝ - {accountName}";
        ws.Range("A1:G1").Merge();
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;

        ws.Cell("A2").Value = $"Dönem: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
        ws.Range("A2:G2").Merge();

        var headers = new[] { "Tarih", "Fiþ No", "Açýklama", "Borç", "Alacak", "Bakiye", "Belge No" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(4, i + 1).Value = headers[i];
            ws.Cell(4, i + 1).Style.Font.Bold = true;
            ws.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#FF9800");
            ws.Cell(4, i + 1).Style.Font.FontColor = XLColor.White;
        }

        int row = 5;
        decimal runningBalance = 0;
        foreach (var item in items)
        {
            runningBalance += item.Borc - item.Alacak;

            ws.Cell(row, 1).Value = item.Tarih.ToString("dd.MM.yyyy");
            ws.Cell(row, 2).Value = item.FisNo;
            ws.Cell(row, 3).Value = item.Aciklama;
            ws.Cell(row, 4).Value = item.Borc;
            ws.Cell(row, 5).Value = item.Alacak;
            ws.Cell(row, 6).Value = runningBalance;
            ws.Cell(row, 7).Value = item.BelgeNo;

            for (int c = 4; c <= 6; c++)
            {
                ws.Cell(row, c).Style.NumberFormat.Format = "#,##0.00 ?";
            }
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region PDF Reports

    public byte[] GeneratePayrollPdfReport(IEnumerable<PayrollReportItem> items, string period, string companyName)
    {
        var itemList = items.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComposePayrollHeader(c, period, companyName));
                page.Content().Element(c => ComposePayrollContent(c, itemList));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposePayrollHeader(IContainer container, string period, string companyName)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Text(companyName).Bold().FontSize(14);
                row.ConstantItem(150).AlignRight().Text($"Tarih: {DateTime.Now:dd.MM.yyyy}");
            });
            col.Item().PaddingVertical(5).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
            col.Item().AlignCenter().Text($"BORDRO RAPORU - {period}").Bold().FontSize(16);
            col.Item().Height(10);
        });
    }

    private void ComposePayrollContent(IContainer container, List<PayrollReportItem> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(50);  // Sicil
                columns.RelativeColumn(2);   // Ad Soyad
                columns.RelativeColumn();    // Departman
                columns.ConstantColumn(70);  // Brüt
                columns.ConstantColumn(55);  // SGK
                columns.ConstantColumn(55);  // Gelir V
                columns.ConstantColumn(55);  // Kesinti
                columns.ConstantColumn(70);  // Net
                columns.ConstantColumn(70);  // Maliyet
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Sicil").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Ad Soyad").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Departman").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Brüt").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("SGK").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Gelir V.").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Kesinti").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Net").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Maliyet").FontColor(Colors.White).Bold();
            });

            // Data
            bool alternate = false;
            foreach (var item in items)
            {
                var bgColor = alternate ? Colors.Grey.Lighten4 : Colors.White;

                table.Cell().Background(bgColor).Padding(4).Text(item.SicilNo);
                table.Cell().Background(bgColor).Padding(4).Text(item.AdSoyad);
                table.Cell().Background(bgColor).Padding(4).Text(item.Departman);
                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{item.BrutMaas:N2}");
                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{item.SgkIsci:N2}");
                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{item.GelirVergisi:N2}");
                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{item.ToplamKesinti:N2}");
                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{item.NetMaas:N2}").Bold();
                table.Cell().Background(bgColor).Padding(4).AlignRight().Text($"{item.ToplamMaliyet:N2}");

                alternate = !alternate;
            }

            // Toplam
            var totals = new
            {
                Brut = items.Sum(x => x.BrutMaas),
                Kesinti = items.Sum(x => x.ToplamKesinti),
                Net = items.Sum(x => x.NetMaas),
                Maliyet = items.Sum(x => x.ToplamMaliyet)
            };

            table.Cell().ColumnSpan(3).Background(Colors.Blue.Lighten4).Padding(5).Text("TOPLAM").Bold();
            table.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text($"{totals.Brut:N2}").Bold();
            table.Cell().ColumnSpan(2).Background(Colors.Blue.Lighten4);
            table.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text($"{totals.Kesinti:N2}").Bold();
            table.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text($"{totals.Net:N2}").Bold();
            table.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text($"{totals.Maliyet:N2}").Bold();
        });
    }

    public byte[] GenerateEmployeeListPdf(IEnumerable<EmployeeReportItem> employees, string companyName)
    {
        var empList = employees.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(companyName).Bold().FontSize(14);
                    col.Item().PaddingVertical(5).LineHorizontal(2).LineColor(Colors.Green.Darken2);
                    col.Item().AlignCenter().Text("PERSONEL LÝSTESÝ").Bold().FontSize(14);
                    col.Item().AlignCenter().Text($"Toplam: {empList.Count} personel").FontSize(10);
                    col.Item().Height(10);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(50);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Green.Darken2).Padding(5).Text("Sicil").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Green.Darken2).Padding(5).Text("Ad Soyad").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Green.Darken2).Padding(5).Text("Departman").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Green.Darken2).Padding(5).Text("Pozisyon").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Green.Darken2).Padding(5).Text("Ýþe Giriþ").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Green.Darken2).Padding(5).Text("Durum").FontColor(Colors.White).Bold();
                    });

                    bool alt = false;
                    foreach (var emp in empList)
                    {
                        var bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                        table.Cell().Background(bg).Padding(4).Text(emp.SicilNo);
                        table.Cell().Background(bg).Padding(4).Text(emp.AdSoyad);
                        table.Cell().Background(bg).Padding(4).Text(emp.Departman);
                        table.Cell().Background(bg).Padding(4).Text(emp.Pozisyon);
                        table.Cell().Background(bg).Padding(4).Text(emp.IseGiris.ToString("dd.MM.yyyy"));
                        table.Cell().Background(bg).Padding(4).Text(emp.Aktif ? "Aktif" : "Pasif");
                        alt = !alt;
                    }
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateFinancialSummaryPdf(FinancialSummaryReport report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(report.CompanyName).Bold().FontSize(14);
                    col.Item().PaddingVertical(5).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    col.Item().AlignCenter().Text("MALÝ ÖZET RAPORU").Bold().FontSize(16);
                    col.Item().AlignCenter().Text($"Dönem: {report.Period}").FontSize(11);
                    col.Item().Height(15);
                });

                page.Content().Column(col =>
                {
                    // Özet Kartlar
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Green.Darken1).Padding(10).Column(c =>
                        {
                            c.Item().Text("Toplam Gelir").FontSize(9).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{report.ToplamGelir:N2} ?").Bold().FontSize(14).FontColor(Colors.Green.Darken2);
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Red.Darken1).Padding(10).Column(c =>
                        {
                            c.Item().Text("Toplam Gider").FontSize(9).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{report.ToplamGider:N2} ?").Bold().FontSize(14).FontColor(Colors.Red.Darken2);
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Blue.Darken1).Padding(10).Column(c =>
                        {
                            c.Item().Text("Net Kar/Zarar").FontSize(9).FontColor(Colors.Grey.Darken1);
                            var color = report.NetKar >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                            c.Item().Text($"{report.NetKar:N2} ?").Bold().FontSize(14).FontColor(color);
                        });
                    });

                    col.Item().Height(20);

                    // Mali Oranlar
                    col.Item().Text("Mali Oranlar").Bold().FontSize(12);
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Oran").FontColor(Colors.White).Bold();
                            h.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Deðer").FontColor(Colors.White).Bold();
                            h.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Durum").FontColor(Colors.White).Bold();
                        });

                        foreach (var ratio in report.Ratios)
                        {
                            table.Cell().Padding(4).Text(ratio.Name);
                            table.Cell().Padding(4).Text($"{ratio.Value:P2}");
                            table.Cell().Padding(4).Text(ratio.Status);
                        }
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateMonthlyReportPdf(MonthlyReport report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Header().Column(col =>
                {
                    col.Item().Text(report.CompanyName).Bold().FontSize(14);
                    col.Item().LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    col.Item().AlignCenter().PaddingVertical(10).Text($"AYLIK RAPOR - {report.Month}/{report.Year}").Bold().FontSize(16);
                });

                page.Content().Column(col =>
                {
                    // Ýstatistikler
                    col.Item().Text("ÖZET ÝSTATÝSTÝKLER").Bold();
                    col.Item().PaddingVertical(5).Row(row =>
                    {
                        row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                        {
                            c.Item().Text("Fiþ Sayýsý");
                            c.Item().Text($"{report.FisSayisi}").Bold().FontSize(18);
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                        {
                            c.Item().Text("Tahsilat");
                            c.Item().Text($"{report.Tahsilat:N0} ?").Bold().FontSize(14);
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(c =>
                        {
                            c.Item().Text("Ödeme");
                            c.Item().Text($"{report.Odeme:N0} ?").Bold().FontSize(14);
                        });
                    });

                    col.Item().Height(20);

                    // Personel özeti
                    col.Item().Text("PERSONEL ÖZETÝ").Bold();
                    col.Item().PaddingVertical(5).Text($"Toplam Personel: {report.PersonelSayisi}");
                    col.Item().Text($"Bordro Toplam: {report.BordroToplam:N2} ?");

                    col.Item().Height(20);

                    // Notlar
                    if (!string.IsNullOrEmpty(report.Notlar))
                    {
                        col.Item().Text("NOTLAR").Bold();
                        col.Item().PaddingVertical(5).Text(report.Notlar);
                    }
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            col.Item().PaddingTop(5).Text(text =>
            {
                text.Span($"Oluþturulma: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(" | ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span("Ayda Müþavirlik").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    #endregion
}

#region Report Models

public class PayrollReportItem
{
    public string SicilNo { get; set; } = "";
    public string AdSoyad { get; set; } = "";
    public string Departman { get; set; } = "";
    public decimal BrutMaas { get; set; }
    public decimal SgkIsci { get; set; }
    public decimal Issizlik { get; set; }
    public decimal GelirVergisi { get; set; }
    public decimal DamgaVergisi { get; set; }
    public decimal ToplamKesinti { get; set; }
    public decimal NetMaas { get; set; }
    public decimal SgkIsveren { get; set; }
    public decimal ToplamMaliyet { get; set; }
}

public class EmployeeReportItem
{
    public string SicilNo { get; set; } = "";
    public string TcKimlik { get; set; } = "";
    public string AdSoyad { get; set; } = "";
    public string Departman { get; set; } = "";
    public string Pozisyon { get; set; } = "";
    public DateTime IseGiris { get; set; }
    public decimal BrutMaas { get; set; }
    public bool Aktif { get; set; } = true;
    public string? Telefon { get; set; }
    public string? Email { get; set; }
}

public class AccountStatementItem
{
    public DateTime Tarih { get; set; }
    public string FisNo { get; set; } = "";
    public string Aciklama { get; set; } = "";
    public decimal Borc { get; set; }
    public decimal Alacak { get; set; }
    public string? BelgeNo { get; set; }
}

public class FinancialSummaryReport
{
    public string CompanyName { get; set; } = "";
    public string Period { get; set; } = "";
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetKar => ToplamGelir - ToplamGider;
    public List<FinancialRatio> Ratios { get; set; } = new();
}

public class FinancialRatio
{
    public string Name { get; set; } = "";
    public decimal Value { get; set; }
    public string Status { get; set; } = "";
}

public class MonthlyReport
{
    public string CompanyName { get; set; } = "";
    public int Year { get; set; }
    public int Month { get; set; }
    public int FisSayisi { get; set; }
    public decimal Tahsilat { get; set; }
    public decimal Odeme { get; set; }
    public int PersonelSayisi { get; set; }
    public decimal BordroToplam { get; set; }
    public string? Notlar { get; set; }
}

#endregion