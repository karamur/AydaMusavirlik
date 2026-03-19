using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Desktop.Views;
using AydaMusavirlik.Desktop.Views.Accounting;
using AydaMusavirlik.Desktop.Views.ArGe;
using AydaMusavirlik.Desktop.Views.Companies;
using AydaMusavirlik.Desktop.Views.Payroll;

namespace AydaMusavirlik.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IAuthService _authService;

    public MainWindow()
    {
        InitializeComponent();
        _authService = App.GetService<IAuthService>();
        
        txtTarih.Text = DateTime.Now.ToString("dd MMMM yyyy dddd");
        txtKullanici.Text = _authService.CurrentUserFullName ?? _authService.CurrentUser ?? "Kullanıcı";
        
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await CheckApiStatus();
    }

    private async Task CheckApiStatus()
    {
        try
        {
            using var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            var response = await client.GetAsync("http://localhost:5000/api/companies");
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.IsSuccessStatusCode)
            {
                apiStatusIndicator.Fill = new SolidColorBrush(Colors.LimeGreen);
                txtApiStatus.Text = "API Bağlı";
            }
            else
            {
                apiStatusIndicator.Fill = new SolidColorBrush(Colors.Orange);
                txtApiStatus.Text = "API Yanıt Yok";
            }
        }
        catch
        {
            apiStatusIndicator.Fill = new SolidColorBrush(Colors.Red);
            txtApiStatus.Text = "API Bağlantısı Yok";
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Çıkış yapmak istediğinize emin misiniz?", "Çıkış", 
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            _authService.Logout();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }

    #region Dosya Menüsü
    private void YeniFirma_Click(object sender, RoutedEventArgs e)
    {
        var editWindow = new CompanyEditWindow();
        editWindow.Owner = this;
        editWindow.ShowDialog();
    }

    private void FirmaSec_Click(object sender, RoutedEventArgs e)
    {
        var companyList = new CompanyListView();
        companyList.CompanySelected += (s, company) =>
        {
            txtAktifFirma.Text = company.Name;
            MessageBox.Show($"'{company.Name}' firması seçildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        };
        OpenTabWithControl("Firma Seç", companyList);
    }

    private void Yedekle_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Veritabanı yedekleme işlemi başlatılacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void GeriYukle_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Yedekten geri yükleme işlemi başlatılacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Cikis_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Programdan çıkmak istediğinize emin misiniz?", "Çıkış", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            Application.Current.Shutdown();
        }
    }
    #endregion

    #region Muhasebe Menüsü
    private void HesapPlani_Click(object sender, RoutedEventArgs e)
    {
        OpenTabWithControl("Hesap Planı", new AccountPlanView());
    }

    private void FisGirisi_Click(object sender, RoutedEventArgs e)
    {
        OpenTabWithControl("Fiş Girişi", new VoucherEntryView());
    }

    private void FisListesi_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Fiş Listesi", "📋 Fiş listesi burada görünecek.");
    }

    private void Mizan_Click(object sender, RoutedEventArgs e)
    {
        OpenTabWithControl("Mizan", new TrialBalanceView());
    }

    private void YevmiyeDefteri_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Yevmiye Defteri", "📖 Yevmiye defteri burada görünecek.");
    }

    private void KebirDefteri_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Kebir Defteri", "📖 Kebir defteri burada görünecek.");
    }

    private void EnvanterDefteri_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Envanter Defteri", "📖 Envanter defteri burada görünecek.");
    }

    private void DonemAcilisi_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Dönem açılış işlemi başlatılacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DonemKapanisi_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Dönem kapanış işlemi başlatılacak.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    #endregion

    #region Bordro Menüsü
    private void PersonelKartlari_Click(object sender, RoutedEventArgs e)
    {
        OpenTabWithControl("Personel Kartları", new EmployeeListView());
    }

    private void BordroHesaplama_Click(object sender, RoutedEventArgs e)
    {
        OpenTabWithControl("Bordro Hesaplama", new PayrollCalculationView());
    }

    private void BordroListesi_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Bordro Listesi", "📋 Bordro listesi burada görünecek.");
    }

    private void IzinTakibi_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("İzin Takibi", "📅 İzin takibi ekranı burada görünecek.");
    }

    private void SgkBildirgeleri_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("SGK Bildirgeleri", "📄 SGK bildirgeleri burada görünecek.");
    }

    private void MuhtasarBeyanname_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Muhtasar Beyanname", "📄 Muhtasar beyanname burada görünecek.");
    }
    #endregion

    #region AR-GE Menüsü
    private void ArgeProjeTanimlari_Click(object sender, RoutedEventArgs e)
    {
        OpenTabWithControl("AR-GE Projeleri", new ArGeProjectListView());
    }

    private void ArgePersoneli_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("AR-GE Personeli", "👨‍🔬 AR-GE personel listesi burada görünecek.");
    }

    private void ArgeGiderTakibi_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("AR-GE Gider Takibi", "💵 AR-GE gider takibi burada görünecek.");
    }

    private void ArgeProjeBazliRapor_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Proje Bazlı Rapor", "📊 Proje bazlı rapor burada görünecek.");
    }

    private void ArgePersonelBazliRapor_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Personel Bazlı Rapor", "📊 Personel bazlı rapor burada görünecek.");
    }

    private void ArgeTesvikHesaplama_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Teşvik Hesaplama", "🧮 AR-GE teşvik hesaplama burada görünecek.");
    }
    #endregion

    #region Şirket Kuruluş Menüsü
    private void SirketKurulusYeniBaşvuru_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Yeni Şirket Başvurusu", "🏢 Şirket kuruluş başvuru formu burada görünecek.");
    }

    private void SirketKurulusBasvuruListesi_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Başvuru Listesi", "📋 Şirket kuruluş başvuru listesi burada görünecek.");
    }

    private void AnaSozlesmeHazirla_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Ana Sözleşme", "📜 Ana sözleşme hazırlama ekranı burada görünecek.");
    }

    private void EvrakTakibi_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Evrak Takibi", "📁 Evrak takibi ekranı burada görünecek.");
    }
    #endregion

    #region Denetim Menüsü
    private void DenetimRaporlari_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Denetim Raporları", "✅ Denetim raporları burada görünecek.");
    }

    private void FinansalAnaliz_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Finansal Analiz", "📈 Finansal analiz ekranı burada görünecek.");
    }

    private void OranAnalizi_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Oran Analizi", "📊 Oran analizi ekranı burada görünecek.");
    }
    #endregion

    #region Raporlar Menüsü
    private void Bilanco_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Bilanço", "📈 Bilanço raporu burada görünecek.");
    }

    private void GelirTablosu_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Gelir Tablosu", "📊 Gelir tablosu burada görünecek.");
    }

    private void NakitAkisTablosu_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Nakit Akış Tablosu", "💵 Nakit akış tablosu burada görünecek.");
    }

    private void KdvRaporu_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("KDV Raporu", "📋 KDV raporu burada görünecek.");
    }

    private void EDefter_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("E-Defter", "📚 E-Defter oluşturma ekranı burada görünecek.");
    }
    #endregion

    #region Ayarlar Menüsü
    private void KullaniciAyarlari_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Kullanıcı Ayarları", "⚙️ Kullanıcı ayarları burada görünecek.");
    }

    private void SistemAyarlari_Click(object sender, RoutedEventArgs e)
    {
        OpenTab("Sistem Ayarları", "🔧 Sistem ayarları burada görünecek.");
    }

    private void Hakkinda_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Ayda Müşavirlik\nMuhasebe ve Danışmanlık Yönetim Sistemi\n\nVersiyon: 1.0.0\n\n© 2024 Tüm hakları saklıdır.",
            "Hakkında",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    #endregion

    #region Tab Yönetimi
    private void OpenTab(string header, string content)
    {
        // Aynı tab açık mı kontrol et
        foreach (TabItem existingTab in tabControl.Items)
        {
            if (existingTab.Header.ToString() == header)
            {
                tabControl.SelectedItem = existingTab;
                return;
            }
        }

        // Yeni tab oluştur
        var tab = new TabItem
        {
            Header = header,
            Content = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(37, 37, 38)),
                Child = new TextBlock
                {
                    Text = content,
                    FontSize = 24,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };

        tabControl.Items.Add(tab);
        tabControl.SelectedItem = tab;
        pnlWelcome.Visibility = Visibility.Collapsed;
    }

    private void OpenTabWithControl(string header, UserControl control)
    {
        // Aynı tab açık mı kontrol et
        foreach (TabItem existingTab in tabControl.Items)
        {
            if (existingTab.Header.ToString() == header)
            {
                tabControl.SelectedItem = existingTab;
                return;
            }
        }

        // Yeni tab oluştur
        var tab = new TabItem
        {
            Header = header,
            Content = control
        };

        tabControl.Items.Add(tab);
        tabControl.SelectedItem = tab;
        pnlWelcome.Visibility = Visibility.Collapsed;
    }

    private void TabClose_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var tabItem = button?.Tag as TabItem;
        
        if (tabItem != null)
        {
            tabControl.Items.Remove(tabItem);
            
            if (tabControl.Items.Count == 0)
            {
                pnlWelcome.Visibility = Visibility.Visible;
            }
        }
    }
    #endregion
}