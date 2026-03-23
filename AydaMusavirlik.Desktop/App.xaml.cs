using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AydaMusavirlik.Desktop.Services;

namespace AydaMusavirlik.Desktop;

public partial class App : System.Windows.Application
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
        // API Settings
        var apiSettings = new ApiSettings
        {
            BaseUrl = "http://localhost:5000",
            TimeoutSeconds = 30
        };
        services.AddSingleton(apiSettings);

        // Auth Token Store
        services.AddSingleton<AuthTokenStore>();

        // HttpClient for API
        services.AddHttpClient<ApiClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<ApiSettings>();
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Services
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddTransient<ICompanyService, CompanyService>();
        services.AddTransient<IEmployeeService, EmployeeService>();
    }

    public static T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }
}

