using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AydaMusavirlik.VeriAnaliz.Models;

namespace AydaMusavirlik.VeriAnaliz.Reports;

/// <summary>
/// PDF rapor olusturma servisi
/// </summary>
public interface IPdfReportService
{
    Task GenerateGelirGiderRaporAsync(string filePath, List<GelirGiderKayit> kayitlar, 
        MuhasebeOzet ozet, string firmaAdi);
    Task GenerateAnalizRaporAsync(string filePath, MuhasebeOzet ozet, 
        List<KategoriOzet> kategoriler, List<AylikOzet> ayliklar, string firmaAdi);
}

public class PdfReportService : IPdfReportService
{
    static PdfReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task GenerateGelirGiderRaporAsync(string filePath, List<GelirGiderKayit> kayitlar, 
        MuhasebeOzet ozet, string firmaAdi)
    {
        return Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeHeader(c, firmaAdi, "Gelir Gider Raporu"));

                    page.Content().Element(c =>
                    {
                        c.PaddingVertical(10).Column(column =>
                        {
                            // Ozet Kutusu
                            column.Item().Element(e => ComposeOzetBox(e, ozet));

                            column.Item().PaddingTop(15).Text("Islem Detaylari").Bold().FontSize(12);

                            // Tablo
                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(70);  // Tarih
                                    columns.ConstantColumn(50);  // Tur
                                    columns.RelativeColumn(2);   // Kategori
                                    columns.RelativeColumn(3);   // Aciklama
                                    columns.ConstantColumn(80);  // Tutar
                                    columns.ConstantColumn(50);  // Kasa
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Tarih").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Tur").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Kategori").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Aciklama").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Tutar").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Kasa").Bold();
                                });

                                foreach (var kayit in kayitlar.Take(50)) // Max 50 kayit
                                {
                                    var bgColor = kayit.Tur == "Gelir" ? Colors.Green.Lighten5 : Colors.Red.Lighten5;

                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten1)
                                        .Padding(3).Text(kayit.Tarih.ToString("dd.MM.yyyy"));
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten1)
                                        .Padding(3).Text(kayit.Tur);
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten1)
                                        .Padding(3).Text(kayit.Kategori);
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten1)
                                        .Padding(3).Text(kayit.Aciklama);
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten1)
                                        .Padding(3).AlignRight().Text($"{kayit.Tutar:N2} TL");
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten1)
                                        .Padding(3).Text(kayit.Kasa);
                                }
                            });

                            if (kayitlar.Count > 50)
                            {
                                column.Item().PaddingTop(5).Text($"... ve {kayitlar.Count - 50} kayit daha")
                                    .Italic().FontColor(Colors.Grey.Medium);
                            }
                        });
                    });

                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(filePath);
        });
    }

    public Task GenerateAnalizRaporAsync(string filePath, MuhasebeOzet ozet, 
        List<KategoriOzet> kategoriler, List<AylikOzet> ayliklar, string firmaAdi)
    {
        return Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeHeader(c, firmaAdi, "Mali Analiz Raporu"));

                    page.Content().Element(c =>
                    {
                        c.PaddingVertical(10).Column(column =>
                        {
                            // Ozet
                            column.Item().Element(e => ComposeOzetBox(e, ozet));

                            // Kategori Analizi
                            column.Item().PaddingTop(20).Text("Kategori Bazli Analiz").Bold().FontSize(12);
                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Kategori").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Gelir").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Gider").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Net").Bold();
                                });

                                foreach (var kat in kategoriler)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(kat.Kategori);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight()
                                        .Text($"{kat.ToplamGelir:N2}").FontColor(Colors.Green.Medium);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight()
                                        .Text($"{kat.ToplamGider:N2}").FontColor(Colors.Red.Medium);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight()
                                        .Text($"{kat.Net:N2}").Bold();
                                }
                            });

                            // Aylik Analiz
                            column.Item().PaddingTop(20).Text("Aylik Analiz").Bold().FontSize(12);
                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Ay").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Gelir").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Gider").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Net").Bold();
                                });

                                foreach (var ay in ayliklar.Where(a => a.IslemSayisi > 0))
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(ay.AyAdi);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight()
                                        .Text($"{ay.ToplamGelir:N2}").FontColor(Colors.Green.Medium);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight()
                                        .Text($"{ay.ToplamGider:N2}").FontColor(Colors.Red.Medium);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight()
                                        .Text($"{ay.Net:N2}").Bold();
                                }
                            });
                        });
                    });

                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf(filePath);
        });
    }

    private void ComposeHeader(IContainer container, string firmaAdi, string raporAdi)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(firmaAdi).Bold().FontSize(16);
                column.Item().Text(raporAdi).FontSize(12).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(100).AlignRight().Column(column =>
            {
                column.Item().Text(DateTime.Now.ToString("dd.MM.yyyy")).AlignRight();
                column.Item().Text("AYDA Musavirlik").AlignRight().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private void ComposeOzetBox(IContainer container, MuhasebeOzet ozet)
    {
        container.Background(Colors.Grey.Lighten4).Padding(15).Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("TOPLAM GELIR").FontSize(9).FontColor(Colors.Grey.Medium);
                column.Item().Text($"{ozet.ToplamGelir:N2} TL").FontSize(14).Bold().FontColor(Colors.Green.Medium);
            });

            row.RelativeItem().Column(column =>
            {
                column.Item().Text("TOPLAM GIDER").FontSize(9).FontColor(Colors.Grey.Medium);
                column.Item().Text($"{ozet.ToplamGider:N2} TL").FontSize(14).Bold().FontColor(Colors.Red.Medium);
            });

            row.RelativeItem().Column(column =>
            {
                column.Item().Text("NET KAR/ZARAR").FontSize(9).FontColor(Colors.Grey.Medium);
                column.Item().Text($"{ozet.Guncel:N2} TL").FontSize(14).Bold()
                    .FontColor(ozet.Guncel >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
            });

            row.RelativeItem().Column(column =>
            {
                column.Item().Text("KAYIT SAYISI").FontSize(9).FontColor(Colors.Grey.Medium);
                column.Item().Text(ozet.KayitSayisi.ToString()).FontSize(14).Bold();
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Sayfa ");
            text.CurrentPageNumber();
            text.Span(" / ");
            text.TotalPages();
        });
    }
}
