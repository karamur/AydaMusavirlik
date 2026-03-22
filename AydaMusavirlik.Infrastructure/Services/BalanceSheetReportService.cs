using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AydaMusavirlik.Infrastructure.Services;

/// <summary>
/// Bilanco raporu - Formul ve aciklamali
/// </summary>
public interface IBalanceSheetReportService
{
    byte[] GenerateBalanceSheetWithFormulasExcel(BalanceSheetData data);
    byte[] GenerateBalanceSheetWithFormulasPdf(BalanceSheetData data);
}

public class BalanceSheetReportService : IBalanceSheetReportService
{
    static BalanceSheetReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateBalanceSheetWithFormulasExcel(BalanceSheetData data)
    {
        using var workbook = new XLWorkbook();
        
        // Sayfa 1: Bilanco
        CreateBilancoSheet(workbook, data);
        
        // Sayfa 2: Formul Aciklamalari
        CreateFormulAciklamalariSheet(workbook, data);
        
        // Sayfa 3: Kaynak Detaylari
        CreateKaynakDetaylariSheet(workbook, data);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void CreateBilancoSheet(XLWorkbook workbook, BalanceSheetData data)
    {
        var ws = workbook.Worksheets.Add("Bilanco");
        int row = 1;

        // Baslik
        ws.Cell(row, 1).Value = $"BILANCO - {data.FirmaAdi}";
        ws.Range(row, 1, row, 6).Merge();
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        ws.Cell(row, 1).Value = $"Tarih: {data.BilancoTarihi:dd.MM.yyyy}";
        row += 2;

        // AKTIF (VARLIKLAR)
        ws.Cell(row, 1).Value = "AKTIF (VARLIKLAR)";
        ws.Cell(row, 4).Value = "PASIF (KAYNAKLAR)";
        StyleBilancoHeader(ws.Range(row, 1, row, 3));
        StyleBilancoHeader(ws.Range(row, 4, row, 6));
        row++;

        // Baslýklar
        ws.Cell(row, 1).Value = "Hesap Kodu";
        ws.Cell(row, 2).Value = "Hesap Adi";
        ws.Cell(row, 3).Value = "Tutar (TL)";
        ws.Cell(row, 4).Value = "Hesap Kodu";
        ws.Cell(row, 5).Value = "Hesap Adi";
        ws.Cell(row, 6).Value = "Tutar (TL)";
        StyleTableHeader(ws.Range(row, 1, row, 6));
        row++;

        int aktifStartRow = row;
        int pasifStartRow = row;

        // I. DONEN VARLIKLAR
        ws.Cell(row, 1).Value = "I. DONEN VARLIKLAR";
        ws.Range(row, 1, row, 2).Merge();
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E3F2FD");
        
        // I. KISA VADELI YABANCI KAYNAKLAR
        ws.Cell(row, 4).Value = "I. KISA VADELI YABANCI KAYNAKLAR";
        ws.Range(row, 4, row, 5).Merge();
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFEBEE");
        row++;

        // Donen Varliklar detay
        foreach (var item in data.DonenVarliklar)
        {
            ws.Cell(row, 1).Value = item.HesapKodu;
            ws.Cell(row, 2).Value = item.HesapAdi;
            ws.Cell(row, 3).Value = item.Tutar;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        // Donen Varliklar Toplami
        ws.Cell(row, 2).Value = "DONEN VARLIKLAR TOPLAMI";
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 3).FormulaA1 = $"=SUM(C{aktifStartRow + 1}:C{row - 1})";
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        int donenVarliklarToplamRow = row;
        row++;

        // Kisa Vadeli Borclar - paralel olarak
        int kvbRow = aktifStartRow + 1;
        foreach (var item in data.KisaVadeliBorclar)
        {
            ws.Cell(kvbRow, 4).Value = item.HesapKodu;
            ws.Cell(kvbRow, 5).Value = item.HesapAdi;
            ws.Cell(kvbRow, 6).Value = item.Tutar;
            ws.Cell(kvbRow, 6).Style.NumberFormat.Format = "#,##0.00";
            kvbRow++;
        }
        ws.Cell(kvbRow, 5).Value = "KISA VADELI BORCLAR TOPLAMI";
        ws.Cell(kvbRow, 5).Style.Font.Bold = true;
        ws.Cell(kvbRow, 6).FormulaA1 = $"=SUM(F{aktifStartRow + 1}:F{kvbRow - 1})";
        ws.Cell(kvbRow, 6).Style.Font.Bold = true;
        ws.Cell(kvbRow, 6).Style.NumberFormat.Format = "#,##0.00";
        int kvbToplamRow = kvbRow;

        row = Math.Max(row, kvbRow) + 2;

        // II. DURAN VARLIKLAR
        ws.Cell(row, 1).Value = "II. DURAN VARLIKLAR";
        ws.Range(row, 1, row, 2).Merge();
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5E9");

        // II. UZUN VADELI YABANCI KAYNAKLAR
        ws.Cell(row, 4).Value = "II. UZUN VADELI YABANCI KAYNAKLAR";
        ws.Range(row, 4, row, 5).Merge();
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF3E0");
        row++;

        int duranStartRow = row;
        foreach (var item in data.DuranVarliklar)
        {
            ws.Cell(row, 1).Value = item.HesapKodu;
            ws.Cell(row, 2).Value = item.HesapAdi;
            ws.Cell(row, 3).Value = item.Tutar;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        ws.Cell(row, 2).Value = "DURAN VARLIKLAR TOPLAMI";
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 3).FormulaA1 = $"=SUM(C{duranStartRow}:C{row - 1})";
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        int duranVarliklarToplamRow = row;

        // Uzun Vadeli Borclar
        int uvbRow = duranStartRow;
        foreach (var item in data.UzunVadeliBorclar)
        {
            ws.Cell(uvbRow, 4).Value = item.HesapKodu;
            ws.Cell(uvbRow, 5).Value = item.HesapAdi;
            ws.Cell(uvbRow, 6).Value = item.Tutar;
            ws.Cell(uvbRow, 6).Style.NumberFormat.Format = "#,##0.00";
            uvbRow++;
        }
        ws.Cell(uvbRow, 5).Value = "UZUN VADELI BORCLAR TOPLAMI";
        ws.Cell(uvbRow, 5).Style.Font.Bold = true;
        ws.Cell(uvbRow, 6).FormulaA1 = $"=SUM(F{duranStartRow}:F{uvbRow - 1})";
        ws.Cell(uvbRow, 6).Style.Font.Bold = true;
        ws.Cell(uvbRow, 6).Style.NumberFormat.Format = "#,##0.00";
        int uvbToplamRow = uvbRow;

        row = Math.Max(row, uvbRow) + 2;

        // III. OZ KAYNAKLAR
        ws.Cell(row, 4).Value = "III. OZ KAYNAKLAR";
        ws.Range(row, 4, row, 5).Merge();
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8EAF6");
        row++;

        int ozkaynakStartRow = row;
        foreach (var item in data.OzKaynaklar)
        {
            ws.Cell(row, 4).Value = item.HesapKodu;
            ws.Cell(row, 5).Value = item.HesapAdi;
            ws.Cell(row, 6).Value = item.Tutar;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }
        ws.Cell(row, 5).Value = "OZ KAYNAKLAR TOPLAMI";
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 6).FormulaA1 = $"=SUM(F{ozkaynakStartRow}:F{row - 1})";
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        int ozkaynakToplamRow = row;

        row += 2;

        // GENEL TOPLAMLAR
        ws.Cell(row, 1).Value = "AKTIF TOPLAMI";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 12;
        ws.Cell(row, 3).FormulaA1 = $"=C{donenVarliklarToplamRow}+C{duranVarliklarToplamRow}";
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.Font.FontSize = 12;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#1976D2");
        ws.Cell(row, 3).Style.Font.FontColor = XLColor.White;

        ws.Cell(row, 4).Value = "PASIF TOPLAMI";
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.Font.FontSize = 12;
        ws.Cell(row, 6).FormulaA1 = $"=F{kvbToplamRow}+F{uvbToplamRow}+F{ozkaynakToplamRow}";
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.Font.FontSize = 12;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#1976D2");
        ws.Cell(row, 6).Style.Font.FontColor = XLColor.White;

        row += 2;

        // Formul Aciklamasi
        ws.Cell(row, 1).Value = "BILANCO DENKLIGI:";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;
        ws.Cell(row, 1).Value = "AKTIF TOPLAMI = PASIF TOPLAMI";
        ws.Cell(row, 1).Style.Font.Italic = true;
        row++;
        ws.Cell(row, 1).Value = "Donen Varliklar + Duran Varliklar = Kisa Vadeli Borclar + Uzun Vadeli Borclar + Oz Kaynaklar";
        ws.Range(row, 1, row, 6).Merge();

        ws.Columns().AdjustToContents();
        ws.Column(2).Width = 35;
        ws.Column(5).Width = 35;
    }

    private void CreateFormulAciklamalariSheet(XLWorkbook workbook, BalanceSheetData data)
    {
        var ws = workbook.Worksheets.Add("Formul Aciklamalari");
        int row = 1;

        ws.Cell(row, 1).Value = "BILANCO FORMUL VE ACIKLAMALARI";
        ws.Range(row, 1, row, 5).Merge();
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Cell(row, 1).Style.Font.Bold = true;
        row += 2;

        // AKTIF HESAPLAR
        ws.Cell(row, 1).Value = "AKTIF HESAPLAR (VARLIKLAR)";
        StyleSectionHeader(ws.Range(row, 1, row, 5));
        row++;

        ws.Cell(row, 1).Value = "Hesap Grubu";
        ws.Cell(row, 2).Value = "Hesap Kodu Araligi";
        ws.Cell(row, 3).Value = "Aciklama";
        ws.Cell(row, 4).Value = "Kaynak";
        ws.Cell(row, 5).Value = "Formul";
        StyleTableHeader(ws.Range(row, 1, row, 5));
        row++;

        // Donen Varliklar Aciklamalari
        AddFormulRow(ws, ref row, "Hazir Degerler", "10X", 
            "Kasa, Banka, Alinan Cekler gibi aninda nakde cevrilebilir varliklar",
            "Muhasebe fisleri (Tahsilat, Odeme)",
            "Borc - Alacak bakiyesi");

        AddFormulRow(ws, ref row, "Menkul Kiymetler", "11X",
            "Hisse senetleri, tahviller gibi kisa vadeli yatirimlar",
            "Yatirim islemleri",
            "Alis maliyeti + Deger artisi");

        AddFormulRow(ws, ref row, "Ticari Alacaklar", "12X",
            "Musterilerden olan alacaklar (Alicilar hesabi)",
            "Satis faturalari - Tahsilatlar",
            "Toplam Satis - Toplam Tahsilat");

        AddFormulRow(ws, ref row, "Diger Alacaklar", "13X",
            "Ortaklardan, personelden, vs. alacaklar",
            "Borc verme islemleri",
            "Verilen Borc - Geri Alinan");

        AddFormulRow(ws, ref row, "Stoklar", "15X",
            "Ticari mallar, hammadde, mamul stoklari",
            "Satin Alma - Satis",
            "Giris Miktari x Birim Fiyat - Cikis");

        AddFormulRow(ws, ref row, "Gelecek Aylara Ait Giderler", "18X",
            "Pesin odenmis giderler (kira, sigorta vs.)",
            "Pesin odeme faturalari",
            "Odenen - Donemsellestirilen");

        row++;

        // Duran Varliklar
        ws.Cell(row, 1).Value = "DURAN VARLIKLAR";
        StyleSectionHeader(ws.Range(row, 1, row, 5));
        row++;

        ws.Cell(row, 1).Value = "Hesap Grubu";
        ws.Cell(row, 2).Value = "Hesap Kodu Araligi";
        ws.Cell(row, 3).Value = "Aciklama";
        ws.Cell(row, 4).Value = "Kaynak";
        ws.Cell(row, 5).Value = "Formul";
        StyleTableHeader(ws.Range(row, 1, row, 5));
        row++;

        AddFormulRow(ws, ref row, "Mali Duran Varliklar", "24X",
            "Uzun vadeli yatirimlar, istirakler",
            "Yatirim kararlari",
            "Alis Maliyeti");

        AddFormulRow(ws, ref row, "Maddi Duran Varliklar", "25X",
            "Arazi, bina, makine, tasit, demirbaslar",
            "Satin alma / Amortisman",
            "Alis Maliyeti - Birikm. Amortisman");

        AddFormulRow(ws, ref row, "Maddi Olmayan Duran Varliklar", "26X",
            "Haklar, serefiye, arastirma gelistirme",
            "Satin alma / Itfa",
            "Maliyet - Birikm. Itfa Payi");

        row += 2;

        // PASIF HESAPLAR
        ws.Cell(row, 1).Value = "PASIF HESAPLAR (KAYNAKLAR)";
        StyleSectionHeader(ws.Range(row, 1, row, 5));
        row++;

        ws.Cell(row, 1).Value = "Hesap Grubu";
        ws.Cell(row, 2).Value = "Hesap Kodu Araligi";
        ws.Cell(row, 3).Value = "Aciklama";
        ws.Cell(row, 4).Value = "Kaynak";
        ws.Cell(row, 5).Value = "Formul";
        StyleTableHeader(ws.Range(row, 1, row, 5));
        row++;

        // Kisa Vadeli Borclar
        AddFormulRow(ws, ref row, "Mali Borclar", "30X",
            "Banka kredileri (1 yildan kisa vadeli)",
            "Kredi sozlesmeleri",
            "Kullanilan Kredi - Odenen");

        AddFormulRow(ws, ref row, "Ticari Borclar", "32X",
            "Saticilara olan borclar",
            "Alis faturalari - Odemeler",
            "Toplam Alis - Toplam Odeme");

        AddFormulRow(ws, ref row, "Diger Borclar", "33X",
            "Ortaklara, personele vs. borclar",
            "Borc alma islemleri",
            "Alinan Borc - Odenen");

        AddFormulRow(ws, ref row, "Odenecek Vergi ve Fonlar", "36X",
            "KDV, Stopaj, SGK primleri vs.",
            "Beyannameler",
            "Tahakkuk Eden - Odenen");

        AddFormulRow(ws, ref row, "Borc ve Gider Karsiliklari", "37X",
            "Kidem tazminati, garanti vs. karsiliklar",
            "Hesaplama / Karsilik ayirma",
            "Tahmin edilen yukumluluk");

        row++;

        // Uzun Vadeli Borclar
        AddFormulRow(ws, ref row, "Mali Borclar (UV)", "40X",
            "Banka kredileri (1 yildan uzun vadeli)",
            "Kredi sozlesmeleri",
            "Kullanilan - Odenen - KV'ye aktarilan");

        row++;

        // Oz Kaynaklar
        ws.Cell(row, 1).Value = "OZ KAYNAKLAR";
        StyleSectionHeader(ws.Range(row, 1, row, 5));
        row++;

        ws.Cell(row, 1).Value = "Hesap Grubu";
        ws.Cell(row, 2).Value = "Hesap Kodu Araligi";
        ws.Cell(row, 3).Value = "Aciklama";
        ws.Cell(row, 4).Value = "Kaynak";
        ws.Cell(row, 5).Value = "Formul";
        StyleTableHeader(ws.Range(row, 1, row, 5));
        row++;

        AddFormulRow(ws, ref row, "Odenmis Sermaye", "50X",
            "Ortaklar tarafindan konulan sermaye",
            "Sirket ana sozlesmesi",
            "Taahhut Edilen Sermaye");

        AddFormulRow(ws, ref row, "Sermaye Yedekleri", "52X",
            "Hisse senedi ihrac primleri, deger artis fonlari",
            "Sermaye islemleri",
            "Nominal Deger Ustu Tutarlar");

        AddFormulRow(ws, ref row, "Kar Yedekleri", "54X",
            "Yasal yedekler, olaganustu yedekler",
            "Kar dagitim kararlari",
            "Gecmis Yil Karlari x Yedek Orani");

        AddFormulRow(ws, ref row, "Gecmis Yillar Karlari", "57X",
            "Dagitilmayan karlar",
            "Onceki donem bilancolar",
            "Onceki Donem Net Kari");

        AddFormulRow(ws, ref row, "Donem Net Kari", "59X",
            "Cari donem kar/zarari",
            "Gelir tablosu",
            "Toplam Gelirler - Toplam Giderler");

        ws.Columns().AdjustToContents();
        ws.Column(3).Width = 45;
        ws.Column(4).Width = 30;
        ws.Column(5).Width = 35;
    }

    private void CreateKaynakDetaylariSheet(XLWorkbook workbook, BalanceSheetData data)
    {
        var ws = workbook.Worksheets.Add("Veri Kaynaklari");
        int row = 1;

        ws.Cell(row, 1).Value = "BILANCO KALEMLERININ VERI KAYNAKLARI";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Cell(row, 1).Style.Font.Bold = true;
        row += 2;

        ws.Cell(row, 1).Value = "Hesap";
        ws.Cell(row, 2).Value = "Veri Kaynagi";
        ws.Cell(row, 3).Value = "Guncelleme Sikligi";
        ws.Cell(row, 4).Value = "Notlar";
        StyleTableHeader(ws.Range(row, 1, row, 4));
        row++;

        // Kaynak detaylari
        var kaynaklar = new List<(string Hesap, string Kaynak, string Siklik, string Not)>
        {
            ("100 - Kasa", "Kasa sayimi + Muhasebe fisleri", "Gunluk", "Fiili sayim ile mutabik olmali"),
            ("102 - Bankalar", "Banka ekstreleri", "Gunluk", "Banka mutabakati yapilmali"),
            ("120 - Alicilar", "Satis faturalari - Tahsilatlar", "Anlik", "Musteriden teyit alinabilir"),
            ("150 - Stoklar", "Stok sayimi + Giris/Cikis fisleri", "Aylik/Yillik", "Fiili envanter ile karsilastirilmali"),
            ("253 - Makine", "Satin alma faturasi - Amortisman", "Aylik", "Amortisman otomatik hesaplanir"),
            ("320 - Saticilar", "Alis faturalari - Odemeler", "Anlik", "Saticidan mutabakat alinabilir"),
            ("360 - Odenecek Vergiler", "Vergi beyannameleri", "Aylik", "Beyanname tutarlari ile esit olmali"),
            ("500 - Sermaye", "Sirket ana sozlesmesi", "Degistiginde", "Ticaret sicil ile uyumlu olmali"),
            ("590 - Donem Kari", "Gelir Tablosu hesaplamasi", "Donem sonu", "Gelir - Gider = Kar/Zarar")
        };

        foreach (var kaynak in kaynaklar)
        {
            ws.Cell(row, 1).Value = kaynak.Hesap;
            ws.Cell(row, 2).Value = kaynak.Kaynak;
            ws.Cell(row, 3).Value = kaynak.Siklik;
            ws.Cell(row, 4).Value = kaynak.Not;
            row++;
        }

        row += 2;

        // Onemli Notlar
        ws.Cell(row, 1).Value = "ONEMLI BILGILER";
        StyleSectionHeader(ws.Range(row, 1, row, 4));
        row++;

        var notlar = new[]
        {
            "1. Bilanco denkligi: AKTIF TOPLAMI = PASIF TOPLAMI her zaman saglanmalidir.",
            "2. Hesap bakiyeleri muhasebe fislerinden otomatik hesaplanir (Borc - Alacak).",
            "3. Aktif hesaplarda Borc bakiyesi, Pasif hesaplarda Alacak bakiyesi olmasi normaldir.",
            "4. Amortisman giderleri 257 hesaptan (Birikm. Amortisman) gelir ve Duran Varliktan dusulur.",
            "5. Donem Net Kari, Gelir Tablosu sonucu olup bilancoya otomatik yansir.",
            "6. Envanter (sayim) sonuclari ile muhasebe kayitlari arasinda fark olmamalidir."
        };

        foreach (var not in notlar)
        {
            ws.Cell(row, 1).Value = not;
            ws.Range(row, 1, row, 4).Merge();
            row++;
        }

        ws.Columns().AdjustToContents();
        ws.Column(1).Width = 25;
        ws.Column(2).Width = 40;
        ws.Column(4).Width = 40;
    }

    private void AddFormulRow(IXLWorksheet ws, ref int row, string grup, string kod, string aciklama, string kaynak, string formul)
    {
        ws.Cell(row, 1).Value = grup;
        ws.Cell(row, 2).Value = kod;
        ws.Cell(row, 3).Value = aciklama;
        ws.Cell(row, 4).Value = kaynak;
        ws.Cell(row, 5).Value = formul;
        ws.Cell(row, 3).Style.Alignment.WrapText = true;
        ws.Row(row).Height = 30;
        row++;
    }

    private void StyleBilancoHeader(IXLRange range)
    {
        range.Merge();
        range.Style.Font.Bold = true;
        range.Style.Font.FontSize = 14;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#1976D2");
        range.Style.Font.FontColor = XLColor.White;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private void StyleSectionHeader(IXLRange range)
    {
        range.Merge();
        range.Style.Font.Bold = true;
        range.Style.Font.FontSize = 12;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#E3F2FD");
    }

    private void StyleTableHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#455A64");
        range.Style.Font.FontColor = XLColor.White;
    }

    public byte[] GenerateBalanceSheetWithFormulasPdf(BalanceSheetData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text($"BILANCO - {data.FirmaAdi}").Bold().FontSize(16);
                    col.Item().Text($"Tarih: {data.BilancoTarihi:dd.MM.yyyy}");
                    col.Item().PaddingVertical(5).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                });

                page.Content().Column(col =>
                {
                    // Bilanco tablosu
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Aktif
                            columns.RelativeColumn(1); // Tutar
                            columns.ConstantColumn(20); // Bosluk
                            columns.RelativeColumn(3); // Pasif
                            columns.RelativeColumn(1); // Tutar
                        });

                        // Basliklar
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("AKTIF (VARLIKLAR)").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("TUTAR").FontColor(Colors.White).Bold();
                            header.Cell();
                            header.Cell().Background(Colors.Red.Darken2).Padding(5).Text("PASIF (KAYNAKLAR)").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Red.Darken2).Padding(5).Text("TUTAR").FontColor(Colors.White).Bold();
                        });

                        // Donen Varliklar / Kisa Vadeli Borclar
                        table.Cell().Background(Colors.Blue.Lighten4).Padding(4).Text("I. DONEN VARLIKLAR").Bold();
                        table.Cell().Background(Colors.Blue.Lighten4);
                        table.Cell();
                        table.Cell().Background(Colors.Red.Lighten4).Padding(4).Text("I. KISA VADELI BORCLAR").Bold();
                        table.Cell().Background(Colors.Red.Lighten4);

                        decimal donenToplam = 0;
                        foreach (var item in data.DonenVarliklar)
                        {
                            table.Cell().Padding(3).Text($"{item.HesapKodu} - {item.HesapAdi}");
                            table.Cell().Padding(3).AlignRight().Text($"{item.Tutar:N2}");
                            table.Cell();
                            table.Cell();
                            table.Cell();
                            donenToplam += item.Tutar;
                        }

                        decimal kvbToplam = 0;
                        foreach (var item in data.KisaVadeliBorclar)
                        {
                            table.Cell();
                            table.Cell();
                            table.Cell();
                            table.Cell().Padding(3).Text($"{item.HesapKodu} - {item.HesapAdi}");
                            table.Cell().Padding(3).AlignRight().Text($"{item.Tutar:N2}");
                            kvbToplam += item.Tutar;
                        }

                        // Toplamlar
                        table.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("DONEN VARLIK TOPLAMI").Bold();
                        table.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text($"{donenToplam:N2}").Bold();
                        table.Cell();
                        table.Cell().Background(Colors.Red.Lighten3).Padding(4).Text("KV BORC TOPLAMI").Bold();
                        table.Cell().Background(Colors.Red.Lighten3).Padding(4).AlignRight().Text($"{kvbToplam:N2}").Bold();
                    });

                    col.Item().PaddingVertical(10);

                    // Formul Aciklamasi
                    col.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(c =>
                    {
                        c.Item().Text("FORMUL ACIKLAMASI").Bold();
                        c.Item().Text("Bilanco Denkligi: AKTIF = PASIF").Italic();
                        c.Item().Text("Donen Varliklar + Duran Varliklar = Kisa Vadeli Borclar + Uzun Vadeli Borclar + Oz Kaynaklar");
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span($"Olusturma: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }
}

#region Data Models

public class BalanceSheetData
{
    public string FirmaAdi { get; set; } = "";
    public DateTime BilancoTarihi { get; set; } = DateTime.Now;
    
    public List<BilancoKalemi> DonenVarliklar { get; set; } = new();
    public List<BilancoKalemi> DuranVarliklar { get; set; } = new();
    public List<BilancoKalemi> KisaVadeliBorclar { get; set; } = new();
    public List<BilancoKalemi> UzunVadeliBorclar { get; set; } = new();
    public List<BilancoKalemi> OzKaynaklar { get; set; } = new();
    
    public decimal AktifToplam => DonenVarliklar.Sum(x => x.Tutar) + DuranVarliklar.Sum(x => x.Tutar);
    public decimal PasifToplam => KisaVadeliBorclar.Sum(x => x.Tutar) + UzunVadeliBorclar.Sum(x => x.Tutar) + OzKaynaklar.Sum(x => x.Tutar);
}

public class BilancoKalemi
{
    public string HesapKodu { get; set; } = "";
    public string HesapAdi { get; set; } = "";
    public decimal Tutar { get; set; }
    public string? Aciklama { get; set; }
    public string? Formul { get; set; }
    public string? VeriKaynagi { get; set; }
}

#endregion