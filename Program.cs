using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Services;
using myapp.Models;

var builder = WebApplication.CreateBuilder(args);

// Force the app to run on port 8898 to avoid all conflicts
builder.WebHost.UseUrls("http://localhost:8898");

// Add services to the container.
var mvcBuilder = builder.Services.AddControllersWithViews();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ActivityLogService>();
builder.Services.AddScoped<NavigationService>();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Authentication and Authorization services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied"; // Also specify the access denied path
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsITSupport", policy => policy.RequireRole("ITSupport"));

    options.AddPolicy("CanAccessWorkQueue", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            context.User.IsInRole("ITSupport") ||
            context.User.HasClaim(c => c.Type == "IsDxStaff" && c.Value == "True")));

    options.AddPolicy("IsApprover", policy =>
        policy.RequireClaim("CanApprove", "True"));
});

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // This will apply pending migrations and create the database if it doesn't exist.
        // It ensures data is preserved between application restarts.
        context.Database.Migrate();
        await SeedData.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Set the default route to the Login page.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
