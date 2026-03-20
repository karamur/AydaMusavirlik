using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Net.Http;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Services;

namespace AydaMusavirlik.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly IAuthService _authService;
    private readonly ISettingsService _settingsService;

    public LoginWindow()
    {
        InitializeComponent();
        _authService = App.GetService<IAuthService>();
        _settingsService = App.GetService<ISettingsService>();

        Loaded += LoginWindow_Loaded;
    }

    private async void LoginWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await CheckApiStatus();
        await CheckDatabaseStatus();
        txtUsername.Focus();
    }

    private async Task CheckApiStatus()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            var response = await client.GetAsync("http://localhost:5000/swagger/index.html");
            
            if (response.IsSuccessStatusCode)
            {
                statusIndicator.Fill = new SolidColorBrush(Colors.Green);
                txtStatus.Text = "Bagli";
            }
            else
            {
                statusIndicator.Fill = new SolidColorBrush(Colors.Orange);
                txtStatus.Text = "Offline";
            }
        }
        catch
        {
            statusIndicator.Fill = new SolidColorBrush(Colors.Orange);
            txtStatus.Text = "Offline";
        }
    }

    private async Task CheckDatabaseStatus()
    {
        try
        {
            var dbSettings = _settingsService.Settings.Database;
            var info = await DatabaseFactory.GetDatabaseInfoAsync(dbSettings);

            if (info.IsConnected)
            {
                dbStatusIndicator.Fill = new SolidColorBrush(Colors.Green);
                txtDbStatus.Text = $"Veritabani bagli ({dbSettings.Provider})";
                txtDbInfo.Text = $"Kullanici: {info.CompanyCount}, Firma: {info.AccountCount} hesap";
                brdDbStatus.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)); // Acik yesil
            }
            else
            {
                dbStatusIndicator.Fill = new SolidColorBrush(Colors.Orange);
                txtDbStatus.Text = "Veritabani bagli degil";
                txtDbInfo.Text = "Yedek kullanicilar aktif";
                brdDbStatus.Background = new SolidColorBrush(Color.FromRgb(255, 243, 224)); // Acik turuncu
            }
        }
        catch (Exception ex)
        {
            dbStatusIndicator.Fill = new SolidColorBrush(Colors.Red);
            txtDbStatus.Text = "Veritabani hatasi";
            txtDbInfo.Text = ex.Message.Length > 50 ? ex.Message.Substring(0, 50) + "..." : ex.Message;
            brdDbStatus.Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)); // Acik kirmizi
        }
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        await DoLogin();
    }

    private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _ = DoLogin();
        }
    }

    private async Task DoLogin()
    {
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Kullanici adi ve sifre giriniz.");
            return;
        }

        btnLogin.IsEnabled = false;
        btnLogin.Content = "GIRIS YAPILIYOR...";
        txtError.Visibility = Visibility.Collapsed;

        try
        {
            var result = await _authService.LoginAsync(username, password);

            if (result.Success)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
            else
            {
                ShowError(result.Error ?? "Giris basarisiz.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Hata: {ex.Message}");
        }
        finally
        {
            btnLogin.IsEnabled = true;
            btnLogin.Content = "GIRIS YAP";
        }
    }

    private void ShowError(string message)
    {
        txtError.Text = message;
        txtError.Visibility = Visibility.Visible;
    }
}