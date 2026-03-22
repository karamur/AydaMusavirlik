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
        
        // Pencereyi sürüklenebilir yap
        MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
    }

    private async void LoginWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await CheckDatabaseStatus();
        txtUsername.Focus();
    }

    private async Task CheckDatabaseStatus()
    {
        try
        {
            var dbSettings = _settingsService.Settings.Database;
            var info = await DatabaseFactory.GetDatabaseInfoAsync(dbSettings);

            if (info.IsConnected)
            {
                dbStatusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                txtDbStatus.Text = $"Veritabaný baðlý ({dbSettings.Provider})";
                txtDbInfo.Text = $"{info.CompanyCount} kullanýcý, {info.AccountCount} hesap";
                brdDbStatus.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
            }
            else
            {
                dbStatusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                txtDbStatus.Text = "Veritabaný baðlý deðil";
                txtDbInfo.Text = "Demo mod aktif";
                brdDbStatus.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
            }
        }
        catch (Exception ex)
        {
            dbStatusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
            txtDbStatus.Text = "Veritabaný hatasý";
            txtDbInfo.Text = ex.Message.Length > 40 ? ex.Message.Substring(0, 40) + "..." : ex.Message;
            brdDbStatus.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));
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

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private async Task DoLogin()
    {
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Kullanýcý adý ve þifre giriniz.");
            return;
        }

        btnLogin.IsEnabled = false;
        btnLogin.Content = "GÝRÝÞ YAPILIYOR...";
        HideError();

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
                ShowError(result.Error ?? "Giriþ baþarýsýz. Kullanýcý adý veya þifre hatalý.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Baðlantý hatasý: {ex.Message}");
        }
        finally
        {
            btnLogin.IsEnabled = true;
            btnLogin.Content = "GÝRÝÞ YAP";
        }
    }

    private void ShowError(string message)
    {
        txtError.Text = message;
        brdError.Visibility = Visibility.Visible;
    }

    private void HideError()
    {
        brdError.Visibility = Visibility.Collapsed;
    }
}