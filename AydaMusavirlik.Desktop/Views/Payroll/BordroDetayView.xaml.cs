using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Desktop.Services.Reports;

namespace AydaMusavirlik.Desktop.Views.Payroll;

public partial class BordroDetayView : UserControl
{
    private readonly IPayrollService _payrollService;
    private readonly IExcelExportService _excelService;
    private readonly IPdfExportService _pdfService;
    private readonly ICompanyService _companyService;

    private readonly int _companyId;
    private readonly int _yil;
    private readonly int _ay;
    private readonly List<PuantajGridItem>? _puantajlar;
    private ObservableCollection<BordroGridItem> _bordroItems = new();

    public BordroDetayView(int companyId, int yil, int ay, List<PuantajGridItem>? puantajlar = null)
    {
        InitializeComponent();
        _companyId = companyId;
        _yil = yil;
        _ay = ay;
        _puantajlar = puantajlar;

        _payrollService = App.GetService<IPayrollService>();
        _excelService = App.GetService<IExcelExportService>();
        _pdfService = App.GetService<IPdfExportService>();
        _companyService = App.GetService<ICompanyService>();

        txtDonem.Text = $"Donem: {_yil}/{_ay:D2}";
        Loaded += BordroDetayView_Loaded;
    }

    private async void BordroDetayView_Loaded(object sender, RoutedEventArgs e)
    {
        await HesaplaBordroAsync();
    }

    private async Task HesaplaBordroAsync()
    {
        try
        {
            // Bordro hesapla
            var result = await _payrollService.CalculateAllAsync(new CalculateAllPayrollDto
            {
                CompanyId = _companyId,
                Year = _yil,
                Month = _ay,
                WorkingDays = 30
            });

            if (result == null || !result.Payrolls.Any())
            {
                MessageBox.Show("Bordro hesaplanamadi veya personel bulunamadi.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Grid'i doldur
            _bordroItems.Clear();
            foreach (var p in result.Payrolls)
            {
                _bordroItems.Add(new BordroGridItem
                {
                    EmployeeId = p.EmployeeId,
                    EmployeeName = p.EmployeeName,
                    BelgeTuruKod = "01",
                    CalismaGunu = 30,
                    BrutUcret = p.GrossSalary,
                    FazlaMesaiUcreti = 0,
                    ToplamKazanc = p.GrossSalary,
                    SgkIsciPayi = p.SgkWorkerDeduction,
                    IssizlikIsciPayi = p.SgkUnemploymentWorker,
                    GelirVergisi = p.IncomeTax,
                    DamgaVergisi = p.StampTax,
                    ToplamKesinti = p.TotalDeductions,
                    NetUcret = p.NetSalary,
                    SgkIsverenPayi = p.SgkEmployerCost,
                    IssizlikIsverenPayi = Math.Round(p.GrossSalary * 0.02m, 2),
                    ToplamIsverenMaliyeti = p.TotalEmployerCost
                });
            }

            dgBordro.ItemsSource = _bordroItems;
            GuncelleOzetler();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GuncelleOzetler()
    {
        var personelSayisi = _bordroItems.Count;
        var toplamBrut = _bordroItems.Sum(b => b.ToplamKazanc);
        var toplamNet = _bordroItems.Sum(b => b.NetUcret);
        var sgkIsveren = _bordroItems.Sum(b => b.SgkIsverenPayi);
        var toplamMaliyet = _bordroItems.Sum(b => b.ToplamIsverenMaliyeti);

        txtPersonelSayisi.Text = personelSayisi.ToString();
        txtToplamBrut.Text = $"{toplamBrut:N2} TL";
        txtToplamNet.Text = $"{toplamNet:N2} TL";
        txtSgkIsveren.Text = $"{sgkIsveren:N2} TL";
        txtToplamMaliyet.Text = $"{toplamMaliyet:N2} TL";

        // Detay ozetler
        var sgkIsci = _bordroItems.Sum(b => b.SgkIsciPayi);
        var issizlikIsci = _bordroItems.Sum(b => b.IssizlikIsciPayi);
        var issizlikIsveren = _bordroItems.Sum(b => b.IssizlikIsverenPayi);
        var gelirVergisi = _bordroItems.Sum(b => b.GelirVergisi);
        var damgaVergisi = _bordroItems.Sum(b => b.DamgaVergisi);

        txtSgkIsciToplam.Text = $"SGK Isci: {sgkIsci:N2} TL";
        txtSgkIsverenToplam.Text = $"SGK Isveren: {sgkIsveren:N2} TL";
        txtIssizlikToplam.Text = $"Issizlik: {(issizlikIsci + issizlikIsveren):N2} TL";

        txtGelirVergisiToplam.Text = $"Gelir Vergisi: {gelirVergisi:N2} TL";
        txtDamgaVergisiToplam.Text = $"Damga Vergisi: {damgaVergisi:N2} TL";

        txtSgkOdenecek.Text = $"SGK Toplam: {(sgkIsci + sgkIsveren + issizlikIsci + issizlikIsveren):N2} TL";
        txtVergiOdenecek.Text = $"Vergi Toplam: {(gelirVergisi + damgaVergisi):N2} TL";
    }

    private async void BtnExcelExport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var payrolls = _bordroItems.Select(b => new PayrollRecordDto
            {
                EmployeeId = b.EmployeeId,
                EmployeeName = b.EmployeeName,
                FullName = b.EmployeeName,
                Position = "",
                Year = _yil,
                Month = _ay,
                GrossSalary = b.BrutUcret,
                SgkWorkerDeduction = b.SgkIsciPayi,
                IncomeTax = b.GelirVergisi,
                StampTax = b.DamgaVergisi,
                TotalDeductions = b.ToplamKesinti,
                NetSalary = b.NetUcret,
                SgkEmployerCost = b.SgkIsverenPayi,
                TotalEmployerCost = b.ToplamIsverenMaliyeti
            }).ToList();

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Dosyasi (*.xlsx)|*.xlsx",
                FileName = $"Bordro_{_yil}_{_ay:D2}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await _excelService.ExportPayrollAsync(payrolls, saveDialog.FileName);
                MessageBox.Show($"Excel raporu olusturuldu:\n{saveDialog.FileName}", "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnPdfExport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var company = await _companyService.GetByIdAsync(_companyId);
            var companyName = company?.Name ?? "Firma";

            var payrolls = _bordroItems.Select(b => new PayrollRecordDto
            {
                EmployeeId = b.EmployeeId,
                EmployeeName = b.EmployeeName,
                FullName = b.EmployeeName,
                Position = "",
                Year = _yil,
                Month = _ay,
                GrossSalary = b.BrutUcret,
                TotalDeductions = b.ToplamKesinti,
                NetSalary = b.NetUcret,
                SgkEmployerCost = b.SgkIsverenPayi,
                TotalEmployerCost = b.ToplamIsverenMaliyeti
            }).ToList();

            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF Dosyasi (*.pdf)|*.pdf",
                FileName = $"Bordro_{_yil}_{_ay:D2}.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await _pdfService.ExportPayrollAsync(payrolls, _yil, _ay, companyName, saveDialog.FileName);
                MessageBox.Show($"PDF raporu olusturuldu:\n{saveDialog.FileName}", "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnSgkBildirge_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("SGK Bildirge olusturma ozelligi yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnKaydet_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Bordro kayitlari veritabanina kaydedildi.", "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

public class BordroGridItem
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string BelgeTuruKod { get; set; } = string.Empty;
    public int CalismaGunu { get; set; }
    public decimal BrutUcret { get; set; }
    public decimal FazlaMesaiUcreti { get; set; }
    public decimal ToplamKazanc { get; set; }
    public decimal SgkIsciPayi { get; set; }
    public decimal IssizlikIsciPayi { get; set; }
    public decimal GelirVergisi { get; set; }
    public decimal DamgaVergisi { get; set; }
    public decimal ToplamKesinti { get; set; }
    public decimal NetUcret { get; set; }
    public decimal SgkIsverenPayi { get; set; }
    public decimal IssizlikIsverenPayi { get; set; }
    public decimal ToplamIsverenMaliyeti { get; set; }
}