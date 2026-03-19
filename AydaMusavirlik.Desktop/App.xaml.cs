using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Desktop.Views;

namespace AydaMusavirlik.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Login penceresi aç
        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Auth Token Store (Singleton - uygulamada tek instance)
        services.AddSingleton<AuthTokenStore>();
        
        // Auth Service (Offline çalışır)
        services.AddSingleton<IAuthService, AuthService>();
    }

    public static T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }
}

