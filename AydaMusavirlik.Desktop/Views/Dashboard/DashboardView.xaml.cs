using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using AydaMusavirlik.Desktop.Services;

namespace AydaMusavirlik.Desktop.Views.Dashboard;

public partial class DashboardView : UserControl
{
    private readonly ICompanyService _companyService;
    private readonly IEmployeeService _employeeService;

    public DashboardView()
    {
        InitializeComponent();
        _companyService = App.GetService<ICompanyService>();
        _employeeService = App.GetService<IEmployeeService>();
        Loaded += DashboardView_Loaded;
    }

    private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        txtTarih.Text = DateTime.Now.ToString("dd MMMM yyyy, dddd");

        // Donem combobox
        var currentYear = DateTime.Now.Year;
        for (int y = currentYear - 2; y <= currentYear; y++)
        {
            for (int m = 1; m <= 12; m++)
            {
                if (y == currentYear && m > DateTime.Now.Month) break;
                cmbDonem.Items.Add($"{y}/{m:D2}");
            }
        }
        cmbDonem.SelectedItem = $"{currentYear}/{DateTime.Now.Month:D2}";

        await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            // Ornek veriler (gercek uygulamada servislerden cekilecek)
            var toplamGelir = 1250000m;
            var toplamGider = 980000m;
            var netKar = toplamGelir - toplamGider;
            var karMarji = toplamGelir > 0 ? (netKar / toplamGelir) * 100 : 0;
            var maliSaglik = 78;

            // Kart verilerini guncelle
            txtToplamGelir.Text = $"{toplamGelir:N0} TL";
            txtGelirDegisim.Text = "+%12.5 gecen aya gore";
            txtToplamGider.Text = $"{toplamGider:N0} TL";
            txtGiderDegisim.Text = "-%3.2 gecen aya gore";
            txtNetKar.Text = $"{netKar:N0} TL";
            txtKarMarji.Text = $"Kar Marji: %{karMarji:N1}";
            
            // Personel bilgisi - ilk firmadan al
            var companies = await _companyService.GetAllAsync();
            var firstCompany = companies.FirstOrDefault();
            if (firstCompany != null)
            {
                var employees = await _employeeService.GetActiveByCompanyAsync(firstCompany.Id);
                txtPersonelSayisi.Text = employees.Count().ToString();
                txtBordroToplam.Text = $"Bordro: {employees.Sum(e => e.GrossSalary):N0} TL";
            }
            else
            {
                txtPersonelSayisi.Text = "0";
                txtBordroToplam.Text = "Bordro: 0 TL";
            }

            // Mali saglik
            txtMaliSaglik.Text = $"{maliSaglik}/100";
            txtMaliSaglikSeviye.Text = GetMaliSaglikSeviye(maliSaglik);
            brdMaliSaglik.Background = new SolidColorBrush(GetMaliSaglikColor(maliSaglik));

            // Mali oranlar
            txtCariOran.Text = "2.50";
            txtAsitTest.Text = "1.80";
            txtBorcOzkaynak.Text = "0.40";
            txtROE.Text = "%20.0";
            txtROA.Text = "%12.0";

            // Grafikleri yukle
            LoadGelirGiderChart();
            LoadGiderDagilimiChart();
            LoadSonIslemler();
            LoadHatirlatmalar();
        }
        catch (Exception ex)
        {
            // Hata durumunda ornek verilerle devam et
            txtPersonelSayisi.Text = "12";
            txtBordroToplam.Text = "Bordro: 185.000 TL";
            LoadGelirGiderChart();
            LoadGiderDagilimiChart();
            LoadSonIslemler();
            LoadHatirlatmalar();
        }
    }

    private void LoadGelirGiderChart()
    {
        var aylar = new[] { "Oca", "Þub", "Mar", "Nis", "May", "Haz", "Tem", "Aðu", "Eyl", "Eki", "Kas", "Ara" };
        var gelirler = new double[] { 95, 102, 98, 110, 105, 115, 108, 120, 118, 125, 130, 128 };
        var giderler = new double[] { 75, 80, 78, 85, 82, 88, 84, 92, 90, 95, 98, 96 };

        chartGelirGider.Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = gelirler,
                Name = "Gelir (x1000)",
                Stroke = new SolidColorPaint(new SKColor(76, 175, 80)) { StrokeThickness = 3 }, // Yeþil
                Fill = null,
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(new SKColor(76, 175, 80)) { StrokeThickness = 2 }
            },
            new LineSeries<double>
            {
                Values = giderler,
                Name = "Gider (x1000)",
                Stroke = new SolidColorPaint(new SKColor(244, 67, 54)) { StrokeThickness = 3 }, // Kýrmýzý
                Fill = null,
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(new SKColor(244, 67, 54)) { StrokeThickness = 2 }
            }
        };

        chartGelirGider.XAxes = new Axis[]
        {
            new Axis
            {
                Labels = aylar,
                LabelsPaint = new SolidColorPaint(new SKColor(97, 97, 97)), // Koyu gri
                TextSize = 11
            }
        };

        chartGelirGider.YAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(new SKColor(97, 97, 97)),
                TextSize = 11
            }
        };
    }

    private void LoadGiderDagilimiChart()
    {
        chartGiderDagilimi.Series = new ISeries[]
        {
            new PieSeries<double> { Values = new double[] { 35 }, Name = "Personel", Fill = new SolidColorPaint(new SKColor(33, 150, 243)) },
            new PieSeries<double> { Values = new double[] { 20 }, Name = "Kira", Fill = new SolidColorPaint(new SKColor(255, 152, 0)) },
            new PieSeries<double> { Values = new double[] { 15 }, Name = "Enerji", Fill = new SolidColorPaint(new SKColor(156, 39, 176)) },
            new PieSeries<double> { Values = new double[] { 12 }, Name = "Hammadde", Fill = new SolidColorPaint(new SKColor(76, 175, 80)) },
            new PieSeries<double> { Values = new double[] { 10 }, Name = "Pazarlama", Fill = new SolidColorPaint(new SKColor(244, 67, 54)) },
            new PieSeries<double> { Values = new double[] { 8 }, Name = "Diðer", Fill = new SolidColorPaint(new SKColor(158, 158, 158)) }
        };
    }

    private void LoadSonIslemler()
    {
        var islemler = new List<SonIslem>
        {
            new SonIslem { Aciklama = "Personel maaþ ödemesi", Tarih = "Bugün 14:30", Tutar = "-125.000 TL", TutarRenk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")) },
            new SonIslem { Aciklama = "ABC Ltd. tahsilat", Tarih = "Dün 16:45", Tutar = "+45.000 TL", TutarRenk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")) },
            new SonIslem { Aciklama = "Kira ödemesi", Tarih = "22.03.2025", Tutar = "-35.000 TL", TutarRenk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")) },
            new SonIslem { Aciklama = "XYZ A.Þ. fatura", Tarih = "21.03.2025", Tutar = "+78.500 TL", TutarRenk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")) },
            new SonIslem { Aciklama = "SGK ödemesi", Tarih = "20.03.2025", Tutar = "-42.000 TL", TutarRenk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")) }
        };
        lstSonIslemler.ItemsSource = islemler;
    }

    private void LoadHatirlatmalar()
    {
        pnlHatirlatmalar.Children.Clear();

        var hatirlatmalar = new[]
        {
            ("??", "SGK Bildirge son gunu: 3 gun", "#FF9800"),
            ("??", "KDV Beyanname: 5 gun", "#FF9800"),
            ("??", "Gecikmi?s fatura: ABC Ltd.", "#F44336"),
            ("??", "Personel izin talebi bekliyor", "#2196F3")
        };

        foreach (var (icon, text, color) in hatirlatmalar)
        {
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E42")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 8, 10, 8),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var stack = new StackPanel { Orientation = Orientation.Horizontal };
            stack.Children.Add(new TextBlock { Text = icon, Margin = new Thickness(0, 0, 8, 0) });
            stack.Children.Add(new TextBlock 
            { 
                Text = text, 
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                FontSize = 12
            });

            border.Child = stack;
            pnlHatirlatmalar.Children.Add(border);
        }
    }

    private string GetMaliSaglikSeviye(int puan)
    {
        if (puan >= 80) return "A+ Mukemmel";
        if (puan >= 70) return "B+ Cok Iyi";
        if (puan >= 60) return "B Iyi";
        if (puan >= 50) return "C Orta";
        if (puan >= 40) return "D Dikkat";
        return "F Kritik";
    }

    private Color GetMaliSaglikColor(int puan)
    {
        if (puan >= 70) return (Color)ColorConverter.ConvertFromString("#2D5A27");
        if (puan >= 50) return (Color)ColorConverter.ConvertFromString("#FF9800");
        return (Color)ColorConverter.ConvertFromString("#8B0000");
    }

    private void BtnYenile_Click(object sender, RoutedEventArgs e)
    {
        _ = LoadDashboardDataAsync();
    }
}

public class SonIslem
{
    public string Aciklama { get; set; } = string.Empty;
    public string Tarih { get; set; } = string.Empty;
    public string Tutar { get; set; } = string.Empty;
    public Brush TutarRenk { get; set; } = Brushes.White;
}