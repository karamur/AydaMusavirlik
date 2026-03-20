using System.Windows;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Desktop.Services.Reports;
using AydaMusavirlik.Desktop.Views;
using AydaMusavirlik.Data.Services;

namespace AydaMusavirlik.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        await ConfigureServicesAsync(services);
        Services = services.BuildServiceProvider();

        // Login penceresi aç
        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }

    private async Task ConfigureServicesAsync(IServiceCollection services)
    {
        // Settings Service - Singleton
        var settingsService = new SettingsService();
        await settingsService.LoadAsync();
        services.AddSingleton<ISettingsService>(settingsService);

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
        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(apiSettings.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // API Services - AuthService with SettingsService
        services.AddSingleton<IAuthService>(sp => new AuthService(
            sp.GetRequiredService<AuthTokenStore>(),
            sp.GetRequiredService<ISettingsService>(),
            sp.GetService<ApiClient>()
        ));
        
        // Company Service with SettingsService
        services.AddTransient<ICompanyService>(sp => new CompanyService(
            sp.GetRequiredService<ISettingsService>(),
            sp.GetService<ApiClient>()
        ));
        
        // Account Service with SettingsService
        services.AddTransient<IAccountService>(sp => new AccountService(
            sp.GetRequiredService<ISettingsService>(),
            sp.GetService<ApiClient>()
        ));
        
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<IPayrollService, PayrollService>();

        // Report Services
        services.AddTransient<IFinancialAnalysisService, FinancialAnalysisService>();
        services.AddTransient<IReportGeneratorService, ReportGeneratorService>();
        services.AddTransient<IExcelExportService, ExcelExportService>();
        services.AddTransient<IPdfExportService, PdfExportService>();
    }

    public static T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }
}

