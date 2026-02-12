using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using System.IO;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Support hosting under an IIS virtual directory (e.g. /itservice)
// Read from configuration `PATH_BASE` or the environment variable `ASPNETCORE_APPL_PATH`.
var pathBase = builder.Configuration["PATH_BASE"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");

// Configure Kestrel to use dynamic ports
//builder.WebHost.UseUrls("http://127.0.0.1:0", "https://127.0.0.1:0");

// 1. [แก้ไข] ตั้งค่า Forwarded Headers ให้ยอมรับค่าจาก Cloud Proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // สำคัญ: บน Cloud Workstations ตัว Proxy IP จะเปลี่ยนตลอดเวลา 
    // การ Clear สองค่านี้จะช่วยให้แอปเชื่อถือ Header ที่ส่งมาจาก Proxy ของระบบ Cloud
    options.KnownProxies.Clear();
    options.KnownNetworks.Clear();
});

// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

// 2. Font Registration
var fontDirectory = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "fonts");
void RegisterFontIfExists(string fileName)
{
    var fontPath = Path.Combine(fontDirectory, fileName);
    if (File.Exists(fontPath))
    {
        using var stream = File.OpenRead(fontPath);
        FontManager.RegisterFont(stream);
    }
}
RegisterFontIfExists("Sarabun-Regular.ttf");
RegisterFontIfExists("Sarabun-Bold.ttf");
RegisterFontIfExists("Sarabun-Italic.ttf");
RegisterFontIfExists("Sarabun-BoldItalic.ttf");

// Culture Configuration
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-GB") };
    options.DefaultRequestCulture = new RequestCulture("en-GB");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ActivityLogService>();
builder.Services.AddScoped<NavigationService>();

var provider = builder.Configuration.GetValue("DatabaseProvider", "SQLite");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (provider == "SQLServer")
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"),
            x => x.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
    }
    else
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("SQLite"),
            x => x.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
    }
});

// 3. [แก้ไข] ปรับปรุง Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        
        // บน Cloud ที่เป็น HTTPS แต่แอปทำงานเป็น HTTP ข้างหลัง 
        // ต้องตั้งค่า SecurePolicy เป็น SameAsRequest หรือใส่เงื่อนไข
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = "MyAppAuthCookie";
        // If the app is hosted under a virtual directory, ensure the cookie path matches.
        if (!string.IsNullOrEmpty(pathBase))
        {
            options.Cookie.Path = pathBase;
        }
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

// Seed database
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

// --- PIPELINE CONFIGURATION ---

// 4. [สำคัญมาก] ย้าย UseForwardedHeaders มาไว้บนสุด
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // หมายเหตุ: ห้ามใช้ app.UseHttpsRedirection() หากระบบ Cloud จัดการ SSL Offloading ให้แล้ว
    // เพราะจะทำให้เกิด Infinite Loop ได้
    app.UseHsts();
}

app.UseRequestLocalization();
app.UseStaticFiles();

app.UseRouting();

// 5. ลำดับต้องเป็น Authentication ก่อน Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();