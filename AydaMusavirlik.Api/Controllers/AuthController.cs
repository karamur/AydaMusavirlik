using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AydaMusavirlik.Data;
using Microsoft.EntityFrameworkCore;

namespace AydaMusavirlik.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Basarisiz giris denemesi: {Username}", request.Username);
            return Unauthorized(new { message = "Kullanici adi veya sifre hatali" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Role.ToString());

        _logger.LogInformation("Kullanici giris yapti: {Username}", request.Username);

        return Ok(new LoginResponse
        {
            Token = token,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetExpireMinutes())
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest(new { message = "Bu kullanici adi zaten kullaniliyor" });

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "Bu e-posta adresi zaten kullaniliyor" });

        var user = new Core.Models.Common.User
        {
            Username = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = HashPassword(request.Password),
            Role = Core.Models.Common.UserRole.User,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Yeni kullanici olusturuldu: {Username}", request.Username);

        return Ok(new { message = "Kullanici basariyla olusturuldu" });
    }

    [HttpPost("seed")]
    public async Task<ActionResult> SeedAdminUser()
    {
        if (await _context.Users.AnyAsync())
            return BadRequest(new { message = "Veritabaninda zaten kullanici mevcut" });

        var adminUser = new Core.Models.Common.User
        {
            Username = "admin",
            Email = "admin@aydamusavirlik.com",
            FullName = "Sistem Yoneticisi",
            PasswordHash = HashPassword("admin123"),
            Role = Core.Models.Common.UserRole.Admin,
            IsActive = true
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Admin kullanicisi olusturuldu. Kullanici: admin, Sifre: admin123" });
    }

    private string GenerateJwtToken(int userId, string username, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "AydaMusavirlikSecretKey2024!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "AydaMusavirlik",
            audience: _configuration["Jwt:Issuer"] ?? "AydaMusavirlik",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetExpireMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetExpireMinutes()
    {
        return int.TryParse(_configuration["Jwt:ExpireMinutes"], out var minutes) ? minutes : 480;
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}