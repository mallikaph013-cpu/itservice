using myapp.Data;
using myapp.Models;
using System.Security.Claims;

namespace myapp.Services
{
    public class ActivityLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivityLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActivityAsync(string action, string details)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;

            var activityLog = new ActivityLog
            {
                Timestamp = DateTime.UtcNow,
                UserName = user?.Identity?.IsAuthenticated == true ? user.FindFirstValue(ClaimTypes.Name) : "Anonymous",
                Action = action,
                Details = details,
                IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString()
            };

            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();
        }
    }
}
