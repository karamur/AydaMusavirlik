using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AydaMusavirlik.Infrastructure.Services;

/// <summary>
/// Ýzin formu PDF oluþturma servisi
/// </summary>
public interface ILeaveFormPdfService
{
    byte[] GenerateLeaveFormPdf(LeaveFormPdfModel model);
    Task<string> SaveLeaveFormPdfAsync(LeaveFormPdfModel model, string outputPath);
}

public class LeaveFormPdfService : ILeaveFormPdfService
{
    static LeaveFormPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateLeaveFormPdf(LeaveFormPdfModel model)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c, model));
                page.Content().Element(c => ComposeContent(c, model));
                page.Footer().Element(c => ComposeFooter(c, model));
            });
        });

        return document.GeneratePdf();
    }

    public async Task<string> SaveLeaveFormPdfAsync(LeaveFormPdfModel model, string outputPath)
    {
        var pdfBytes = GenerateLeaveFormPdf(model);
        var fileName = $"IzinFormu_{model.FormNo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var fullPath = Path.Combine(outputPath, fileName);

        Directory.CreateDirectory(outputPath);
        await File.WriteAllBytesAsync(fullPath, pdfBytes);

        return fullPath;
    }

    private void ComposeHeader(IContainer container, LeaveFormPdfModel model)
    {
        container.Column(column =>
        {
            // Firma logosu ve bilgisi
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(model.FirmaAdi).Bold().FontSize(14);
                    col.Item().Text(model.FirmaAdres ?? "").FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(100).AlignRight().Text($"Tarih: {DateTime.Now:dd.MM.yyyy}").FontSize(9);
            });

            column.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);

            // Form baþlýðý
            column.Item().AlignCenter().PaddingVertical(15).Column(col =>
            {
                col.Item().Text("ÝZÝN TALEBÝ FORMU").Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                col.Item().Text($"Form No: {model.FormNo}").FontSize(10).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeContent(IContainer container, LeaveFormPdfModel model)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // Personel Bilgileri
            column.Item().Element(c => ComposeSectionTitle(c, "PERSONEL BÝLGÝLERÝ"));
            column.Item().Element(c => ComposePersonelInfo(c, model));

            column.Item().PaddingVertical(15);

            // Ýzin Bilgileri
            column.Item().Element(c => ComposeSectionTitle(c, "ÝZÝN BÝLGÝLERÝ"));
            column.Item().Element(c => ComposeIzinInfo(c, model));

            column.Item().PaddingVertical(15);

            // Ýzin Bakiyesi
            column.Item().Element(c => ComposeSectionTitle(c, "ÝZÝN BAKÝYESÝ"));
            column.Item().Element(c => ComposeIzinBakiye(c, model));

            column.Item().PaddingVertical(15);

            // Onay Durumu
            column.Item().Element(c => ComposeOnayDurumu(c, model));

            column.Item().PaddingVertical(30);

            // Ýmza Alanlarý
            column.Item().Element(c => ComposeImzaAlanlari(c, model));
        });
    }

    private void ComposeSectionTitle(IContainer container, string title)
    {
        container.Background(Colors.Blue.Lighten5).Padding(8).Text(title).Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
    }

    private void ComposePersonelInfo(IContainer container, LeaveFormPdfModel model)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten1).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(150);
                columns.RelativeColumn();
            });

            AddTableRow(table, "Adý Soyadý", model.PersonelAdi);
            AddTableRow(table, "TC Kimlik No", model.TcKimlikNo ?? "-", true);
            AddTableRow(table, "Sicil No", model.SicilNo ?? "-");
            AddTableRow(table, "Departman", model.Departman ?? "-", true);
            AddTableRow(table, "Pozisyon", model.Pozisyon ?? "-");
            AddTableRow(table, "Ýþe Giriþ Tarihi", model.IseGirisTarihi.ToString("dd.MM.yyyy"), true);
        });
    }

    private void ComposeIzinInfo(IContainer container, LeaveFormPdfModel model)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten1).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(150);
                columns.RelativeColumn();
            });

            AddTableRow(table, "Ýzin Türü", model.IzinTuru, false, true);
            AddTableRow(table, "Baþlangýç Tarihi", model.BaslangicTarihi.ToString("dd MMMM yyyy, dddd"), true);
            AddTableRow(table, "Bitiþ Tarihi", model.BitisTarihi.ToString("dd MMMM yyyy, dddd"));
            AddTableRow(table, "Toplam Gün", $"{model.GunSayisi} gün", true, true);

            if (!string.IsNullOrEmpty(model.Aciklama))
                AddTableRow(table, "Ýzin Nedeni", model.Aciklama);

            if (!string.IsNullOrEmpty(model.VekilAdi))
                AddTableRow(table, "Yerine Bakacak", model.VekilAdi, true);

            if (!string.IsNullOrEmpty(model.IletisimTelefon))
                AddTableRow(table, "Ýletiþim Tel", model.IletisimTelefon);
        });
    }

    private void ComposeIzinBakiye(IContainer container, LeaveFormPdfModel model)
    {
        container.Row(row =>
        {
            row.RelativeItem().Border(1).BorderColor(Colors.Blue.Lighten2).Background(Colors.Blue.Lighten5).Padding(15).Column(col =>
            {
                col.Item().Text("Hak Edilen").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text($"{model.ToplamHakedilen} gün").Bold().FontSize(14);
            });

            row.ConstantItem(10);

            row.RelativeItem().Border(1).BorderColor(Colors.Red.Lighten2).Background(Colors.Red.Lighten5).Padding(15).Column(col =>
            {
                col.Item().Text("Kullanýlan").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text($"{model.Kullanilan} gün").Bold().FontSize(14);
            });

            row.ConstantItem(10);

            row.RelativeItem().Border(1).BorderColor(Colors.Green.Lighten2).Background(Colors.Green.Lighten5).Padding(15).Column(col =>
            {
                col.Item().Text("Kalan").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text($"{model.Kalan} gün").Bold().FontSize(14);
            });
        });
    }

    private void ComposeOnayDurumu(IContainer container, LeaveFormPdfModel model)
    {
        var bgColor = model.OnayDurumu switch
        {
            "Onaylandý" => Colors.Green.Lighten4,
            "Reddedildi" => Colors.Red.Lighten4,
            _ => Colors.Orange.Lighten4
        };

        var borderColor = model.OnayDurumu switch
        {
            "Onaylandý" => Colors.Green.Darken1,
            "Reddedildi" => Colors.Red.Darken1,
            _ => Colors.Orange.Darken1
        };

        var icon = model.OnayDurumu switch
        {
            "Onaylandý" => "?",
            "Reddedildi" => "?",
            _ => "?"
        };

        container.Border(2).BorderColor(borderColor).Background(bgColor).Padding(15).Row(row =>
        {
            row.ConstantItem(30).AlignCenter().Text(icon).FontSize(20);
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(model.OnayDurumu.ToUpper()).Bold().FontSize(12);
                if (!string.IsNullOrEmpty(model.OnaylayanAdi))
                    col.Item().Text($"Onaylayan: {model.OnaylayanAdi} | Tarih: {model.OnayTarihi:dd.MM.yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeImzaAlanlari(IContainer container, LeaveFormPdfModel model)
    {
        container.Row(row =>
        {
            row.RelativeItem().AlignCenter().Column(col =>
            {
                col.Item().Height(50).Border(0).BorderBottom(1).BorderColor(Colors.Black);
                col.Item().PaddingTop(5).Text("Personel Ýmza").FontSize(9);
                col.Item().Text(model.PersonelAdi).FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            row.ConstantItem(80);

            row.RelativeItem().AlignCenter().Column(col =>
            {
                col.Item().Height(50).Border(0).BorderBottom(1).BorderColor(Colors.Black);
                col.Item().PaddingTop(5).Text("Yönetici Onay").FontSize(9);
                col.Item().Text(model.OnaylayanAdi ?? "").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeFooter(IContainer container, LeaveFormPdfModel model)
    {
        container.AlignCenter().Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            col.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text($"© {DateTime.Now.Year} {model.FirmaAdi}").FontSize(8).FontColor(Colors.Grey.Darken1);
                row.ConstantItem(150).AlignRight().Text($"Oluþturulma: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void AddTableRow(TableDescriptor table, string label, string value, bool alternate = false, bool bold = false)
    {
        var bgColor = alternate ? Colors.Grey.Lighten4 : Colors.White;

        table.Cell().Background(bgColor).Padding(8).Text(label).SemiBold();
        table.Cell().Background(bgColor).Padding(8).Text(text =>
        {
            if (bold)
                text.Span(value).Bold();
            else
                text.Span(value);
        });
    }
}

/// <summary>
/// Ýzin formu PDF modeli
/// </summary>
public class LeaveFormPdfModel
{
    public string FormNo { get; set; } = "";
    public string FirmaAdi { get; set; } = "Ayda Müþavirlik";
    public string? FirmaAdres { get; set; }

    // Personel bilgileri
    public string PersonelAdi { get; set; } = "";
    public string? TcKimlikNo { get; set; }
    public string? SicilNo { get; set; }
    public string? Departman { get; set; }
    public string? Pozisyon { get; set; }
    public DateTime IseGirisTarihi { get; set; }

    // Ýzin bilgileri
    public string IzinTuru { get; set; } = "";
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int GunSayisi { get; set; }
    public string? Aciklama { get; set; }
    public string? VekilAdi { get; set; }
    public string? IletisimTelefon { get; set; }
    public string? IletisimAdres { get; set; }

    // Bakiye
    public int ToplamHakedilen { get; set; }
    public int Kullanilan { get; set; }
    public int Kalan { get; set; }

    // Onay
    public string OnayDurumu { get; set; } = "Bekliyor";
    public string? OnaylayanAdi { get; set; }
    public DateTime? OnayTarihi { get; set; }
    public string? OnayNotu { get; set; }
}