using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NguyenDinhMinhKhang_2380600989.Data;
using NguyenDinhMinhKhang_2380600989.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// DATABASE CONFIGURATION
// ============================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================
// COOKIE CONFIGURATION (SỬA ĐỂ HỖ TRỢ HTTP)
// ============================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;  // ← SỬA: Hỗ trợ cả HTTP và HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = ".TapHoaXanh.Auth";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;

    // Redirect URLs
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// ============================================
// IDENTITY CONFIGURATION
// ============================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Sign In
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;

    // Password
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // User
    options.User.RequireUniqueEmail = true;

    // Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ============================================
// MVC & RAZOR PAGES
// ============================================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ============================================
// CUSTOM SERVICES
// ============================================
builder.Services.AddScoped<CartService>();
builder.Services.AddMemoryCache();  // Cho OTP

// ============================================
// EMAIL SERVICES (ĐĂNG KÝ CẢ 2)
// ============================================
// 1. Custom IEmailService (cho code của bạn)
builder.Services.AddScoped<IEmailService, EmailService>();

// 2. IEmailSender của ASP.NET Core Identity (SỬA: Dùng EmailService thay vì EmailSender)
builder.Services.AddTransient<IEmailSender, EmailService>();  // ← SỬA ĐÂY

// ============================================
// OAUTH 2.0 - GOOGLE AUTHENTICATION
// ============================================
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;
        options.Scope.Add("email");
        options.Scope.Add("profile");

        options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                var redirectUri = context.RedirectUri;
                if (!redirectUri.Contains("prompt="))
                {
                    redirectUri = redirectUri + "&prompt=select_account";
                }
                context.Response.Redirect(redirectUri);
                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/Identity/Account/Login?error=google_auth_failed");
                return Task.CompletedTask;
            },
            OnTicketReceived = context =>
            {
                System.Diagnostics.Debug.WriteLine("Google OAuth Ticket Received");
                return Task.CompletedTask;
            }
        };
    });

// ============================================
// BUILD APP
// ============================================
var app = builder.Build();

// ============================================
// SEED DATABASE
// ============================================
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        await DbSeeder.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// ============================================
// MIDDLEWARE PIPELINE
// ============================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ← COMMENT DÒNG NÀY VÌ SMARTERASP DÙNG HTTP
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ============================================
// ROUTING
// ============================================

// Friendly URL cho chi tiết sản phẩm
app.MapControllerRoute(
    name: "ProductDetail",
    pattern: "San-pham/{id}/{slug?}",
    defaults: new { controller = "Home", action = "Detail" });

// Friendly URL cho danh mục sản phẩm
app.MapControllerRoute(
    name: "ProductCategory",
    pattern: "Danh-muc/{id}/{slug?}",
    defaults: new { controller = "Home", action = "Products" });

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ============================================
// RUN APP
// ============================================
app.Run();