using System.Windows;
using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Seed;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Desktop.ViewModels;
using AydaMusavirlik.Desktop.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AydaMusavirlik.Desktop;

public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Database initialization
        await InitializeDatabaseAsync();

        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Database - PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql("Host=localhost;Database=AydaMusavirlik;Username=postgres;Password=postgres"),
            ServiceLifetime.Transient);

        // Services
        services.AddSingleton<AuthTokenStore>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<CompaniesViewModel>();
        services.AddTransient<AccountingViewModel>();
        services.AddTransient<PayrollViewModel>();
        services.AddTransient<ArGeViewModel>();
        services.AddTransient<CompanyFormationViewModel>();
        services.AddTransient<FinancialAnalysisViewModel>();
        services.AddTransient<AuditViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Create database if not exists
            await context.Database.EnsureCreatedAsync();

            // Seed initial data
            await DataSeeder.SeedAsync(context);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Veritabani baglantisi kurulamadi:\n{ex.Message}\n\nPostgreSQL calistigindan emin olun.",
                "Veritabani Hatasi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }
}

