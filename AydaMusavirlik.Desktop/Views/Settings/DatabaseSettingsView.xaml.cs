using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using AydaMusavirlik.Core.Configuration;
using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Services;

namespace AydaMusavirlik.Desktop.Views.Settings;

public partial class DatabaseSettingsView : UserControl
{
    private readonly ISettingsService _settingsService;

    public DatabaseSettingsView()
    {
        InitializeComponent();

        _settingsService = App.GetService<ISettingsService>();

        Loaded += DatabaseSettingsView_Loaded;
    }

    private async void DatabaseSettingsView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadCurrentSettings();
    }

    private async Task LoadCurrentSettings()
    {
        var settings = _settingsService.Settings.Database;

        // Veritabani turunu sec
        switch (settings.Provider)
        {
            case DatabaseProvider.SQLite:
                rbSqlite.IsChecked = true;
                txtSqlitePath.Text = settings.SqliteFilePath;
                break;
            case DatabaseProvider.SqlServer:
                rbSqlServer.IsChecked = true;
                txtSqlServerHost.Text = settings.SqlServerHost;
                txtSqlServerPort.Text = settings.SqlServerPort.ToString();
                txtSqlServerDb.Text = settings.SqlServerDatabase;
                chkTrustedConnection.IsChecked = settings.SqlServerTrustedConnection;
                txtSqlServerUser.Text = settings.SqlServerUsername;
                break;
            case DatabaseProvider.PostgreSQL:
                rbPostgres.IsChecked = true;
                txtPostgresHost.Text = settings.PostgresHost;
                txtPostgresPort.Text = settings.PostgresPort.ToString();
                txtPostgresDb.Text = settings.PostgresDatabase;
                txtPostgresUser.Text = settings.PostgresUsername;
                break;
        }

        // Baglanti durumunu guncelle
        await RefreshConnectionStatus();
    }

    private void DatabaseType_Changed(object sender, RoutedEventArgs e)
    {
        pnlSqlite.Visibility = rbSqlite.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        pnlSqlServer.Visibility = rbSqlServer.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        pnlPostgres.Visibility = rbPostgres.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
    }

    private void TrustedConnection_Changed(object sender, RoutedEventArgs e)
    {
        pnlSqlServerAuth.Visibility = chkTrustedConnection.IsChecked == true 
            ? Visibility.Collapsed 
            : Visibility.Visible;
    }

    private void BrowseSqlite_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "SQLite Veritabani|*.db|Tum Dosyalar|*.*",
            FileName = txtSqlitePath.Text,
            Title = "Veritabani Dosyasi Sec"
        };

        if (dialog.ShowDialog() == true)
        {
            txtSqlitePath.Text = dialog.FileName;
        }
    }

    private DatabaseSettings GetCurrentSettings()
    {
        var settings = new DatabaseSettings();

        if (rbSqlite.IsChecked == true)
        {
            settings.Provider = DatabaseProvider.SQLite;
            settings.SqliteFilePath = txtSqlitePath.Text;
        }
        else if (rbSqlServer.IsChecked == true)
        {
            settings.Provider = DatabaseProvider.SqlServer;
            settings.SqlServerHost = txtSqlServerHost.Text;
            settings.SqlServerPort = int.TryParse(txtSqlServerPort.Text, out var port) ? port : 1433;
            settings.SqlServerDatabase = txtSqlServerDb.Text;
            settings.SqlServerTrustedConnection = chkTrustedConnection.IsChecked == true;
            settings.SqlServerUsername = txtSqlServerUser.Text;
            settings.SqlServerPassword = txtSqlServerPass.Password;
        }
        else if (rbPostgres.IsChecked == true)
        {
            settings.Provider = DatabaseProvider.PostgreSQL;
            settings.PostgresHost = txtPostgresHost.Text;
            settings.PostgresPort = int.TryParse(txtPostgresPort.Text, out var port) ? port : 5432;
            settings.PostgresDatabase = txtPostgresDb.Text;
            settings.PostgresUsername = txtPostgresUser.Text;
            settings.PostgresPassword = txtPostgresPass.Password;
        }

        return settings;
    }

    private async void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        btnTestConnection.IsEnabled = false;
        btnTestConnection.Content = "Test ediliyor...";
        ShowMessage("Baglanti test ediliyor...", false);

        try
        {
            var settings = GetCurrentSettings();
            var (success, message) = await DatabaseFactory.TestConnectionAsync(settings);

            if (success)
            {
                ShowMessage("Baglanti basarili!", false);
                await RefreshConnectionStatus(settings);
            }
            else
            {
                ShowMessage($"Baglanti hatasi: {message}", true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Hata: {ex.Message}", true);
        }
        finally
        {
            btnTestConnection.IsEnabled = true;
            btnTestConnection.Content = "Baglanti Test Et";
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        btnSave.IsEnabled = false;
        btnSave.Content = "Kaydediliyor...";

        try
        {
            var settings = GetCurrentSettings();
            var success = await _settingsService.UpdateDatabaseSettingsAsync(settings);

            if (success)
            {
                ShowMessage("Ayarlar kaydedildi ve veritabanina baglandi!", false);
                await RefreshConnectionStatus(settings);

                MessageBox.Show("Veritabani ayarlari basariyla kaydedildi.", "Basarili", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ShowMessage("Ayarlar kaydedilemedi. Baglanti bilgilerini kontrol edin.", true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Hata: {ex.Message}", true);
        }
        finally
        {
            btnSave.IsEnabled = true;
            btnSave.Content = "Kaydet ve Baglan";
        }
    }

    private async void CreateDatabase_Click(object sender, RoutedEventArgs e)
    {
        btnCreateDb.IsEnabled = false;
        btnCreateDb.Content = "Olusturuluyor...";

        try
        {
            var settings = GetCurrentSettings();
            var (success, message) = await DatabaseFactory.InitializeDatabaseAsync(settings, seedData: false);

            if (success)
            {
                ShowMessage(message, false);
                await RefreshConnectionStatus(settings);
                
                MessageBox.Show("Veritabani basariyla olusturuldu.", "Basarili", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ShowMessage(message, true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Hata: {ex.Message}", true);
        }
        finally
        {
            btnCreateDb.IsEnabled = true;
            btnCreateDb.Content = "Veritabani Olustur";
        }
    }

    private async void SeedData_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Ornek veriler yuklenecek. Bu islem mevcut verileri etkilemez.\n\nDevam etmek istiyor musunuz?",
            "Ornek Veri Yukleme",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        btnSeedData.IsEnabled = false;
        btnSeedData.Content = "Yukleniyor...";

        try
        {
            var settings = GetCurrentSettings();
            var (success, message) = await DatabaseFactory.InitializeDatabaseAsync(settings, seedData: true);

            if (success)
            {
                ShowMessage("Ornek veriler basariyla yuklendi!", false);
                await RefreshConnectionStatus(settings);
                
                MessageBox.Show(
                    "Ornek veriler basariyla yuklendi!\n\n" +
                    "Varsayilan Kullanicilar:\n" +
                    "- admin / Admin123!\n" +
                    "- muhasebeci / Muhasebe123!\n" +
                    "- kullanici / Kullanici123!",
                    "Basarili", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ShowMessage(message, true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Hata: {ex.Message}", true);
        }
        finally
        {
            btnSeedData.IsEnabled = true;
            btnSeedData.Content = "Ornek Veri Yukle";
        }
    }

    private async Task RefreshConnectionStatus(DatabaseSettings? settings = null)
    {
        settings ??= _settingsService.Settings.Database;

        try
        {
            var info = await DatabaseFactory.GetDatabaseInfoAsync(settings);

            txtDbProvider.Text = info.Provider;
            txtConnectionString.Text = info.ConnectionString;

            if (info.IsConnected)
            {
                txtStatus.Text = "Bagli";
                txtStatus.Foreground = new SolidColorBrush(Colors.LightGreen);
                txtCompanyCount.Text = info.CompanyCount.ToString();
                txtAccountCount.Text = info.AccountCount.ToString();
                txtRecordCount.Text = info.RecordCount.ToString();
            }
            else
            {
                txtStatus.Text = "Bagli Degil";
                txtStatus.Foreground = new SolidColorBrush(Colors.Orange);
                txtCompanyCount.Text = "-";
                txtAccountCount.Text = "-";
                txtRecordCount.Text = "-";

                if (!string.IsNullOrEmpty(info.ErrorMessage))
                {
                    ShowMessage(info.ErrorMessage, true);
                }
            }
        }
        catch (Exception ex)
        {
            txtStatus.Text = "Hata";
            txtStatus.Foreground = new SolidColorBrush(Colors.Red);
            ShowMessage(ex.Message, true);
        }
    }

    private void ShowMessage(string message, bool isError)
    {
        brdMessage.Visibility = Visibility.Visible;
        brdMessage.Background = isError 
            ? new SolidColorBrush(Color.FromRgb(139, 0, 0)) 
            : new SolidColorBrush(Color.FromRgb(45, 90, 39));
        txtMessage.Text = message;
    }
}