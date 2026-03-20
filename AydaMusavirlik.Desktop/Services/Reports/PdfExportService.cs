using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AydaMusavirlik.Desktop.Services.Reports;

/// <summary>
/// PDF Export Servisi - QuestPDF ile profesyonel raporlar
/// </summary>
public interface IPdfExportService
{
    Task<string> ExportBalanceSheetAsync(BalanceSheetReport report, string filePath);
    Task<string> ExportIncomeStatementAsync(IncomeStatementReport report, string filePath);
    Task<string> ExportFinancialAnalysisAsync(FinancialHealthScore score, LiquidityRatios liquidity, 
        ProfitabilityRatios profitability, LeverageRatios leverage, string filePath);
}

public class PdfExportService : IPdfExportService
{
    static PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> ExportBalanceSheetAsync(BalanceSheetReport report, string filePath)
    {
        return await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeHeader(c, "BÝLANÇO", report.ReportDate));

                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        // AKTÝF
                        column.Item().Text("AKTÝF (VARLIKLAR)").Bold().FontSize(12);
                        column.Item().Element(c => ComposeBalanceSection(c, report.DonenVarliklar));
                        column.Item().Element(c => ComposeBalanceSection(c, report.DuranVarliklar));

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("TOPLAM AKTÝF").Bold();
                            row.ConstantItem(100).AlignRight().Text($"{report.ToplamAktif:N2} TL").Bold();
                        });

                        column.Item().LineHorizontal(1);

                        // PASÝF
                        column.Item().Text("PASÝF (KAYNAKLAR)").Bold().FontSize(12);
                        column.Item().Element(c => ComposeBalanceSection(c, report.KisaVadeliYabanciKaynaklar));
                        column.Item().Element(c => ComposeBalanceSection(c, report.UzunVadeliYabanciKaynaklar));
                        column.Item().Element(c => ComposeBalanceSection(c, report.OzKaynaklar));

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("TOPLAM PASÝF").Bold();
                            row.ConstantItem(100).AlignRight().Text($"{report.ToplamPasif:N2} TL").Bold();
                        });

                        // Denge kontrolü
                        if (!report.IsBalanced)
                        {
                            column.Item().Background(Colors.Red.Lighten4).Padding(5)
                                .Text($"?? Aktif-Pasif farký: {report.ToplamAktif - report.ToplamPasif:N2} TL")
                                .FontColor(Colors.Red.Darken2);
                        }
                    });

                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(filePath);

            return filePath;
        });
    }

    public async Task<string> ExportIncomeStatementAsync(IncomeStatementReport report, string filePath)
    {
        return await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeHeader(c, "GELÝR TABLOSU", report.EndDate));

                    page.Content().Column(column =>
                    {
                        column.Spacing(5);

                        column.Item().Text($"Dönem: {report.Period}").FontSize(9);
                        column.Item().LineHorizontal(1);

                        // Tablo baþlýklarý
                        column.Item().Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
                        {
                            row.ConstantItem(60).Text("KOD").Bold();
                            row.RelativeItem().Text("HESAP ADI").Bold();
                            row.ConstantItem(100).AlignRight().Text("TUTAR (TL)").Bold();
                        });

                        foreach (var item in report.Items)
                        {
                            column.Item().Padding(3).Row(row =>
                            {
                                row.ConstantItem(60).Text(item.Code);
                                row.RelativeItem().Text(item.Name);
                                row.ConstantItem(100).AlignRight().Text($"{item.Amount:N2}");
                            });
                        }

                        column.Item().LineHorizontal(2);

                        // Özet Bilgiler
                        column.Item().PaddingTop(10).Text("ÖZET").Bold().FontSize(12);

                        AddSummaryRow(column, "Brüt Satýþlar", report.BrutSatislar);
                        AddSummaryRow(column, "Net Satýþlar", report.NetSatislar);
                        AddSummaryRow(column, "Brüt Satýþ Karý", report.BrutSatisKari);
                        AddSummaryRow(column, "Faaliyet Karý", report.FaaliyetKari);

                        column.Item().Background(report.DonemKariZarari >= 0 ? Colors.Green.Lighten4 : Colors.Red.Lighten4)
                            .Padding(5).Row(row =>
                            {
                                row.RelativeItem().Text("DÖNEM KARI/ZARARI").Bold();
                                row.ConstantItem(100).AlignRight().Text($"{report.DonemKariZarari:N2} TL").Bold();
                            });
                    });

                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(filePath);

            return filePath;
        });
    }

    public async Task<string> ExportFinancialAnalysisAsync(FinancialHealthScore score, 
        LiquidityRatios liquidity, ProfitabilityRatios profitability, LeverageRatios leverage, string filePath)
    {
        return await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeHeader(c, "MALÝ ANALÝZ RAPORU", DateTime.Now));

                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        // Genel Skor
                        column.Item().Background(GetScoreColor(score.TotalScore)).Padding(15).Column(inner =>
                        {
                            inner.Item().AlignCenter().Text("MALÝ SAÐLIK PUANI").Bold().FontSize(14).FontColor(Colors.White);
                            inner.Item().AlignCenter().Text($"{score.TotalScore:N0} / 100").Bold().FontSize(32).FontColor(Colors.White);
                            inner.Item().AlignCenter().Text($"Not: {score.Grade}").FontSize(16).FontColor(Colors.White);
                            inner.Item().AlignCenter().PaddingTop(5).Text(score.Interpretation).FontSize(10).FontColor(Colors.White);
                        });

                        // Likidite Oranlarý
                        column.Item().Text("1. LÝKÝDÝTE ORANLARI").Bold().FontSize(12);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Oran").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Deðer").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Durum").Bold();
                            });

                            AddRatioRow(table, "Cari Oran", $"{liquidity.CariOran:N2}", liquidity.CariOranYorum);
                            AddRatioRow(table, "Asit Test Oraný", $"{liquidity.AsitTestOrani:N2}", liquidity.AsitTestOrani >= 1 ? "Ýyi" : "Dikkat");
                            AddRatioRow(table, "Nakit Oraný", $"{liquidity.NakitOrani:N2}", liquidity.NakitOrani >= 0.2m ? "Ýyi" : "Dikkat");
                            AddRatioRow(table, "Net Ýþletme Sermayesi", $"{liquidity.NetIsletmeSermayesi:N2} TL", liquidity.NetIsletmeSermayesi > 0 ? "Pozitif" : "Negatif");
                        });

                        // Karlýlýk Oranlarý
                        column.Item().Text("2. KARLILIK ORANLARI").Bold().FontSize(12);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Oran").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Deðer").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Durum").Bold();
                            });

                            AddRatioRow(table, "Brüt Kar Marjý", $"%{profitability.BrutKarMarji:N2}", profitability.BrutKarMarji > 20 ? "Ýyi" : "Orta");
                            AddRatioRow(table, "Net Kar Marjý", $"%{profitability.NetKarMarji:N2}", profitability.NetKarMarji > 10 ? "Ýyi" : "Orta");
                            AddRatioRow(table, "Özkaynak Karlýlýðý (ROE)", $"%{profitability.OzkaynakKarliligi:N2}", profitability.OzkaynakKarliligi > 15 ? "Ýyi" : "Orta");
                            AddRatioRow(table, "Aktif Karlýlýðý (ROA)", $"%{profitability.AktifKarliligi:N2}", profitability.AktifKarliligi > 5 ? "Ýyi" : "Orta");
                        });

                        // Kaldýraç Oranlarý
                        column.Item().Text("3. BORÇLULUK ORANLARI").Bold().FontSize(12);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Oran").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Deðer").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Durum").Bold();
                            });

                            AddRatioRow(table, "Borç/Özkaynak Oraný", $"{leverage.BorcOzkaynakOrani:N2}", leverage.BorcOzkaynakOrani < 1 ? "Ýyi" : "Dikkat");
                            AddRatioRow(table, "Toplam Borç Oraný", $"%{leverage.ToplamBorcOrani:N2}", leverage.ToplamBorcOrani < 50 ? "Ýyi" : "Dikkat");
                            AddRatioRow(table, "Özkaynak Oraný", $"%{leverage.OzkaynakOrani:N2}", leverage.OzkaynakOrani > 50 ? "Ýyi" : "Orta");
                        });

                        // Puan Daðýlýmý
                        column.Item().Text("PUAN DAÐILIMI").Bold().FontSize(12);
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Likidite: {score.LiquidityScore:N0}/25");
                                c.Item().Text($"Karlýlýk: {score.ProfitabilityScore:N0}/25");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Borçluluk: {score.LeverageScore:N0}/25");
                                c.Item().Text($"Verimlilik: {score.ActivityScore:N0}/25");
                            });
                        });
                    });

                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(filePath);

            return filePath;
        });
    }

    private void ComposeHeader(IContainer container, string title, DateTime date)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(title).Bold().FontSize(18);
                    c.Item().Text($"Rapor Tarihi: {date:dd.MM.yyyy}").FontSize(9);
                });
                row.ConstantItem(100).AlignRight().Text("AYDA MÜÞAVÝRLÝK").Bold();
            });
            column.Item().PaddingTop(5).LineHorizontal(2);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1);
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Oluþturma: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8);
                row.RelativeItem().AlignRight().Text(x =>
                {
                    x.Span("Sayfa ").FontSize(8);
                    x.CurrentPageNumber().FontSize(8);
                    x.Span(" / ").FontSize(8);
                    x.TotalPages().FontSize(8);
                });
            });
        });
    }

    private void ComposeBalanceSection(IContainer container, BalanceSheetSection section)
    {
        container.Column(column =>
        {
            column.Item().Background(Colors.Grey.Lighten4).Padding(5).Text(section.Title).Bold();

            foreach (var item in section.Items)
            {
                column.Item().Padding(3).Row(row =>
                {
                    row.ConstantItem(50).Text(item.Code);
                    row.RelativeItem().Text(item.Name);
                    row.ConstantItem(100).AlignRight().Text($"{item.Amount:N2}");
                });
            }

            column.Item().Background(Colors.Grey.Lighten3).Padding(3).Row(row =>
            {
                row.RelativeItem().Text($"{section.Title} TOPLAMI").Bold();
                row.ConstantItem(100).AlignRight().Text($"{section.Total:N2} TL").Bold();
            });
        });
    }

    private void AddSummaryRow(ColumnDescriptor column, string label, decimal value)
    {
        column.Item().Padding(3).Row(row =>
        {
            row.RelativeItem().Text(label);
            row.ConstantItem(100).AlignRight().Text($"{value:N2} TL");
        });
    }

    private void AddRatioRow(TableDescriptor table, string name, string value, string status)
    {
        table.Cell().Padding(5).Text(name);
        table.Cell().Padding(5).Text(value);
        table.Cell().Padding(5).Text(status);
    }

    private Color GetScoreColor(decimal score)
    {
        return score switch
        {
            >= 80 => Colors.Green.Darken1,
            >= 60 => Colors.Blue.Darken1,
            >= 40 => Colors.Orange.Darken1,
            _ => Colors.Red.Darken1
        };
    }
}