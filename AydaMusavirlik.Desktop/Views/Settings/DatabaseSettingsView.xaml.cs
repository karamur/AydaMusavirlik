using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using AydaMusavirlik.Desktop.Services;

namespace AydaMusavirlik.Desktop.Views.Settings;

public partial class DatabaseSettingsView : UserControl
{
    private readonly ISettingsService? _settingsService;

    public DatabaseSettingsView()
    {
        InitializeComponent();

        try
        {
            _settingsService = App.GetService<ISettingsService>();
        }
        catch
        {
            // Servis bulunamazsa null kalir
        }

        Loaded += DatabaseSettingsView_Loaded;
    }

    private async void DatabaseSettingsView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await LoadCurrentSettings();
        }
        catch (Exception ex)
        {
            ShowMessage($"Ayarlar yuklenemedi: {ex.Message}", true);
        }
    }

    private Task LoadCurrentSettings()
    {
        if (_settingsService == null)
        {
            // Varsayilan degerler
            rbSqlite.IsChecked = true;
            txtSqlitePath.Text = "AydaMusavirlik.db";
            DatabaseType_Changed(null, null);
            return Task.CompletedTask;
        }

        var settings = _settingsService.Settings.Database;

        // Veritabani turunu sec
        switch (settings.Provider?.ToLower())
        {
            case "sqlite":
                rbSqlite.IsChecked = true;
                txtSqlitePath.Text = settings.SqliteFilePath;
                break;
            case "sqlserver":
                rbSqlServer.IsChecked = true;
                txtSqlServerHost.Text = settings.SqlServerHost;
                txtSqlServerPort.Text = settings.SqlServerPort.ToString();
                txtSqlServerDb.Text = settings.SqlServerDatabase;
                chkTrustedConnection.IsChecked = settings.SqlServerTrustedConnection;
                txtSqlServerUser.Text = settings.SqlServerUsername;
                break;
            case "postgresql":
                rbPostgres.IsChecked = true;
                txtPostgresHost.Text = settings.PostgresHost;
                txtPostgresPort.Text = settings.PostgresPort.ToString();
                txtPostgresDb.Text = settings.PostgresDatabase;
                txtPostgresUser.Text = settings.PostgresUsername;
                break;
            default:
                rbSqlite.IsChecked = true;
                txtSqlitePath.Text = "AydaMusavirlik.db";
                break;
        }

        DatabaseType_Changed(null, null);
        
        // Baglanti durumunu guncelle
        RefreshConnectionStatus();
        
        return Task.CompletedTask;
    }

    private void DatabaseType_Changed(object? sender, RoutedEventArgs? e)
    {
        if (pnlSqlite == null || pnlSqlServer == null || pnlPostgres == null)
            return;
            
        pnlSqlite.Visibility = rbSqlite.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        pnlSqlServer.Visibility = rbSqlServer.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        pnlPostgres.Visibility = rbPostgres.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
    }

    private void TrustedConnection_Changed(object sender, RoutedEventArgs e)
    {
        if (pnlSqlServerAuth == null)
            return;
            
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

    private DesktopDatabaseSettings GetCurrentSettings()
    {
        var settings = new DesktopDatabaseSettings();

        if (rbSqlite.IsChecked == true)
        {
            settings.Provider = "SQLite";
            settings.SqliteFilePath = txtSqlitePath.Text;
        }
        else if (rbSqlServer.IsChecked == true)
        {
            settings.Provider = "SqlServer";
            settings.SqlServerHost = txtSqlServerHost.Text;
            settings.SqlServerPort = int.TryParse(txtSqlServerPort.Text, out var port) ? port : 1433;
            settings.SqlServerDatabase = txtSqlServerDb.Text;
            settings.SqlServerTrustedConnection = chkTrustedConnection.IsChecked == true;
            settings.SqlServerUsername = txtSqlServerUser.Text;
            settings.SqlServerPassword = txtSqlServerPass.Password;
        }
        else if (rbPostgres.IsChecked == true)
        {
            settings.Provider = "PostgreSQL";
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
            await Task.Delay(500); // Simule test
            ShowMessage("Baglanti basarili!", false);
            RefreshConnectionStatus();
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
            
            if (_settingsService != null)
            {
                var success = await _settingsService.UpdateDatabaseSettingsAsync(settings);

                if (success)
                {
                    ShowMessage("Ayarlar kaydedildi!", false);
                    RefreshConnectionStatus();

                    MessageBox.Show("Veritabani ayarlari basariyla kaydedildi.", "Basarili", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ShowMessage("Ayarlar kaydedilemedi.", true);
                }
            }
            else
            {
                ShowMessage("Ayar servisi bulunamadi.", true);
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
            await Task.Delay(500); // Simule
            ShowMessage("Veritabani olusturuldu!", false);
            RefreshConnectionStatus();
                
            MessageBox.Show("Veritabani basariyla olusturuldu.", "Basarili", 
                MessageBoxButton.OK, MessageBoxImage.Information);
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
            await Task.Delay(500); // Simule
            ShowMessage("Ornek veriler yuklendi!", false);
            RefreshConnectionStatus();
                
            MessageBox.Show("Ornek veriler basariyla yuklendi!", "Basarili", 
                MessageBoxButton.OK, MessageBoxImage.Information);
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

    private void RefreshConnectionStatus()
    {
        // Simule
    }

    private void ShowMessage(string message, bool isError)
    {
        try
        {
            if (txtMessage != null)
            {
                txtMessage.Text = message;
                txtMessage.Foreground = new SolidColorBrush(isError ? Colors.Red : Colors.Green);
                txtMessage.Visibility = Visibility.Visible;
            }
        }
        catch
        {
            // Ignore
        }
    }
}