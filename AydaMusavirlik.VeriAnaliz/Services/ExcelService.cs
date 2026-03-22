using ClosedXML.Excel;
using AydaMusavirlik.VeriAnaliz.Models;

namespace AydaMusavirlik.VeriAnaliz.Services;

/// <summary>
/// Excel import/export servisi
/// </summary>
public interface IExcelService
{
    Task<List<GelirGiderKayit>> ImportFromExcelAsync(string filePath, int firmaId);
    Task ExportToExcelAsync(string filePath, List<GelirGiderKayit> kayitlar, string firmaAdi);
    Task ExportOzetRaporAsync(string filePath, MuhasebeOzet ozet, List<KategoriOzet> kategoriler, List<AylikOzet> ayliklar, string firmaAdi);
}

public class ExcelService : IExcelService
{
    public async Task<List<GelirGiderKayit>> ImportFromExcelAsync(string filePath, int firmaId)
    {
        var kayitlar = new List<GelirGiderKayit>();

        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.First();

            var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int row = 2; row <= rowCount; row++) // Header atla
            {
                try
                {
                    var tarihStr = worksheet.Cell(row, 1).GetString();
                    var tur = worksheet.Cell(row, 2).GetString();
                    var kategori = worksheet.Cell(row, 3).GetString();
                    var aciklama = worksheet.Cell(row, 4).GetString();
                    var tutarStr = worksheet.Cell(row, 5).GetString();
                    var kasa = worksheet.Cell(row, 6).GetString();
                    var belgeNo = worksheet.Cell(row, 7).GetString();

                    if (DateTime.TryParse(tarihStr, out var tarih) && 
                        decimal.TryParse(tutarStr, out var tutar))
                    {
                        kayitlar.Add(new GelirGiderKayit
                        {
                            FirmaId = firmaId,
                            Tarih = tarih,
                            Tur = string.IsNullOrEmpty(tur) ? "Gelir" : tur,
                            Kategori = kategori ?? "Diger",
                            Aciklama = aciklama ?? "",
                            Tutar = tutar,
                            Kasa = string.IsNullOrEmpty(kasa) ? "Nakit" : kasa,
                            BelgeNo = belgeNo
                        });
                    }
                }
                catch
                {
                    // Hatali satiri atla
                }
            }
        });

        return kayitlar;
    }

    public async Task ExportToExcelAsync(string filePath, List<GelirGiderKayit> kayitlar, string firmaAdi)
    {
        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Gelir Gider");

            // Baslik
            ws.Cell(1, 1).Value = $"{firmaAdi} - Gelir Gider Listesi";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 8).Merge();

            ws.Cell(2, 1).Value = $"Olusturma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}";
            ws.Range(2, 1, 2, 8).Merge();

            // Header
            int row = 4;
            var headers = new[] { "Tarih", "Tur", "Kategori", "Aciklama", "Tutar", "Kasa", "Belge No", "Notlar" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
            }
            ws.Range(row, 1, row, 8).Style.Font.Bold = true;
            ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.LightGray;
            row++;

            // Data
            foreach (var kayit in kayitlar)
            {
                ws.Cell(row, 1).Value = kayit.Tarih.ToString("dd.MM.yyyy");
                ws.Cell(row, 2).Value = kayit.Tur;
                ws.Cell(row, 3).Value = kayit.Kategori;
                ws.Cell(row, 4).Value = kayit.Aciklama;
                ws.Cell(row, 5).Value = kayit.Tutar;
                ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 6).Value = kayit.Kasa;
                ws.Cell(row, 7).Value = kayit.BelgeNo ?? "";
                ws.Cell(row, 8).Value = kayit.Notlar ?? "";

                // Gelir yesil, Gider kirmizi
                if (kayit.Tur == "Gelir")
                    ws.Cell(row, 5).Style.Font.FontColor = XLColor.Green;
                else
                    ws.Cell(row, 5).Style.Font.FontColor = XLColor.Red;

                row++;
            }

            // Ozet
            row++;
            var toplamGelir = kayitlar.Where(k => k.Tur == "Gelir").Sum(k => k.Tutar);
            var toplamGider = kayitlar.Where(k => k.Tur == "Gider").Sum(k => k.Tutar);

            ws.Cell(row, 4).Value = "Toplam Gelir:";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 5).Value = toplamGelir;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Style.Font.FontColor = XLColor.Green;
            row++;

            ws.Cell(row, 4).Value = "Toplam Gider:";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 5).Value = toplamGider;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Style.Font.FontColor = XLColor.Red;
            row++;

            ws.Cell(row, 4).Value = "Net:";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 5).Value = toplamGelir - toplamGider;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        });
    }

    public async Task ExportOzetRaporAsync(string filePath, MuhasebeOzet ozet, 
        List<KategoriOzet> kategoriler, List<AylikOzet> ayliklar, string firmaAdi)
    {
        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();

            // Genel Ozet
            var wsOzet = workbook.Worksheets.Add("Genel Ozet");
            wsOzet.Cell(1, 1).Value = $"{firmaAdi} - Mali Ozet Raporu";
            wsOzet.Cell(1, 1).Style.Font.Bold = true;
            wsOzet.Cell(1, 1).Style.Font.FontSize = 16;
            wsOzet.Range(1, 1, 1, 3).Merge();

            wsOzet.Cell(3, 1).Value = "Toplam Gelir:";
            wsOzet.Cell(3, 2).Value = ozet.ToplamGelir;
            wsOzet.Cell(3, 2).Style.NumberFormat.Format = "#,##0.00 TL";
            wsOzet.Cell(3, 2).Style.Font.FontColor = XLColor.Green;

            wsOzet.Cell(4, 1).Value = "Toplam Gider:";
            wsOzet.Cell(4, 2).Value = ozet.ToplamGider;
            wsOzet.Cell(4, 2).Style.NumberFormat.Format = "#,##0.00 TL";
            wsOzet.Cell(4, 2).Style.Font.FontColor = XLColor.Red;

            wsOzet.Cell(5, 1).Value = "Net Kar/Zarar:";
            wsOzet.Cell(5, 2).Value = ozet.Guncel;
            wsOzet.Cell(5, 2).Style.NumberFormat.Format = "#,##0.00 TL";
            wsOzet.Cell(5, 2).Style.Font.Bold = true;

            wsOzet.Cell(6, 1).Value = "Kayit Sayisi:";
            wsOzet.Cell(6, 2).Value = ozet.KayitSayisi;

            wsOzet.Range(3, 1, 6, 1).Style.Font.Bold = true;
            wsOzet.Columns().AdjustToContents();

            // Kategori Ozet
            var wsKategori = workbook.Worksheets.Add("Kategori Analizi");
            wsKategori.Cell(1, 1).Value = "Kategori";
            wsKategori.Cell(1, 2).Value = "Gelir";
            wsKategori.Cell(1, 3).Value = "Gider";
            wsKategori.Cell(1, 4).Value = "Net";
            wsKategori.Cell(1, 5).Value = "Islem Sayisi";
            wsKategori.Range(1, 1, 1, 5).Style.Font.Bold = true;
            wsKategori.Range(1, 1, 1, 5).Style.Fill.BackgroundColor = XLColor.LightGray;

            int row = 2;
            foreach (var kat in kategoriler)
            {
                wsKategori.Cell(row, 1).Value = kat.Kategori;
                wsKategori.Cell(row, 2).Value = kat.ToplamGelir;
                wsKategori.Cell(row, 3).Value = kat.ToplamGider;
                wsKategori.Cell(row, 4).Value = kat.Net;
                wsKategori.Cell(row, 5).Value = kat.IslemSayisi;
                row++;
            }
            wsKategori.Range(2, 2, row - 1, 4).Style.NumberFormat.Format = "#,##0.00";
            wsKategori.Columns().AdjustToContents();

            // Aylik Ozet
            var wsAylik = workbook.Worksheets.Add("Aylik Analiz");
            wsAylik.Cell(1, 1).Value = "Ay";
            wsAylik.Cell(1, 2).Value = "Gelir";
            wsAylik.Cell(1, 3).Value = "Gider";
            wsAylik.Cell(1, 4).Value = "Net";
            wsAylik.Cell(1, 5).Value = "Islem Sayisi";
            wsAylik.Range(1, 1, 1, 5).Style.Font.Bold = true;
            wsAylik.Range(1, 1, 1, 5).Style.Fill.BackgroundColor = XLColor.LightGray;

            row = 2;
            foreach (var ay in ayliklar)
            {
                wsAylik.Cell(row, 1).Value = ay.AyAdi;
                wsAylik.Cell(row, 2).Value = ay.ToplamGelir;
                wsAylik.Cell(row, 3).Value = ay.ToplamGider;
                wsAylik.Cell(row, 4).Value = ay.Net;
                wsAylik.Cell(row, 5).Value = ay.IslemSayisi;
                row++;
            }
            wsAylik.Range(2, 2, row - 1, 4).Style.NumberFormat.Format = "#,##0.00";
            wsAylik.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        });
    }
}
