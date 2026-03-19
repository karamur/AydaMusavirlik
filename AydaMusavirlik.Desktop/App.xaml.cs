using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AydaMusavirlik.Desktop.Services;

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
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Auth Token Store
        services.AddSingleton<AuthTokenStore>();
        
        // Auth Service (Offline)
        services.AddSingleton<IAuthService, AuthService>();
    }

    public static T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }
}

