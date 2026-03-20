using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Desktop.Services.Reports;

namespace AydaMusavirlik.Desktop.Views.Reports;

public partial class ReportsView : UserControl
{
    private readonly IExcelExportService _excelService;
    private readonly IPdfExportService _pdfService;
    private readonly IPayrollService _payrollService;
    private readonly IAccountService _accountService;
    private readonly ICompanyService _companyService;
    private readonly IEmployeeService _employeeService;
    private int _currentCompanyId = 1;

    public ReportsView()
    {
        InitializeComponent();
        _excelService = App.GetService<IExcelExportService>();
        _pdfService = App.GetService<IPdfExportService>();
        _payrollService = App.GetService<IPayrollService>();
        _accountService = App.GetService<IAccountService>();
        _companyService = App.GetService<ICompanyService>();
        _employeeService = App.GetService<IEmployeeService>();
    }

    #region Mali Tablolar

    private async void Bilanco_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var report = await GenerateBalanceSheetAsync();
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Dosyasi (*.xlsx)|*.xlsx",
                FileName = $"Bilanco_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await _excelService.ExportBalanceSheetAsync(report, saveDialog.FileName);
                MessageBox.Show($"Bilanco raporu olusturuldu:\n{saveDialog.FileName}", 
                    "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void GelirTablosu_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var report = await GenerateIncomeStatementAsync();
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Dosyasi (*.xlsx)|*.xlsx",
                FileName = $"GelirTablosu_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await _excelService.ExportIncomeStatementAsync(report, saveDialog.FileName);
                MessageBox.Show($"Gelir tablosu raporu olusturuldu:\n{saveDialog.FileName}", 
                    "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Mizan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var startDate = new DateTime(DateTime.Now.Year, 1, 1);
            var endDate = DateTime.Now;
            var trialBalance = await _accountService.GetTrialBalanceAsync(_currentCompanyId, startDate, endDate);

            if (trialBalance == null)
            {
                MessageBox.Show("Mizan verisi bulunamadi.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Dosyasi (*.xlsx)|*.xlsx",
                FileName = $"Mizan_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await _excelService.ExportTrialBalanceAsync(trialBalance, saveDialog.FileName);
                MessageBox.Show($"Mizan raporu olusturuldu:\n{saveDialog.FileName}", 
                    "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Kebir_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Kebir Defteri raporu yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion

    #region Bordro Raporlari

    private async void BordroOzet_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;

            var payrolls = await _payrollService.GetByPeriodAsync(_currentCompanyId, year, month);

            if (!payrolls.Any())
            {
                // Hesapla
                var result = await _payrollService.CalculateAllAsync(new CalculateAllPayrollDto
                {
                    CompanyId = _currentCompanyId,
                    Year = year,
                    Month = month,
                    WorkingDays = 30
                });

                if (result != null)
                {
                    payrolls = await _payrollService.GetByPeriodAsync(_currentCompanyId, year, month);
                }
            }

            if (!payrolls.Any())
            {
                MessageBox.Show("Bu donem icin bordro verisi bulunamadi.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Dosyasi (*.xlsx)|*.xlsx",
                FileName = $"Bordro_{year}_{month:D2}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await _excelService.ExportPayrollAsync(payrolls, saveDialog.FileName);
                MessageBox.Show($"Bordro raporu olusturuldu:\n{saveDialog.FileName}", 
                    "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BordroPdf_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;

            var payrolls = await _payrollService.GetByPeriodAsync(_currentCompanyId, year, month);

            if (!payrolls.Any())
            {
                var result = await _payrollService.CalculateAllAsync(new CalculateAllPayrollDto
                {
                    CompanyId = _currentCompanyId,
                    Year = year,
                    Month = month,
                    WorkingDays = 30
                });

                if (result != null)
                {
                    payrolls = await _payrollService.GetByPeriodAsync(_currentCompanyId, year, month);
                }
            }

            if (!payrolls.Any())
            {
                MessageBox.Show("Bu donem icin bordro verisi bulunamadi.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var company = await _companyService.GetByIdAsync(_currentCompanyId);
            var companyName = company?.Name ?? "Firma";

            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF Dosyasi (*.pdf)|*.pdf",
                FileName = $"Bordro_{year}_{month:D2}.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await _pdfService.ExportPayrollAsync(payrolls, year, month, companyName, saveDialog.FileName);
                MessageBox.Show($"Bordro PDF raporu olusturuldu:\n{saveDialog.FileName}", 
                    "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BordroDetay_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Personel Bordro Detay raporu yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SgkBildirge_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("SGK Bildirge raporu yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Muhtasar_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Muhtasar Beyanname raporu yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion

    #region Vergi Raporlari

    private void KdvBeyan_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("KDV Beyannamesi raporu yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void GeciciVergi_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Gecici Vergi raporu yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BaBeFormlar_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Ba-Bs Formlari raporu yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion

    #region Diger Raporlar

    private void CariHesap_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Cari Hesap Ekstre raporu yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void FirmaKarti_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Firma Karti raporu yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void PersonelListe_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var employees = await _employeeService.GetActiveByCompanyAsync(_currentCompanyId);

            if (!employees.Any())
            {
                MessageBox.Show("Personel verisi bulunamadi.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Dosyasi (*.xlsx)|*.xlsx",
                FileName = $"PersonelListesi_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await ExportEmployeeListAsync(employees, saveDialog.FileName);
                MessageBox.Show($"Personel listesi olusturuldu:\n{saveDialog.FileName}", 
                    "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<BalanceSheetReport> GenerateBalanceSheetAsync()
    {
        var accounts = await _accountService.GetByCompanyAsync(_currentCompanyId);

        return new BalanceSheetReport
        {
            ReportDate = DateTime.Now,
            DonenVarliklar = new BalanceSheetSection
            {
                Title = "I. DONEN VARLIKLAR",
                Items = accounts.Where(a => a.Code.StartsWith("1"))
                    .Select(a => new BalanceSheetItem { Code = a.Code, Name = a.Name, Amount = a.CurrentBalance })
                    .ToList()
            },
            DuranVarliklar = new BalanceSheetSection
            {
                Title = "II. DURAN VARLIKLAR",
                Items = accounts.Where(a => a.Code.StartsWith("2"))
                    .Select(a => new BalanceSheetItem { Code = a.Code, Name = a.Name, Amount = a.CurrentBalance })
                    .ToList()
            },
            KisaVadeliYabanciKaynaklar = new BalanceSheetSection
            {
                Title = "I. KISA VADELI YABANCI KAYNAKLAR",
                Items = accounts.Where(a => a.Code.StartsWith("3"))
                    .Select(a => new BalanceSheetItem { Code = a.Code, Name = a.Name, Amount = a.CurrentBalance })
                    .ToList()
            },
            UzunVadeliYabanciKaynaklar = new BalanceSheetSection
            {
                Title = "II. UZUN VADELI YABANCI KAYNAKLAR",
                Items = accounts.Where(a => a.Code.StartsWith("4"))
                    .Select(a => new BalanceSheetItem { Code = a.Code, Name = a.Name, Amount = a.CurrentBalance })
                    .ToList()
            },
            OzKaynaklar = new BalanceSheetSection
            {
                Title = "III. OZ KAYNAKLAR",
                Items = accounts.Where(a => a.Code.StartsWith("5"))
                    .Select(a => new BalanceSheetItem { Code = a.Code, Name = a.Name, Amount = a.CurrentBalance })
                    .ToList()
            }
        };
    }

    private async Task<IncomeStatementReport> GenerateIncomeStatementAsync()
    {
        var accounts = await _accountService.GetByCompanyAsync(_currentCompanyId);
        
        var items = new List<IncomeStatementItem>();
        
        // Satislar
        items.AddRange(accounts.Where(a => a.Code.StartsWith("60"))
            .Select(a => new IncomeStatementItem { Code = a.Code, Name = a.Name, Amount = a.CurrentBalance }));
        
        // Satis Indirimleri
        items.AddRange(accounts.Where(a => a.Code.StartsWith("61"))
            .Select(a => new IncomeStatementItem { Code = a.Code, Name = a.Name, Amount = a.CurrentBalance }));
        
        // Satis Maliyeti
        items.AddRange(accounts.Where(a => a.Code.StartsWith("62"))
            .Select(a => new IncomeStatementItem { Code = a.Code, Name = a.Name, Amount = a.CurrentBalance }));
        
        // Faaliyet Giderleri
        items.AddRange(accounts.Where(a => a.Code.StartsWith("63"))
            .Select(a => new IncomeStatementItem { Code = a.Code, Name = a.Name, Amount = a.CurrentBalance }));
        
        // Diger Gelir/Giderler
        items.AddRange(accounts.Where(a => a.Code.StartsWith("64") || a.Code.StartsWith("65") || a.Code.StartsWith("66"))
            .Select(a => new IncomeStatementItem { Code = a.Code, Name = a.Name, Amount = a.CurrentBalance }));

        return new IncomeStatementReport
        {
            StartDate = new DateTime(DateTime.Now.Year, 1, 1),
            EndDate = DateTime.Now,
            Items = items
        };
    }

    private async Task ExportEmployeeListAsync(IEnumerable<EmployeeDto> employees, string filePath)
    {
        await Task.Run(() =>
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Personel Listesi");

            ws.Cell("A1").Value = "PERSONEL LISTESI";
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 16;
            ws.Range("A1:H1").Merge();

            int row = 3;
            ws.Cell(row, 1).Value = "SICIL NO";
            ws.Cell(row, 2).Value = "AD SOYAD";
            ws.Cell(row, 3).Value = "TC KIMLIK";
            ws.Cell(row, 4).Value = "DEPARTMAN";
            ws.Cell(row, 5).Value = "POZISYON";
            ws.Cell(row, 6).Value = "ISE GIRIS";
            ws.Cell(row, 7).Value = "BRUT MAAS";
            ws.Cell(row, 8).Value = "DURUM";

            ws.Range(row, 1, row, 8).Style.Font.Bold = true;
            ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            row++;

            foreach (var emp in employees)
            {
                ws.Cell(row, 1).Value = emp.EmployeeNumber;
                ws.Cell(row, 2).Value = emp.FullName;
                ws.Cell(row, 3).Value = emp.TcKimlikNo;
                ws.Cell(row, 4).Value = emp.Department;
                ws.Cell(row, 5).Value = emp.Position;
                ws.Cell(row, 6).Value = emp.HireDate.ToString("dd.MM.yyyy");
                ws.Cell(row, 7).Value = emp.GrossSalary;
                ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 8).Value = emp.IsActive ? "Aktif" : "Pasif";
                row++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        });
    }

    #endregion
}