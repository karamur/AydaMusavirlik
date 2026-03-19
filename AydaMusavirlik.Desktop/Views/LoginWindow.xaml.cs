using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AydaMusavirlik.Desktop.Services;

namespace AydaMusavirlik.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly IAuthService _authService;

    public LoginWindow()
    {
        InitializeComponent();
        _authService = App.GetService<IAuthService>();

        Loaded += LoginWindow_Loaded;
    }

    private async void LoginWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await CheckApiStatus();
    }

    private async Task CheckApiStatus()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync("http://localhost:5000/swagger/index.html");

            if (response.IsSuccessStatusCode)
            {
                statusIndicator.Fill = new SolidColorBrush(Colors.Green);
                txtStatus.Text = "Baðlý";
            }
            else
            {
                statusIndicator.Fill = new SolidColorBrush(Colors.Orange);
                txtStatus.Text = "Yanýt alýnamadý";
            }
        }
        catch
        {
            statusIndicator.Fill = new SolidColorBrush(Colors.Red);
            txtStatus.Text = "Baðlantý yok";
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
            ShowError("Kullanýcý adý ve þifre giriniz.");
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
                ShowError(result.Error ?? "Giriþ baþarýsýz.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Baðlantý hatasý: {ex.Message}");
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