using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Desktop.Services.Reports;

namespace AydaMusavirlik.Desktop.Views.Reports;

public partial class FinancialAnalysisView : UserControl
{
    private readonly IFinancialAnalysisService _analysisService;
    private readonly IPdfExportService _pdfService;
    private readonly IReportGeneratorService _reportService;

    private FinancialHealthScore? _currentScore;
    private LiquidityRatios? _liquidityRatios;
    private ProfitabilityRatios? _profitabilityRatios;
    private LeverageRatios? _leverageRatios;
    private ActivityRatios? _activityRatios;

    public FinancialAnalysisView()
    {
        InitializeComponent();
        
        _analysisService = new FinancialAnalysisService();
        _pdfService = App.GetService<IPdfExportService>();
        _reportService = App.GetService<IReportGeneratorService>();

        Loaded += FinancialAnalysisView_Loaded;
    }

    private void FinancialAnalysisView_Loaded(object sender, RoutedEventArgs e)
    {
        // Ornek verilerle baslat
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        // Ornek veriler yukleniyor
        txtSkor.Text = "78";
        txtDerece.Text = "Mali Saglik Derecesi: B+";
        txtYorum.Text = "Iyi mali saglik. Bazi alanlarda iyilestirme yapilabilir.";
        
        txtLikiditePuan.Text = "20/25";
        txtKarlilikPuan.Text = "18/25";
        txtBorclulukPuan.Text = "22/25";
        txtVerimlilikPuan.Text = "18/25";
    }

    private async void AnalizYap_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            btnAnalizYap.IsEnabled = false;
            btnAnalizYap.Content = "Analiz yapiliyor...";
            
            // Ornek bilanco ve gelir tablosu olustur
            var balance = GenerateSampleBalance();
            var income = GenerateSampleIncome();

            // Oranlari hesapla
            _liquidityRatios = _analysisService.CalculateLiquidityRatios(balance);
            _profitabilityRatios = _analysisService.CalculateProfitabilityRatios(balance, income);
            _leverageRatios = _analysisService.CalculateLeverageRatios(balance);
            _activityRatios = _analysisService.CalculateActivityRatios(balance, income);
            _currentScore = _analysisService.CalculateHealthScore(balance, income);

            // UI guncelle
            UpdateUI();

            MessageBox.Show("Mali analiz basariyla tamamlandi!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Analiz sirasinda hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            btnAnalizYap.IsEnabled = true;
            btnAnalizYap.Content = "Analiz Yap";
        }
    }

    private void UpdateUI()
    {
        if (_currentScore == null) return;

        // Skor
        txtSkor.Text = $"{_currentScore.TotalScore:N0}";
        txtDerece.Text = $"Mali Saglik Derecesi: {_currentScore.Grade}";
        txtYorum.Text = _currentScore.Interpretation;

        // Puan dagilimi
        txtLikiditePuan.Text = $"{_currentScore.LiquidityScore:N0}/25";
        txtKarlilikPuan.Text = $"{_currentScore.ProfitabilityScore:N0}/25";
        txtBorclulukPuan.Text = $"{_currentScore.LeverageScore:N0}/25";
        txtVerimlilikPuan.Text = $"{_currentScore.ActivityScore:N0}/25";

        // Likidite
        if (_liquidityRatios != null)
        {
            txtCariOran.Text = $"{_liquidityRatios.CariOran:N2}";
            txtAsitTest.Text = $"{_liquidityRatios.AsitTestOrani:N2}";
            txtNakitOran.Text = $"{_liquidityRatios.NakitOrani:N2}";
            txtNetIsletmeSermayesi.Text = $"{_liquidityRatios.NetIsletmeSermayesi:N0} TL";
        }

        // Karlilik
        if (_profitabilityRatios != null)
        {
            txtBrutKarMarji.Text = $"%{_profitabilityRatios.BrutKarMarji:N2}";
            txtNetKarMarji.Text = $"%{_profitabilityRatios.NetKarMarji:N2}";
            txtROE.Text = $"%{_profitabilityRatios.OzkaynakKarliligi:N2}";
            txtROA.Text = $"%{_profitabilityRatios.AktifKarliligi:N2}";
        }

        // Borcluluk
        if (_leverageRatios != null)
        {
            txtBorcOzkaynak.Text = $"{_leverageRatios.BorcOzkaynakOrani:N2}";
            txtToplamBorc.Text = $"%{_leverageRatios.ToplamBorcOrani:N2}";
            txtOzkaynakOrani.Text = $"%{_leverageRatios.OzkaynakOrani:N2}";
            txtFinansalKaldirac.Text = $"{_leverageRatios.FinansalKaldirac:N2}";
        }

        // Verimlilik
        if (_activityRatios != null)
        {
            txtStokDevir.Text = $"{_activityRatios.StokDevirHizi:N2} kez";
            txtStokDevirSure.Text = $"{_activityRatios.StokDevirSuresi:N0} gun";
            txtAlacakTahsil.Text = $"{_activityRatios.AlacakTahsilSuresi:N0} gun";
            txtAktifDevir.Text = $"{_activityRatios.AktifDevirHizi:N2} kez";
        }
    }

    private async void PdfExport_Click(object sender, RoutedEventArgs e)
    {
        if (_currentScore == null || _liquidityRatios == null || _profitabilityRatios == null || _leverageRatios == null)
        {
            MessageBox.Show("Once analiz yapmaniz gerekmektedir.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "PDF Dosyasi|*.pdf",
            FileName = $"MaliAnaliz_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _pdfService.ExportFinancialAnalysisAsync(_currentScore, _liquidityRatios, 
                    _profitabilityRatios, _leverageRatios, dialog.FileName);
                
                MessageBox.Show($"PDF basariyla olusturuldu:\n{dialog.FileName}", "Basarili", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                // PDF'i ac
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dialog.FileName,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF olusturulurken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ExcelExport_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Excel export ozelligi yakin zamanda eklenecek.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private BalanceSheetReport GenerateSampleBalance()
    {
        return new BalanceSheetReport
        {
            ReportDate = DateTime.Now,
            DonenVarliklar = new BalanceSheetSection
            {
                Title = "DONEN VARLIKLAR",
                Items = new List<BalanceSheetItem>
                {
                    new() { Code = "100", Name = "KASA", Amount = 50000 },
                    new() { Code = "102", Name = "BANKALAR", Amount = 450000 },
                    new() { Code = "120", Name = "ALICILAR", Amount = 280000 },
                    new() { Code = "150", Name = "STOKLAR", Amount = 180000 },
                }
            },
            DuranVarliklar = new BalanceSheetSection
            {
                Title = "DURAN VARLIKLAR",
                Items = new List<BalanceSheetItem>
                {
                    new() { Code = "253", Name = "TESIS MAKINA", Amount = 320000 },
                    new() { Code = "254", Name = "TASITLAR", Amount = 150000 },
                }
            },
            KisaVadeliYabanciKaynaklar = new BalanceSheetSection
            {
                Title = "KISA VADELI YABANCI KAYNAKLAR",
                Items = new List<BalanceSheetItem>
                {
                    new() { Code = "300", Name = "BANKA KREDILERI", Amount = 200000 },
                    new() { Code = "320", Name = "SATICILAR", Amount = 180000 },
                }
            },
            UzunVadeliYabanciKaynaklar = new BalanceSheetSection
            {
                Title = "UZUN VADELI YABANCI KAYNAKLAR",
                Items = new List<BalanceSheetItem>
                {
                    new() { Code = "400", Name = "BANKA KREDILERI", Amount = 250000 },
                }
            },
            OzKaynaklar = new BalanceSheetSection
            {
                Title = "OZ KAYNAKLAR",
                Items = new List<BalanceSheetItem>
                {
                    new() { Code = "500", Name = "SERMAYE", Amount = 500000 },
                    new() { Code = "570", Name = "GECMIS YIL KARLARI", Amount = 200000 },
                    new() { Code = "590", Name = "DONEM NET KARI", Amount = 100000 },
                }
            }
        };
    }

    private IncomeStatementReport GenerateSampleIncome()
    {
        return new IncomeStatementReport
        {
            StartDate = new DateTime(DateTime.Now.Year, 1, 1),
            EndDate = DateTime.Now,
            Items = new List<IncomeStatementItem>
            {
                new() { Code = "600", Name = "YURTICI SATISLAR", Amount = 2500000 },
                new() { Code = "610", Name = "SATIS INDIRIMLERI", Amount = 50000 },
                new() { Code = "620", Name = "SATILAN MAMULLER MALIYETI", Amount = 1800000 },
                new() { Code = "630", Name = "AR-GE GIDERLERI", Amount = 100000 },
                new() { Code = "631", Name = "PAZARLAMA GIDERLERI", Amount = 150000 },
                new() { Code = "632", Name = "GENEL YONETIM GIDERLERI", Amount = 200000 },
            }
        };
    }
}