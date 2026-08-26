using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using ERP_System.Models;
using ERP_System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DB Context with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ERP_System.Helpers.ICurrencyService, ERP_System.Helpers.CurrencyService>();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

// Seed database permissions table on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await SeedData.InitializePermissionsAsync(context);
        await SeedData.InitializeSalesManagementTablesAsync(context);
        await SeedData.InitializeHRManagementTablesAsync(context);
        await SeedData.InitializeESSManagementTablesAsync(context);
        await SeedData.InitializeInventoryManagementTablesAsync(context);
        await SeedData.InitializeAdminManagementTablesAsync(context);
        await SeedData.InitializeSuperAdminManagementTablesAsync(context);
        await SeedData.InitializeTransactionsAsync(context);
        await SeedData.InitializeDesignationsAsync(context);
        await SeedData.InitializeHolidaysAsync(context);
        await SeedData.InitializeRegionalSettingsAsync(context);
        await SeedData.InitializeImportLogsAsync(context);
        await SeedData.InitializeExportLogsAsync(context);
        //await SeedData.InitializeAuditLogsAsync(context);
        //await SeedData.InitializeHRAttendanceTablesAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the RolePermissions database table.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
            {
                await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync(context, CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/Account/Login");
                return;
            }
        }
    }
    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();