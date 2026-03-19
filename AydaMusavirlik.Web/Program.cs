using AydaMusavirlik.Components;
using AydaMusavirlik.Services;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/ayda-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MudBlazor
builder.Services.AddMudServices();

// Application Services
builder.Services.AddSingleton<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<CompanyService>();
builder.Services.AddSingleton<AccountingService>();
builder.Services.AddSingleton<FinancialAnalysisService>();
builder.Services.AddSingleton<AppointmentService>();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
