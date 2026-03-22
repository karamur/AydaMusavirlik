using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Core.Models.FinancialAnalysis;

namespace AydaMusavirlik.Desktop.Views.Reports;

public partial class MaliAnalizView : UserControl
{
    private readonly ICompanyService _companyService;
    private RasyoAnaliziResult? _rasyoResult;
    private DuPontAnaliziResult? _dupontResult;
    private SwotAnaliziResult? _swotResult;

    public MaliAnalizView()
    {
        InitializeComponent();
        _companyService = App.GetService<ICompanyService>();
        Loaded += MaliAnalizView_Loaded;
    }

    private async void MaliAnalizView_Loaded(object sender, RoutedEventArgs e)
    {
        // Yil combobox
        var currentYear = DateTime.Now.Year;
        for (int y = currentYear - 5; y <= currentYear; y++)
            cmbYil.Items.Add(y);
        cmbYil.SelectedItem = currentYear;

        // Firma listesi
        try
        {
            var firmalar = await _companyService.GetAllAsync();
            cmbFirma.ItemsSource = firmalar;
            if (firmalar.Any())
                cmbFirma.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Firma listesi yuklenemedi: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnAnaliz_Click(object sender, RoutedEventArgs e)
    {
        if (cmbFirma.SelectedItem == null || cmbYil.SelectedItem == null)
        {
            MessageBox.Show("Lutfen firma ve yil secin.", "Uyari", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            btnAnaliz.IsEnabled = false;
            btnAnaliz.Content = "Analiz yapiliyor...";

            var firma = (CompanyDto)cmbFirma.SelectedItem;
            var yil = (int)cmbYil.SelectedItem;

            // Analizleri yap (ornek verilerle)
            _rasyoResult = CreateSampleRasyoResult(firma.Name, yil);
            _dupontResult = CreateSampleDuPontResult(firma.Name, yil);
            _swotResult = CreateSampleSwotResult(firma.Name, yil);

            // Ekranlari doldur
            LoadRasyoAnalizi();
            LoadDuPontAnalizi();
            LoadSwotAnalizi();
            LoadTrendAnalizi(firma.Name, yil);
            LoadMaliSaglik();

            MessageBox.Show("Mali analiz tamamlandi!", "Basarili", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            btnAnaliz.IsEnabled = true;
            btnAnaliz.Content = "Analiz Yap";
        }
    }

    private void LoadRasyoAnalizi()
    {
        if (_rasyoResult == null) return;
        pnlRasyo.Children.Clear();

        // Likidite Rasyolari
        AddSectionTitle(pnlRasyo, "?? LÝKÝDÝTE RASYOLARI", "#2196F3");
        AddRasyoCard(pnlRasyo, "Cari Oran", _rasyoResult.Likidite.CariOran, "", _rasyoResult.Likidite.CariOranYorum, GetRasyoColor(_rasyoResult.Likidite.CariOran, 1, 2));
        AddRasyoCard(pnlRasyo, "Asit Test Orani", _rasyoResult.Likidite.AsitTestOrani, "", _rasyoResult.Likidite.AsitTestYorum, GetRasyoColor(_rasyoResult.Likidite.AsitTestOrani, 0.7m, 1));
        AddRasyoCard(pnlRasyo, "Nakit Orani", _rasyoResult.Likidite.NakitOrani, "", "Nakit / Kisa Vadeli Borclar", "#9C27B0");
        AddRasyoCard(pnlRasyo, "Net Isletme Sermayesi", _rasyoResult.Likidite.NetIsletmeSermayesi, "TL", "Donen Varliklar - KVYK", "#607D8B");

        // Karlilik Rasyolari
        AddSectionTitle(pnlRasyo, "?? KARLILIK RASYOLARI", "#4CAF50");
        AddRasyoCard(pnlRasyo, "Brut Kar Marji", _rasyoResult.Karlilik.BrutKarMarji, "%", "Brut Kar / Net Satislar", "#8BC34A");
        AddRasyoCard(pnlRasyo, "Net Kar Marji", _rasyoResult.Karlilik.NetKarMarji, "%", _rasyoResult.Karlilik.NetKarMarjiYorum, GetRasyoColor(_rasyoResult.Karlilik.NetKarMarji, 5, 15));
        AddRasyoCard(pnlRasyo, "ROA (Aktif Karliligi)", _rasyoResult.Karlilik.AktifKarliligi, "%", "Net Kar / Toplam Aktif", "#00BCD4");
        AddRasyoCard(pnlRasyo, "ROE (Ozsermaye Karliligi)", _rasyoResult.Karlilik.OzsermayeKarliligi, "%", _rasyoResult.Karlilik.ROEYorum, GetRasyoColor(_rasyoResult.Karlilik.OzsermayeKarliligi, 10, 20));

        // Finansal Yapi
        AddSectionTitle(pnlRasyo, "?? FÝNANSAL YAPI RASYOLARI", "#FF9800");
        AddRasyoCard(pnlRasyo, "Borc Orani", _rasyoResult.FinansalYapi.BorcOrani, "%", _rasyoResult.FinansalYapi.BorcOraniYorum, GetReverseRasyoColor(_rasyoResult.FinansalYapi.BorcOrani, 40, 70));
        AddRasyoCard(pnlRasyo, "Ozsermaye Orani", _rasyoResult.FinansalYapi.OzsermayeOrani, "%", "Ozsermaye / Toplam Aktif", "#009688");
        AddRasyoCard(pnlRasyo, "Finansal Kaldirac", _rasyoResult.FinansalYapi.FinansalKaldirac, "", _rasyoResult.FinansalYapi.FinansalKaldiracYorum, "#673AB7");
    }

    private void LoadDuPontAnalizi()
    {
        if (_dupontResult == null) return;
        pnlDuPont.Children.Clear();

        AddSectionTitle(pnlDuPont, "?? DuPont ANALÝZÝ - ROE Ayristirmasi", "#E91E63");

        // ROE Formul
        var formulPanel = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 10, 0, 20)
        };
        var formulText = new TextBlock
        {
            Text = "ROE = Net Kar Marji × Varlik Devir Hizi × Finansal Kaldirac",
            FontSize = 18,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        formulPanel.Child = formulText;
        pnlDuPont.Children.Add(formulPanel);

        // Bilesenler
        var grid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        AddDuPontCard(grid, 0, "Net Kar Marji", _dupontResult.NetKarMarji, "%", _dupontResult.NetKarMarjiYorum, "#2196F3");
        AddDuPontCard(grid, 1, "Varlik Devir Hizi", _dupontResult.VarlikDevirHizi, "x", _dupontResult.VarlikDevirHiziYorum, "#4CAF50");
        AddDuPontCard(grid, 2, "Finansal Kaldirac", _dupontResult.FinansalKaldirac, "x", _dupontResult.FinansalKaldiracYorum, "#FF9800");

        pnlDuPont.Children.Add(grid);

        // Sonuclar
        AddSectionTitle(pnlDuPont, "?? SONUÇLAR", "#9C27B0");
        AddDuPontResultCard(pnlDuPont, "ROA (Aktif Karliligi)", _dupontResult.ROA, "%", "#00BCD4");
        AddDuPontResultCard(pnlDuPont, "ROE (Ozsermaye Karliligi)", _dupontResult.ROE, "%", "#E91E63");

        // Yorum
        var yorumPanel = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 20, 0, 0)
        };
        var yorumText = new TextBlock
        {
            Text = _dupontResult.ROEYorum,
            FontSize = 14,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap
        };
        yorumPanel.Child = yorumText;
        pnlDuPont.Children.Add(yorumPanel);
    }

    private void LoadSwotAnalizi()
    {
        if (_swotResult == null) return;
        pnlSwot.Children.Clear();
        pnlSwot.RowDefinitions.Clear();
        pnlSwot.ColumnDefinitions.Clear();

        pnlSwot.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        pnlSwot.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        pnlSwot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        pnlSwot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        AddSwotQuadrant(pnlSwot, 0, 0, "?? GÜÇLÜ YÖNLER", _swotResult.GucluYonler, "#4CAF50");
        AddSwotQuadrant(pnlSwot, 0, 1, "?? ZAYIF YÖNLER", _swotResult.ZayifYonler, "#F44336");
        AddSwotQuadrant(pnlSwot, 1, 0, "?? FIRSATLAR", _swotResult.Firsatlar, "#2196F3");
        AddSwotQuadrant(pnlSwot, 1, 1, "? TEHDÝTLER", _swotResult.Tehditler, "#FF9800");
    }

    private void LoadTrendAnalizi(string firmaAdi, int yil)
    {
        pnlTrend.Children.Clear();
        AddSectionTitle(pnlTrend, "?? TREND ANALÝZÝ (Son 5 Yil)", "#673AB7");

        var infoText = new TextBlock
        {
            Text = $"{firmaAdi} - {yil - 4} ile {yil} yillari arasi performans trendi\n\n" +
                   "• Gelir Buyume Ortalamasi: %10\n" +
                   "• Gider Buyume Ortalamasi: %8\n" +
                   "• Kar Buyume Ortalamasi: %15\n\n" +
                   "Genel Degerlendirme: Firma istikrarli buyume trendinde. " +
                   "Gelirler giderlerden hizli artiyor, kar marji iyilesiyor.",
            FontSize = 14,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 10, 0, 0)
        };
        pnlTrend.Children.Add(infoText);
    }

    private void LoadMaliSaglik()
    {
        if (_rasyoResult == null) return;
        pnlSaglik.Children.Clear();

        var puan = _rasyoResult.MaliSaglikPuani;
        var renk = puan >= 70 ? "#4CAF50" : puan >= 40 ? "#FF9800" : "#F44336";

        AddSectionTitle(pnlSaglik, "?? MALÝ SAÐLIK PUANI", renk);

        // Buyuk puan gosterimi
        var puanPanel = new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(renk)),
            CornerRadius = new CornerRadius(100),
            Width = 200,
            Height = 200,
            Margin = new Thickness(0, 20, 0, 20),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var puanText = new TextBlock
        {
            Text = puan.ToString(),
            FontSize = 72,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        puanPanel.Child = puanText;
        pnlSaglik.Children.Add(puanPanel);

        // Degerlendirme
        var yorumPanel = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 20)
        };
        var yorumText = new TextBlock
        {
            Text = _rasyoResult.GenelDegerlendirme,
            FontSize = 16,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center
        };
        yorumPanel.Child = yorumText;
        pnlSaglik.Children.Add(yorumPanel);
    }

    #region Helper Methods
    private void AddSectionTitle(Panel parent, string title, string color)
    {
        var border = new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(15, 8, 15, 8),
            Margin = new Thickness(0, 15, 0, 10)
        };
        var text = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White
        };
        border.Child = text;
        parent.Children.Add(border);
    }

    private void AddRasyoCard(Panel parent, string baslik, decimal deger, string birim, string aciklama, string color)
    {
        var card = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(15),
            Margin = new Thickness(0, 0, 0, 10)
        };

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = baslik, FontSize = 14, Foreground = Brushes.Gray });
        stack.Children.Add(new TextBlock 
        { 
            Text = $"{deger:N2}{birim}", 
            FontSize = 24, 
            FontWeight = FontWeights.Bold, 
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
            Margin = new Thickness(0, 5, 0, 5)
        });
        stack.Children.Add(new TextBlock { Text = aciklama, FontSize = 12, Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap });

        card.Child = stack;
        parent.Children.Add(card);
    }

    private void AddDuPontCard(Grid parent, int col, string baslik, decimal deger, string birim, string aciklama, string color)
    {
        var card = new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(15),
            Margin = new Thickness(5)
        };
        Grid.SetColumn(card, col);

        var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        stack.Children.Add(new TextBlock { Text = baslik, FontSize = 14, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center });
        stack.Children.Add(new TextBlock 
        { 
            Text = $"{deger:N2}{birim}", 
            FontSize = 28, 
            FontWeight = FontWeights.Bold, 
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 10)
        });
        stack.Children.Add(new TextBlock { Text = aciklama, FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(240,240,240)), TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center });

        card.Child = stack;
        parent.Children.Add(card);
    }

    private void AddDuPontResultCard(Panel parent, string baslik, decimal deger, string birim, string color)
    {
        var card = new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 10)
        };
        var stack = new StackPanel { Orientation = Orientation.Horizontal };
        stack.Children.Add(new TextBlock { Text = baslik + ": ", FontSize = 18, Foreground = Brushes.White });
        stack.Children.Add(new TextBlock { Text = $"{deger:N2}{birim}", FontSize = 24, FontWeight = FontWeights.Bold, Foreground = Brushes.White });
        card.Child = stack;
        parent.Children.Add(card);
    }

    private void AddSwotQuadrant(Grid parent, int row, int col, string baslik, List<SwotItem> items, string color)
    {
        var card = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
            BorderThickness = new Thickness(3, 3, 0, 0),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(5),
            Padding = new Thickness(15)
        };
        Grid.SetRow(card, row);
        Grid.SetColumn(card, col);

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock 
        { 
            Text = baslik, 
            FontSize = 16, 
            FontWeight = FontWeights.Bold, 
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
            Margin = new Thickness(0, 0, 0, 10)
        });

        foreach (var item in items)
        {
            var itemStack = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };
            itemStack.Children.Add(new TextBlock { Text = "• " + item.Baslik, FontSize = 13, Foreground = Brushes.White, FontWeight = FontWeights.SemiBold });
            itemStack.Children.Add(new TextBlock { Text = "  " + item.Aciklama, FontSize = 11, Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap });
            stack.Children.Add(itemStack);
        }

        card.Child = stack;
        parent.Children.Add(card);
    }

    private string GetRasyoColor(decimal deger, decimal orta, decimal iyi)
    {
        if (deger >= iyi) return "#4CAF50";
        if (deger >= orta) return "#FF9800";
        return "#F44336";
    }

    private string GetReverseRasyoColor(decimal deger, decimal iyi, decimal orta)
    {
        if (deger <= iyi) return "#4CAF50";
        if (deger <= orta) return "#FF9800";
        return "#F44336";
    }
    #endregion

    #region Sample Data
    private RasyoAnaliziResult CreateSampleRasyoResult(string firmaAdi, int yil)
    {
        return new RasyoAnaliziResult
        {
            Yil = yil,
            FirmaAdi = firmaAdi,
            MaliSaglikPuani = 72,
            GenelDegerlendirme = "Iyi mali saglik. Kucuk iyilestirmeler yapilabilir.",
            Likidite = new LikiditeRasyolari
            {
                CariOran = 2.5m,
                AsitTestOrani = 1.8m,
                NakitOrani = 0.35m,
                NetIsletmeSermayesi = 300000
            },
            Karlilik = new KarlilikRasyolari
            {
                BrutKarMarji = 25,
                NetKarMarji = 10,
                FaaliyetKarMarji = 15,
                AktifKarliligi = 12,
                OzsermayeKarliligi = 20
            },
            Faaliyet = new FaaliyetRasyolari
            {
                StokDevirHizi = 6,
                VarlikDevirHizi = 1.2m,
                OzsermayeDevirHizi = 2
            },
            FinansalYapi = new FinansalYapiRasyolari
            {
                BorcOrani = 40,
                OzsermayeOrani = 60,
                FinansalKaldirac = 1.67m,
                FaizKarsilamaOrani = 9
            }
        };
    }

    private DuPontAnaliziResult CreateSampleDuPontResult(string firmaAdi, int yil)
    {
        return new DuPontAnaliziResult
        {
            Yil = yil,
            FirmaAdi = firmaAdi,
            NetKar = 120000,
            NetSatislar = 1200000,
            ToplamVarliklar = 1000000,
            Ozsermaye = 600000,
            NetKarMarji = 10,
            VarlikDevirHizi = 1.2m,
            FinansalKaldirac = 1.67m,
            ROA = 12,
            ROE = 20
        };
    }

    private SwotAnaliziResult CreateSampleSwotResult(string firmaAdi, int yil)
    {
        return new SwotAnaliziResult
        {
            Yil = yil,
            FirmaAdi = firmaAdi,
            GucluYonler = new List<SwotItem>
            {
                new SwotItem { Baslik = "Guclu Likidite", Aciklama = "Cari oran 2.5 ile rahat odeme kapasitesi", OnemDerecesi = 5 },
                new SwotItem { Baslik = "Yuksek Karlilik", Aciklama = "ROE %20 ile sektor ortalamasinin ustunde", OnemDerecesi = 5 },
                new SwotItem { Baslik = "Dusuk Borc Orani", Aciklama = "%40 borc orani ile saglikli finansal yapi", OnemDerecesi = 4 }
            },
            ZayifYonler = new List<SwotItem>
            {
                new SwotItem { Baslik = "Stok Yonetimi", Aciklama = "Stok devir hizi iyilestirilebilir", OnemDerecesi = 3 },
                new SwotItem { Baslik = "Pazarlama Giderleri", Aciklama = "Pazarlama yatirimlari yetersiz", OnemDerecesi = 2 }
            },
            Firsatlar = new List<SwotItem>
            {
                new SwotItem { Baslik = "Dijitallesme", Aciklama = "E-ticaret ve dijital kanallar ile buyume", OnemDerecesi = 5 },
                new SwotItem { Baslik = "Ihracat Potansiyeli", Aciklama = "Yeni pazarlara aciilma firsati", OnemDerecesi = 4 }
            },
            Tehditler = new List<SwotItem>
            {
                new SwotItem { Baslik = "Enflasyon", Aciklama = "Maliyet artislari kar marjini dusurabilir", OnemDerecesi = 4 },
                new SwotItem { Baslik = "Rekabet", Aciklama = "Sektorde artan rekabet baskisi", OnemDerecesi = 3 }
            }
        };
    }
    #endregion

    private void BtnExport_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("PDF export ozelligi yaklasimda...", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}