using System.Windows;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using AydaMusavirlik.Desktop.Services;
using AydaMusavirlik.Desktop.Services.Reports;
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

        // API Services
        services.AddSingleton<IAuthService, AuthService>();
        services.AddTransient<ICompanyService, CompanyService>();
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<IPayrollService, PayrollService>();
        services.AddTransient<IAccountService, AccountService>();

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

