
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configure services for forwarded headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // The following lines are important for proxy environments.
    options.KnownProxies.Clear();
    options.KnownNetworks.Clear();
});

// Configure QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

// --- Register Fonts for QuestPDF ---
var fontDirectory = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "fonts");
Action<string> RegisterFontIfExists = (fileName) =>
{
    var fontPath = Path.Combine(fontDirectory, fileName);
    if (File.Exists(fontPath))
    {
        using var stream = File.OpenRead(fontPath);
        FontManager.RegisterFont(stream);
    }
};
RegisterFontIfExists("Sarabun-Regular.ttf");
RegisterFontIfExists("Sarabun-Bold.ttf");
RegisterFontIfExists("Sarabun-Italic.ttf");
RegisterFontIfExists("Sarabun-BoldItalic.ttf");
// --- End Font Registration ---

// --- Add Culture Configuration --- 
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-GB") };
    options.DefaultRequestCulture = new RequestCulture("en-GB");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
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
        options.AccessDeniedPath = "/Account/AccessDenied";
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
app.UseForwardedHeaders(); // This must be one of the first middleware.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRequestLocalization();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
