namespace AydaMusavirlik.Application.Common.Interfaces;

/// <summary>
/// Kullanici Kimlik Dogrulama Servisi
/// </summary>
public interface IIdentityService
{
    Task<string?> GetUserNameAsync(int userId);
    Task<bool> IsInRoleAsync(int userId, string role);
    Task<bool> AuthorizeAsync(int userId, string policyName);
    Task<(bool Succeeded, int UserId)> CreateUserAsync(string userName, string password);
    Task<bool> DeleteUserAsync(int userId);
}

/// <summary>
/// Mevcut Kullanici Bilgisi
/// </summary>
public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}

/// <summary>
/// Tarih/Zaman Servisi
/// </summary>
public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}

/// <summary>
/// Email Gonderim Servisi
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true);
}

/// <summary>
/// Dosya Depolama Servisi
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder);
    Task<Stream?> GetFileAsync(string filePath);
    Task DeleteFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
}

/// <summary>
/// PDF Olusturma Servisi
/// </summary>
public interface IPdfGeneratorService
{
    Task<byte[]> GeneratePdfAsync<T>(T model, string templateName);
    Task<byte[]> GenerateReportAsync(string reportType, object data);
}

/// <summary>
/// Excel Olusturma Servisi
/// </summary>
public interface IExcelGeneratorService
{
    Task<byte[]> GenerateExcelAsync<T>(IEnumerable<T> data, string sheetName);
    Task<byte[]> GenerateReportAsync(string reportType, object data);
}