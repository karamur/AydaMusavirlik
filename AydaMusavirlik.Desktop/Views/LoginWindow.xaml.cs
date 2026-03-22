using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AydaMusavirlik.Desktop.Services;

namespace AydaMusavirlik.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly IAuthService _authService;
    private bool _isApiOnline = false;

    public LoginWindow()
    {
        InitializeComponent();
        _authService = App.GetService<IAuthService>();

        Loaded += LoginWindow_Loaded;
    }

    private async void LoginWindow_Loaded(object sender, RoutedEventArgs e)
    {
        txtUsername.Focus();
        await CheckApiStatus();
    }

    private async Task CheckApiStatus()
    {
        try
        {
            statusIndicator.Fill = new SolidColorBrush(Colors.Orange);
            txtStatus.Text = "API kontrol ediliyor...";

            using var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);

            var response = await client.GetAsync("http://localhost:5000/swagger/index.html");

            if (response.IsSuccessStatusCode)
            {
                _isApiOnline = true;
                statusIndicator.Fill = new SolidColorBrush(Colors.LimeGreen);
                txtStatus.Text = "API Bagli (Online)";
            }
            else
            {
                SetOfflineMode();
            }
        }
        catch
        {
            SetOfflineMode();
        }
    }

    private void SetOfflineMode()
    {
        _isApiOnline = false;
        statusIndicator.Fill = new SolidColorBrush(Colors.Gray);
        txtStatus.Text = "Offline Mod";
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
        }
    }

    private void ShowError(string message)
    {
        txtError.Text = message;
        txtError.Visibility = Visibility.Visible;
    }
}