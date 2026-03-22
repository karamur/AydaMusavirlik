using AydaMusavirlik.Application.Common.Interfaces;

namespace AydaMusavirlik.Infrastructure.Services;

/// <summary>
/// DateTime Service Implementation
/// </summary>
public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}

/// <summary>
/// Current User Service Implementation
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public bool IsAuthenticated => UserId.HasValue;

    public void SetUser(int userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }

    public void ClearUser()
    {
        UserId = null;
        UserName = null;
    }
}