using AydaMusavirlik.Data;
using AydaMusavirlik.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Environment variables support
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();

// PostgreSQL Database - Environment variable ile þifre desteði
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres";
connectionString = connectionString?.Replace("${DB_PASSWORD}", dbPassword);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(30);
    }));

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountingRecordRepository, AccountingRecordRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IPayrollRecordRepository, PayrollRecordRepository>();
builder.Services.AddScoped<IArGeProjectRepository, ArGeProjectRepository>();

// JWT Authentication - Environment variable ile secret key desteði
var jwtKey = builder.Configuration["Jwt:Key"] ?? "AydaMusavirlikSecretKey2024!";
var jwtSecretFromEnv = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
if (!string.IsNullOrEmpty(jwtSecretFromEnv))
    jwtKey = jwtSecretFromEnv;
else
    jwtKey = jwtKey.Replace("${JWT_SECRET_KEY}", "AydaMusavirlikSecretKey2024SuperSecure!");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AydaMusavirlik";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Ayda Musavirlik API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto migrate database (with error handling)
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Veritabani baglantisi kontrol ediliyor...");
    
    if (await db.Database.CanConnectAsync())
    {
        logger.LogInformation("Veritabani baglantisi basarili. Migration uygulaniyor...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Migration tamamlandi.");
    }
    else
    {
        logger.LogWarning("Veritabani baglantisi kurulamadi. Migration atlanýyor.");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Veritabani migration sirasinda hata olustu. Uygulama devam ediyor.");
}

app.Run();
