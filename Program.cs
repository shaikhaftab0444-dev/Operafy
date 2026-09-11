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

        // Safe automatic raw SQL schema patch BEFORE any LINQ queries execute
        string schemaPatchSql = @"
            -- 1. Ensure ShiftId and JoiningDate exist on erp_Users in AITStudent schema
            IF OBJECT_ID('AITStudent.erp_Users', 'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_Users') AND name = 'ShiftId')
                BEGIN
                    ALTER TABLE AITStudent.erp_Users ADD ShiftId INT NULL;
                END
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_Users') AND name = 'JoiningDate')
                BEGIN
                    ALTER TABLE AITStudent.erp_Users ADD JoiningDate DATETIME NULL;
                END
            END
            ELSE IF OBJECT_ID('erp_Users', 'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('erp_Users') AND name = 'ShiftId')
                BEGIN
                    ALTER TABLE erp_Users ADD ShiftId INT NULL;
                END
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('erp_Users') AND name = 'JoiningDate')
                BEGIN
                    ALTER TABLE erp_Users ADD JoiningDate DATETIME NULL;
                END
            END

            -- AspNetUsers fallback
            IF OBJECT_ID('AspNetUsers', 'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'ShiftId')
                BEGIN
                    ALTER TABLE AspNetUsers ADD ShiftId INT NULL;
                END
            END

            -- Employees fallback
            IF OBJECT_ID('Employees', 'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Employees') AND name = 'ShiftId')
                BEGIN
                    ALTER TABLE Employees ADD ShiftId INT NULL;
                END
            END

            -- 2. Ensure erp_WorkShifts table exists
            IF OBJECT_ID('AITStudent.erp_WorkShifts', 'U') IS NULL
            BEGIN
                CREATE TABLE AITStudent.erp_WorkShifts (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    ShiftName NVARCHAR(150) NOT NULL,
                    StartTime TIME NOT NULL,
                    EndTime TIME NOT NULL,
                    GracePeriodMinutes INT NOT NULL DEFAULT 15
                );

                INSERT INTO AITStudent.erp_WorkShifts (ShiftName, StartTime, EndTime, GracePeriodMinutes)
                VALUES 
                ('Morning General (09:00 AM - 06:00 PM)', '09:00:00', '18:00:00', 15),
                ('Early Shift (08:00 AM - 05:00 PM)', '08:00:00', '17:00:00', 15),
                ('Evening Shift (02:00 PM - 11:00 PM)', '14:00:00', '23:00:00', 15),
                ('Night Shift (10:00 PM - 07:00 AM)', '22:00:00', '07:00:00', 15);
            END

            IF OBJECT_ID('WorkShifts', 'U') IS NULL AND OBJECT_ID('AITStudent.erp_WorkShifts', 'U') IS NULL
            BEGIN
                CREATE TABLE WorkShifts (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    ShiftName NVARCHAR(150) NOT NULL,
                    StartTime TIME NOT NULL,
                    EndTime TIME NOT NULL,
                    GracePeriodMinutes INT NOT NULL DEFAULT 15
                );

                INSERT INTO WorkShifts (ShiftName, StartTime, EndTime, GracePeriodMinutes)
                VALUES 
                ('General Day Shift (09:30 AM - 06:30 PM)', '09:30:00', '18:30:00', 15),
                ('Morning Shift (07:00 AM - 04:00 PM)', '07:00:00', '16:00:00', 15),
                ('Night Shift (08:00 PM - 05:00 AM)', '20:00:00', '05:00:00', 15);
            END

            -- 3. Ensure SystemAuditTrails and AuditFlaggedItems exist
            IF OBJECT_ID('AITStudent.SystemAuditTrails', 'U') IS NULL AND OBJECT_ID('SystemAuditTrails', 'U') IS NULL
            BEGIN
                CREATE TABLE SystemAuditTrails (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    EntityName NVARCHAR(100) NOT NULL,
                    RecordId NVARCHAR(100) NOT NULL,
                    ActionType NVARCHAR(50) NOT NULL,
                    PerformedByUserId NVARCHAR(100) NULL,
                    PerformedByUserUserId INT NULL,
                    ChangesSummary NVARCHAR(500) NOT NULL,
                    IpAddress NVARCHAR(50) NULL DEFAULT '127.0.0.1',
                    Timestamp DATETIME NOT NULL DEFAULT GETUTCDATE()
                );
            END

            IF OBJECT_ID('AITStudent.AuditFlaggedItems', 'U') IS NULL AND OBJECT_ID('AuditFlaggedItems', 'U') IS NULL
            BEGIN
                CREATE TABLE AuditFlaggedItems (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Module NVARCHAR(100) NOT NULL,
                    ReferenceNumber NVARCHAR(100) NOT NULL,
                    Description NVARCHAR(500) NOT NULL,
                    DiscrepancyAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
                    Severity NVARCHAR(50) NOT NULL DEFAULT 'Medium',
                    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending Review',
                    FlaggedByUserId NVARCHAR(100) NULL,
                    FlaggedOn DATETIME NOT NULL DEFAULT GETUTCDATE()
                );
            END
        ";
        await context.Database.ExecuteSqlRawAsync(schemaPatchSql);

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
        await SeedData.InitializeRecruitmentTablesAsync(context);
        await SeedData.InitializePerformanceTablesAsync(context);
        await SeedData.InitializeHRAttendanceTablesAsync(context);
        await SeedData.InitializeLeaveApplicationsAsync(context);
        await SeedData.InitializePayrollRunsAndPayslipsAsync(context);
        await SeedData.InitializeAuditorDataAsync(context);
        await DbInitializer.InitializeAsync(context);
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