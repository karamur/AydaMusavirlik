using System.Net.Http;
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

        // Önce Login penceresi aç
        var loginWindow = new LoginWindow();
        loginWindow.Show();
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

        // Auth Token Store (Singleton - uygulamada tek instance)
        services.AddSingleton<AuthTokenStore>();

        // HttpClient
        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(apiSettings.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Services
        services.AddSingleton<IAuthService, AuthService>();
        services.AddTransient<ICompanyService, CompanyService>();
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<IPayrollService, PayrollService>();
        services.AddTransient<IAccountService, AccountService>();
    }

    public static T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }
}

