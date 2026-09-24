using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_System.Migrations
{
    /// <inheritdoc />
    public partial class AddSignatureToOfferLetter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_Payslips_erp_Users_UserId",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.AddColumn<string>(
                name: "BranchName",
                schema: "AITStudent",
                table: "erp_Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                schema: "AITStudent",
                table: "erp_Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                schema: "AITStudent",
                table: "erp_Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JoiningDate",
                schema: "AITStudent",
                table: "erp_Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportingManagerId",
                schema: "AITStudent",
                table: "erp_Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportingManagerName",
                schema: "AITStudent",
                table: "erp_Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShiftId",
                schema: "AITStudent",
                table: "erp_Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanApprove",
                schema: "AITStudent",
                table: "erp_RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreate",
                schema: "AITStudent",
                table: "erp_RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDelete",
                schema: "AITStudent",
                table: "erp_RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanEdit",
                schema: "AITStudent",
                table: "erp_RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanView",
                schema: "AITStudent",
                table: "erp_RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "GstRate",
                schema: "AITStudent",
                table: "erp_Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ReservedStockQty",
                schema: "AITStudent",
                table: "erp_Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitSellingPrice",
                schema: "AITStudent",
                table: "erp_Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "TransportAllowance",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<decimal>(
                name: "ProvidentFund",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ProfessionalTax",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "PayPeriod",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "PaidDays",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetSalary",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MedicalAllowance",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "HRA",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "BasicSalary",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "AbsentDays",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BonusIncentives",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ESI",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerESI",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerPF",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossSalary",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LOPDeduction",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LTA",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherAllowance",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OvertimeHours",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimePay",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaidLeaveDays",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayrollRunId",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayslipNumber",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PresentDays",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SpecialAllowance",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TDS",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCTC",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeductions",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalWorkingDays",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnpaidLeaveDays",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BranchName",
                schema: "AITStudent",
                table: "erp_Branches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "AITStudent",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AssignedRepId",
                schema: "AITStudent",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedRepUserUserId",
                schema: "AITStudent",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                schema: "AITStudent",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "AITStudent",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                schema: "AITStudent",
                table: "Customers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                schema: "AITStudent",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                schema: "AITStudent",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "OutstandingBalance",
                schema: "AITStudent",
                table: "Customers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TaxIdOrGSTIN",
                schema: "AITStudent",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditFlaggedItems",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DiscrepancyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AuditedByUserUserId = table.Column<int>(type: "int", nullable: true),
                    FlaggedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FlaggedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditFlaggedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditFlaggedItems_erp_Users_AuditedByUserUserId",
                        column: x => x.AuditedByUserUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CompanyBankAccounts",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IFSCCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPrimaryAccount = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyBankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyHolidays",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HolidayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyHolidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "erp_AdminAnnouncements",
                schema: "AITStudent",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TargetAudience = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetBranch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_AdminAnnouncements", x => x.AnnouncementId);
                });

            migrationBuilder.CreateTable(
                name: "erp_AdminBackupLogs",
                schema: "AITStudent",
                columns: table => new
                {
                    BackupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Filename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BackupSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BackupType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TriggeredBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StorageLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_AdminBackupLogs", x => x.BackupId);
                });

            migrationBuilder.CreateTable(
                name: "erp_AdminBranchHours",
                schema: "AITStudent",
                columns: table => new
                {
                    HourId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    OpeningTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClosingTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OffDay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GracePeriod = table.Column<int>(type: "int", nullable: false),
                    BreakDuration = table.Column<int>(type: "int", nullable: false),
                    HalfDayMinHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsContinuousShift = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_AdminBranchHours", x => x.HourId);
                    table.ForeignKey(
                        name: "FK_erp_AdminBranchHours_erp_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_AdminLoginAudits",
                schema: "AITStudent",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SessionDuration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_AdminLoginAudits", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "erp_AdminPasswordResets",
                schema: "AITStudent",
                columns: table => new
                {
                    ResetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryMethod = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_AdminPasswordResets", x => x.ResetId);
                });

            migrationBuilder.CreateTable(
                name: "erp_AllowanceDeductionMasters",
                schema: "AITStudent",
                columns: table => new
                {
                    ComponentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ComponentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Taxability = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CalculationBasis = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DefaultValueOrRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayFrequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_AllowanceDeductionMasters", x => x.ComponentId);
                });

            migrationBuilder.CreateTable(
                name: "erp_AppraisalCycles",
                schema: "AITStudent",
                columns: table => new
                {
                    CycleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CycleName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SelfReviewDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManagerReviewDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicableDepartmentIds = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_AppraisalCycles", x => x.CycleId);
                });

            migrationBuilder.CreateTable(
                name: "erp_AuditLogs",
                schema: "AITStudent",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActionSubject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Device = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_AuditLogs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "erp_BankReconciliations",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankAccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StatementBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnreconciledEntriesCount = table.Column<int>(type: "int", nullable: false),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_BankReconciliations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "erp_Candidates",
                schema: "AITStudent",
                columns: table => new
                {
                    CandidateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Education = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Experience = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Skills = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CurrentCompany = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CurrentSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExpectedSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NoticePeriod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResumePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LinkedIn = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Portfolio = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ApplicationSource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferredBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Candidates", x => x.CandidateId);
                });

            migrationBuilder.CreateTable(
                name: "erp_Contracts",
                schema: "AITStudent",
                columns: table => new
                {
                    ContractId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SigningStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Contracts", x => x.ContractId);
                });

            migrationBuilder.CreateTable(
                name: "erp_Currencies",
                schema: "AITStudent",
                columns: table => new
                {
                    CurrencyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CurrencyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsBaseCurrency = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Currencies", x => x.CurrencyId);
                });

            migrationBuilder.CreateTable(
                name: "erp_Departments",
                schema: "AITStudent",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HODId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ParentDepartmentId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Departments", x => x.DepartmentId);
                    table.ForeignKey(
                        name: "FK_erp_Departments_erp_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_Departments_erp_Departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_erp_Departments_erp_Users_HODId",
                        column: x => x.HODId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_DepartmentTasks",
                schema: "AITStudent",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AssignedToName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AssignedToEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_DepartmentTasks", x => x.TaskId);
                });

            migrationBuilder.CreateTable(
                name: "erp_ESSExpenseClaims",
                schema: "AITStudent",
                columns: table => new
                {
                    ExpenseClaimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExpenseType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClaimDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReceiptFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ManagerStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerRemarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_ESSExpenseClaims", x => x.ExpenseClaimId);
                    table.ForeignKey(
                        name: "FK_erp_ESSExpenseClaims_erp_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_ESSLeaveApplications",
                schema: "AITStudent",
                columns: table => new
                {
                    LeaveApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDays = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ManagerStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerRemarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_ESSLeaveApplications", x => x.LeaveApplicationId);
                });

            migrationBuilder.CreateTable(
                name: "erp_ESSPunches",
                schema: "AITStudent",
                columns: table => new
                {
                    PunchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PunchSource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_ESSPunches", x => x.PunchId);
                });

            migrationBuilder.CreateTable(
                name: "erp_ESSSupportTickets",
                schema: "AITStudent",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResolutionRemarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_ESSSupportTickets", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_erp_ESSSupportTickets_erp_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_ESSTasks",
                schema: "AITStudent",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TaskTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DepartmentTaskId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_ESSTasks", x => x.TaskId);
                });

            migrationBuilder.CreateTable(
                name: "erp_ExportAuditLogs",
                schema: "AITStudent",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatasetName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecordsCount = table.Column<int>(type: "int", nullable: false),
                    ExportedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_ExportAuditLogs", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "erp_HierarchicalTasks",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    TaskType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsGeneralTask = table.Column<bool>(type: "bit", nullable: false),
                    TargetWarehouseLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_HierarchicalTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "erp_Holidays",
                schema: "AITStudent",
                columns: table => new
                {
                    HolidayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HolidayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Holidays", x => x.HolidayId);
                    table.ForeignKey(
                        name: "FK_erp_Holidays_erp_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Branches",
                        principalColumn: "BranchId");
                });

            migrationBuilder.CreateTable(
                name: "erp_HRAttendanceLogs",
                schema: "AITStudent",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkHours = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PunchSource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_HRAttendanceLogs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "erp_HRAttendanceRegularizations",
                schema: "AITStudent",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CorrectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncorrectPunch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestedCorrectTime = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AdminRemarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ManagerStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerRemarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_HRAttendanceRegularizations", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "erp_HRBiometricDevices",
                schema: "AITStudent",
                columns: table => new
                {
                    DeviceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IpOrLocation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ConnectionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastSyncTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TodaySyncCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_HRBiometricDevices", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "erp_HROvertimeRecords",
                schema: "AITStudent",
                columns: table => new
                {
                    OvertimeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MonthYear = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StandardHours = table.Column<int>(type: "int", nullable: false),
                    HoursLogged = table.Column<int>(type: "int", nullable: false),
                    OvertimeHours = table.Column<int>(type: "int", nullable: false),
                    Multiplier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOvertimePay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayoutStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_HROvertimeRecords", x => x.OvertimeId);
                });

            migrationBuilder.CreateTable(
                name: "erp_HRShiftRosters",
                schema: "AITStudent",
                columns: table => new
                {
                    RosterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ShiftName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timings = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WeeklyOffs = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_HRShiftRosters", x => x.RosterId);
                });

            migrationBuilder.CreateTable(
                name: "erp_ImportLogs",
                schema: "AITStudent",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    SuccessRows = table.Column<int>(type: "int", nullable: false),
                    FailedRows = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogFileUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_ImportLogs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "erp_InvGrns",
                schema: "AITStudent",
                columns: table => new
                {
                    GrnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrnNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_InvGrns", x => x.GrnId);
                });

            migrationBuilder.CreateTable(
                name: "erp_InvScrapWriteOffs",
                schema: "AITStudent",
                columns: table => new
                {
                    ScrapId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScrapNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    QtyScrapped = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    WriteOffDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_InvScrapWriteOffs", x => x.ScrapId);
                });

            migrationBuilder.CreateTable(
                name: "erp_InvStockAudits",
                schema: "AITStudent",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AuditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiscrepancyFound = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_InvStockAudits", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "erp_InvTransfers",
                schema: "AITStudent",
                columns: table => new
                {
                    TransferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FromWarehouse = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ToWarehouse = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_InvTransfers", x => x.TransferId);
                });

            migrationBuilder.CreateTable(
                name: "erp_InvWarehouses",
                schema: "AITStudent",
                columns: table => new
                {
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_InvWarehouses", x => x.WarehouseId);
                });

            migrationBuilder.CreateTable(
                name: "erp_JournalVouchers",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VoucherType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DebitAccount = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreditAccount = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_JournalVouchers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "erp_Leads",
                schema: "AITStudent",
                columns: table => new
                {
                    LeadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Stage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedExecutiveId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Leads", x => x.LeadId);
                    table.ForeignKey(
                        name: "FK_erp_Leads_erp_Users_AssignedExecutiveId",
                        column: x => x.AssignedExecutiveId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_Offboardings",
                schema: "AITStudent",
                columns: table => new
                {
                    OffboardingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ResignationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastWorkingDay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssetReturn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ITClearance = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FinanceClearance = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExitInterview = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FfStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Offboardings", x => x.OffboardingId);
                });

            migrationBuilder.CreateTable(
                name: "erp_Onboardings",
                schema: "AITStudent",
                columns: table => new
                {
                    OnboardingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentsStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BackgroundCheck = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KycProgress = table.Column<int>(type: "int", nullable: false),
                    OrientationStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Onboardings", x => x.OnboardingId);
                });

            migrationBuilder.CreateTable(
                name: "erp_PaymentReceipts",
                schema: "AITStudent",
                columns: table => new
                {
                    PaymentReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PendingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_PaymentReceipts", x => x.PaymentReceiptId);
                });

            migrationBuilder.CreateTable(
                name: "erp_PaymentReceivables",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PendingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_PaymentReceivables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_PaymentReceivables_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "AITStudent",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_ProductCategories",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HsnCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DefaultGstRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_ProductCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "erp_Quotations",
                schema: "AITStudent",
                columns: table => new
                {
                    QuotationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Quotations", x => x.QuotationId);
                });

            migrationBuilder.CreateTable(
                name: "erp_RegionalConfigurations",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CurrencySymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NumberSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Timezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaxSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FinancialYearCycle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_RegionalConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "erp_SalesCoordinatorProfiles",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoordinatorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserUserId = table.Column<int>(type: "int", nullable: true),
                    PrimaryTerritory = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalesCoordinatorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_SalesCoordinatorProfiles_erp_Users_UserUserId",
                        column: x => x.UserUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_SalesLeads",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeadTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedDealValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WinProbability = table.Column<int>(type: "int", nullable: false),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedToUserUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedCloseDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalesLeads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_SalesLeads_erp_Users_AssignedToUserUserId",
                        column: x => x.AssignedToUserUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_SalesOrders",
                schema: "AITStudent",
                columns: table => new
                {
                    SalesOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalesOrders", x => x.SalesOrderId);
                    table.ForeignKey(
                        name: "FK_erp_SalesOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "AITStudent",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_SalesQuotations",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalesQuotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_SalesQuotations_erp_Users_CreatedByUserUserId",
                        column: x => x.CreatedByUserUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_SalesReturns",
                schema: "AITStudent",
                columns: table => new
                {
                    SalesReturnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalInvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefundValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditNoteNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalesReturns", x => x.SalesReturnId);
                    table.ForeignKey(
                        name: "FK_erp_SalesReturns_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "AITStudent",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_SalesTargetAllocations",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalQuarter = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SalesRepUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SalesRepUserUserId = table.Column<int>(type: "int", nullable: true),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AchievedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalesTargetAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_SalesTargetAllocations_erp_Users_SalesRepUserUserId",
                        column: x => x.SalesRepUserUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_SalesTargets",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesRepUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalesRepUserUserId = table.Column<int>(type: "int", nullable: true),
                    FiscalQuarter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AchievedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExecutiveUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalesTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_SalesTargets_erp_Users_SalesRepUserUserId",
                        column: x => x.SalesRepUserUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_StatutoryConfigurations",
                schema: "AITStudent",
                columns: table => new
                {
                    StatutoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmployerRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WageCeilingLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StandardDeductionAnnual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DefaultTaxRegime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConfigurationDetailsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_StatutoryConfigurations", x => x.StatutoryId);
                });

            migrationBuilder.CreateTable(
                name: "erp_StatutoryFilingLogs",
                schema: "AITStudent",
                columns: table => new
                {
                    FilingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplianceAct = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Period = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_StatutoryFilingLogs", x => x.FilingId);
                });

            migrationBuilder.CreateTable(
                name: "erp_SuperAdminErrorLogs",
                schema: "AITStudent",
                columns: table => new
                {
                    ErrorLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ErrorMessage = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SuperAdminErrorLogs", x => x.ErrorLogId);
                });

            migrationBuilder.CreateTable(
                name: "erp_SuperAdminIntegrations",
                schema: "AITStudent",
                columns: table => new
                {
                    IntegrationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ApiUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SuperAdminIntegrations", x => x.IntegrationId);
                });

            migrationBuilder.CreateTable(
                name: "erp_SuperAdminMaintenances",
                schema: "AITStudent",
                columns: table => new
                {
                    MaintenanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsMaintenanceMode = table.Column<bool>(type: "bit", nullable: false),
                    CustomMessage = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SetBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SuperAdminMaintenances", x => x.MaintenanceId);
                });

            migrationBuilder.CreateTable(
                name: "erp_SuperAdminPriceOverrides",
                schema: "AITStudent",
                columns: table => new
                {
                    OverrideId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VendorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SuperAdminPriceOverrides", x => x.OverrideId);
                    table.ForeignKey(
                        name: "FK_erp_SuperAdminPriceOverrides_erp_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_SuperAdminRestorePoints",
                schema: "AITStudent",
                columns: table => new
                {
                    RestorePointId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PointName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SuperAdminRestorePoints", x => x.RestorePointId);
                });

            migrationBuilder.CreateTable(
                name: "erp_TaxSlabs",
                schema: "AITStudent",
                columns: table => new
                {
                    TaxSlabId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CombinedRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGST = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGST = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Regime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRcmActive = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_TaxSlabs", x => x.TaxSlabId);
                });

            migrationBuilder.CreateTable(
                name: "erp_Transfers",
                schema: "AITStudent",
                columns: table => new
                {
                    TransferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FromDeptOrDesg = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ToDeptOrDesg = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Transfers", x => x.TransferId);
                });

            migrationBuilder.CreateTable(
                name: "erp_UnitsOfMeasure",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsBaseUnit = table.Column<bool>(type: "bit", nullable: false),
                    AllowDecimals = table.Column<bool>(type: "bit", nullable: false),
                    OfficialGstUomCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_UnitsOfMeasure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "erp_UserPermissions",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsAllowed = table.Column<bool>(type: "bit", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CanCreate = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    CanApprove = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_UserPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_UserPermissions_erp_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_WarehouseLocations",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WarehouseName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    WarehouseType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocationAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SupervisorUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    MaxCapacityUnits = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_WarehouseLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_WarehouseLocations_erp_Users_SupervisorId",
                        column: x => x.SupervisorId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_WorkShifts",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    GracePeriodMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_WorkShifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InternalMessages",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderUserId = table.Column<int>(type: "int", nullable: false),
                    RecipientUserId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Folder = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalMessages_erp_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InternalMessages_erp_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    YearlyLimit = table.Column<int>(type: "int", nullable: false),
                    CarryForwardLimit = table.Column<int>(type: "int", nullable: false),
                    IsEncashable = table.Column<bool>(type: "bit", nullable: false),
                    EligibilityCriteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollComponents",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Taxability = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CalculationBasis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultValueOrRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxCapLimit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayFrequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollComponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatutoryRuleConfigs",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployerRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmployeeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WageCeiling = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StandardDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActiveRegime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatutoryRuleConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemAuditTrails",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecordId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PerformedByUserUserId = table.Column<int>(type: "int", nullable: true),
                    ChangesSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAuditTrails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemAuditTrails_erp_Users_PerformedByUserUserId",
                        column: x => x.PerformedByUserUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SystemMutationLogs",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecordId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangesSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OldValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemMutationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemNotifications",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemNotifications_erp_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankTransactions",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyBankAccountId = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DebitAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankTransactions_CompanyBankAccounts_CompanyBankAccountId",
                        column: x => x.CompanyBankAccountId,
                        principalSchema: "AITStudent",
                        principalTable: "CompanyBankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_PayrollRuns",
                schema: "AITStudent",
                columns: table => new
                {
                    PayrollRunId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayPeriod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalEmployees = table.Column<int>(type: "int", nullable: false),
                    TotalGrossSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEmployerPF = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEmployerESI = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompanyBankAccountId = table.Column<int>(type: "int", nullable: true),
                    ProcessedByUserId = table.Column<int>(type: "int", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidByUserId = table.Column<int>(type: "int", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisbursedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_PayrollRuns", x => x.PayrollRunId);
                    table.ForeignKey(
                        name: "FK_erp_PayrollRuns_CompanyBankAccounts_CompanyBankAccountId",
                        column: x => x.CompanyBankAccountId,
                        principalSchema: "AITStudent",
                        principalTable: "CompanyBankAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StatutoryReturnFilings",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplianceReturn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilingPeriod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptOrChallanNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChallanAmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompanyBankAccountId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoggedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatutoryReturnFilings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatutoryReturnFilings_CompanyBankAccounts_CompanyBankAccountId",
                        column: x => x.CompanyBankAccountId,
                        principalSchema: "AITStudent",
                        principalTable: "CompanyBankAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "erp_EmployeeAppraisals",
                schema: "AITStudent",
                columns: table => new
                {
                    AppraisalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CycleId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    SelfReviewSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    SelfRating = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    SelfComments = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    SelfSubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerReviewSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    ManagerRating = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    ManagerComments = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    ManagerSubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HRComments = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    GoalScore = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    KpiScore = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    FinalScore = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    FinalRatingBand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_EmployeeAppraisals", x => x.AppraisalId);
                    table.ForeignKey(
                        name: "FK_erp_EmployeeAppraisals_erp_AppraisalCycles_CycleId",
                        column: x => x.CycleId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_AppraisalCycles",
                        principalColumn: "CycleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_EmployeeAppraisals_erp_Users_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_EmployeeAppraisals_erp_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_CurrencyRateHistories",
                schema: "AITStudent",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_CurrencyRateHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_erp_CurrencyRateHistories_erp_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Currencies",
                        principalColumn: "CurrencyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_Designations",
                schema: "AITStudent",
                columns: table => new
                {
                    DesignationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DesignationCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    HierarchyLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinCTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxCTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Designations", x => x.DesignationId);
                    table.ForeignKey(
                        name: "FK_erp_Designations_erp_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_ProcurementCatalogItems",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_ProcurementCatalogItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_ProcurementCatalogItems_erp_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_SalaryStructureMasters",
                schema: "AITStudent",
                columns: table => new
                {
                    StructureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StructureName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BasicPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HRAPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LTAPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConveyanceAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MedicalAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AutoCalculateSpecialAllowance = table.Column<bool>(type: "bit", nullable: false),
                    IsPFEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PFEmployeeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PFEmployerRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PFWageCeiling = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsESIEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ESIEmployeeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ESIEmployerRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ESIWageCeiling = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPTEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsTDSEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalaryStructureMasters", x => x.StructureId);
                    table.ForeignKey(
                        name: "FK_erp_SalaryStructureMasters_erp_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Departments",
                        principalColumn: "DepartmentId");
                });

            migrationBuilder.CreateTable(
                name: "erp_ProductSubCategories",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_ProductSubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_ProductSubCategories_erp_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_SalesExecutiveProfiles",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExecCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserUserId = table.Column<int>(type: "int", nullable: true),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    MonthlyTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedCoordinatorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalesExecutiveProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_SalesExecutiveProfiles_erp_SalesCoordinatorProfiles_AssignedCoordinatorId",
                        column: x => x.AssignedCoordinatorId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_SalesCoordinatorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_SalesExecutiveProfiles_erp_Users_UserUserId",
                        column: x => x.UserUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_SalesInvoices",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalesOrderId = table.Column<int>(type: "int", nullable: true),
                    LinkedOrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxableValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GstAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserUserId = table.Column<int>(type: "int", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalesInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_SalesInvoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "AITStudent",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_SalesInvoices_erp_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_SalesOrders",
                        principalColumn: "SalesOrderId");
                    table.ForeignKey(
                        name: "FK_erp_SalesInvoices_erp_Users_CreatedByUserUserId",
                        column: x => x.CreatedByUserUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_SalesOrderItems",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ItemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalesOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_SalesOrderItems_erp_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Products",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_erp_SalesOrderItems_erp_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_SalesOrders",
                        principalColumn: "SalesOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_UomConversions",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromUomId = table.Column<int>(type: "int", nullable: false),
                    ToUomId = table.Column<int>(type: "int", nullable: false),
                    ConversionFactor = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_UomConversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_UomConversions_erp_UnitsOfMeasure_FromUomId",
                        column: x => x.FromUomId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_UomConversions_erp_UnitsOfMeasure_ToUomId",
                        column: x => x.ToUomId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_GoodsReceiptNotes",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrnNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PurchaseOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupplierName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedByUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReceivedById = table.Column<int>(type: "int", nullable: true),
                    DestinationWarehouseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_GoodsReceiptNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_GoodsReceiptNotes_erp_Users_ReceivedById",
                        column: x => x.ReceivedById,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_GoodsReceiptNotes_erp_WarehouseLocations_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_MaterialDispatches",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DispatchSlipNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SalesOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DestinationParty = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CarrierName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EWayBillNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DispatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceWarehouseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_MaterialDispatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_MaterialDispatches_erp_WarehouseLocations_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDays = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApproverRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActionedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ManagerStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ManagerRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalSchema: "AITStudent",
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_erp_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_BonusIncentives",
                schema: "AITStudent",
                columns: table => new
                {
                    BonusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PerformancePeriod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PayoutMonth = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayrollRunId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_BonusIncentives", x => x.BonusId);
                    table.ForeignKey(
                        name: "FK_erp_BonusIncentives_erp_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_PayrollRuns",
                        principalColumn: "PayrollRunId");
                    table.ForeignKey(
                        name: "FK_erp_BonusIncentives_erp_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalarySlips",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    MonthName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkingDays = table.Column<int>(type: "int", nullable: false),
                    PaidDays = table.Column<int>(type: "int", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Hra = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Allowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PfDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PtDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TdsDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalarySlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalarySlips_erp_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_PayrollRuns",
                        principalColumn: "PayrollRunId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalarySlips_erp_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_JobOpenings",
                schema: "AITStudent",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    DesignationId = table.Column<int>(type: "int", nullable: true),
                    HiringManagerId = table.Column<int>(type: "int", nullable: true),
                    RecruiterId = table.Column<int>(type: "int", nullable: true),
                    EmploymentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Vacancies = table.Column<int>(type: "int", nullable: false),
                    JobLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WorkMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExperienceRequired = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinimumEducation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiredSkills = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Responsibilities = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Requirements = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MinimumSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaximumSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_JobOpenings", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_erp_JobOpenings_erp_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_JobOpenings_erp_Designations_DesignationId",
                        column: x => x.DesignationId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Designations",
                        principalColumn: "DesignationId");
                    table.ForeignKey(
                        name: "FK_erp_JobOpenings_erp_Users_HiringManagerId",
                        column: x => x.HiringManagerId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_erp_JobOpenings_erp_Users_RecruiterId",
                        column: x => x.RecruiterId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_Kpis",
                schema: "AITStudent",
                columns: table => new
                {
                    KpiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KpiName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    DesignationId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualAchievement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Weightage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MeasurementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReviewPeriod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AchievementPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Kpis", x => x.KpiId);
                    table.ForeignKey(
                        name: "FK_erp_Kpis_erp_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_erp_Kpis_erp_Designations_DesignationId",
                        column: x => x.DesignationId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Designations",
                        principalColumn: "DesignationId");
                    table.ForeignKey(
                        name: "FK_erp_Kpis_erp_Users_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_Okrs",
                schema: "AITStudent",
                columns: table => new
                {
                    OkrId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObjectiveTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    DesignationId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Weightage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OverallProgressPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Okrs", x => x.OkrId);
                    table.ForeignKey(
                        name: "FK_erp_Okrs_erp_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_erp_Okrs_erp_Designations_DesignationId",
                        column: x => x.DesignationId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Designations",
                        principalColumn: "DesignationId");
                    table.ForeignKey(
                        name: "FK_erp_Okrs_erp_Users_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_PurchaseRequisitions",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    CatalogItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedTotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UrgencyLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BusinessJustification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_PurchaseRequisitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_PurchaseRequisitions_erp_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_PurchaseRequisitions_erp_ProcurementCatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_ProcurementCatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_PurchaseRequisitions_erp_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_EmployeeSalaryAssignments",
                schema: "AITStudent",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StructureId = table.Column<int>(type: "int", nullable: true),
                    AnnualCTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyBasic = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyHRA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyLTA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlySpecialAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyConveyance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyMedical = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyOtherAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyGrossSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyPFEmployee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyESIEmployee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyPT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyTDS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyNetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyPFEmployer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyESIEmployer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_EmployeeSalaryAssignments", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_erp_EmployeeSalaryAssignments_erp_SalaryStructureMasters_StructureId",
                        column: x => x.StructureId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_SalaryStructureMasters",
                        principalColumn: "StructureId");
                    table.ForeignKey(
                        name: "FK_erp_EmployeeSalaryAssignments_erp_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_CatalogItems",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SKU = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: true),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentStock = table.Column<int>(type: "int", nullable: false),
                    MinimumReorderLevel = table.Column<int>(type: "int", nullable: false),
                    BranchLocation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BinLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitsSold = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_CatalogItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_CatalogItems_erp_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_CatalogItems_erp_ProductSubCategories_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_ProductSubCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_CatalogItems_erp_UnitsOfMeasure_UomId",
                        column: x => x.UomId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_CandidateApplications",
                schema: "AITStudent",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatchScore = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_CandidateApplications", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_erp_CandidateApplications_erp_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Candidates",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_CandidateApplications_erp_JobOpenings_JobId",
                        column: x => x.JobId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_JobOpenings",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_KeyResults",
                schema: "AITStudent",
                columns: table => new
                {
                    KeyResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OkrId = table.Column<int>(type: "int", nullable: false),
                    KeyResultName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MeasurementUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Weightage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ProgressPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_KeyResults", x => x.KeyResultId);
                    table.ForeignKey(
                        name: "FK_erp_KeyResults_erp_Okrs_OkrId",
                        column: x => x.OkrId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Okrs",
                        principalColumn: "OkrId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_BinRackMasters",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    RackCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BinLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedCatalogItemId = table.Column<int>(type: "int", nullable: true),
                    MaxCapacityVolume = table.Column<int>(type: "int", nullable: false),
                    CurrentOccupancyVolume = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_BinRackMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_BinRackMasters_erp_CatalogItems_AssignedCatalogItemId",
                        column: x => x.AssignedCatalogItemId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_CatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_BinRackMasters_erp_WarehouseLocations_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_StockMovementLogs",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReferenceDocument = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HandledByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_StockMovementLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_StockMovementLogs_erp_CatalogItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_CatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_CandidateStageHistories",
                schema: "AITStudent",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    PreviousStage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NewStage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReasonNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_CandidateStageHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_erp_CandidateStageHistories_erp_CandidateApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_CandidateApplications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_CandidateStageHistories_erp_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_InterviewSchedules",
                schema: "AITStudent",
                columns: table => new
                {
                    InterviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    InterviewRound = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InterviewType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InterviewMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EndTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    MeetingLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InterviewerId = table.Column<int>(type: "int", nullable: true),
                    InterviewerNames = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_InterviewSchedules", x => x.InterviewId);
                    table.ForeignKey(
                        name: "FK_erp_InterviewSchedules_erp_CandidateApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_CandidateApplications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_InterviewSchedules_erp_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Candidates",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_InterviewSchedules_erp_JobOpenings_JobId",
                        column: x => x.JobId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_JobOpenings",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_InterviewSchedules_erp_Users_InterviewerId",
                        column: x => x.InterviewerId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_OfferLetters",
                schema: "AITStudent",
                columns: table => new
                {
                    OfferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    DesignationId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    EmploymentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProposedJoiningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportingManagerId = table.Column<int>(type: "int", nullable: true),
                    OfferedCTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalaryStructure = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    OfferExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TermsAndConditions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConvertedToEmployeeId = table.Column<int>(type: "int", nullable: true),
                    SignaturePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AcceptedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_OfferLetters", x => x.OfferId);
                    table.ForeignKey(
                        name: "FK_erp_OfferLetters_erp_CandidateApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_CandidateApplications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_OfferLetters_erp_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Candidates",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_OfferLetters_erp_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_erp_OfferLetters_erp_Designations_DesignationId",
                        column: x => x.DesignationId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Designations",
                        principalColumn: "DesignationId");
                    table.ForeignKey(
                        name: "FK_erp_OfferLetters_erp_JobOpenings_JobId",
                        column: x => x.JobId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_JobOpenings",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_OfferLetters_erp_Users_ConvertedToEmployeeId",
                        column: x => x.ConvertedToEmployeeId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_erp_OfferLetters_erp_Users_ReportingManagerId",
                        column: x => x.ReportingManagerId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "erp_DispatchLineItems",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DispatchId = table.Column<int>(type: "int", nullable: false),
                    CatalogItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PickedFromBinId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_DispatchLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_DispatchLineItems_erp_BinRackMasters_PickedFromBinId",
                        column: x => x.PickedFromBinId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_BinRackMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_DispatchLineItems_erp_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_CatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_DispatchLineItems_erp_MaterialDispatches_DispatchId",
                        column: x => x.DispatchId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_MaterialDispatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_GrnLineItems",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrnId = table.Column<int>(type: "int", nullable: false),
                    CatalogItemId = table.Column<int>(type: "int", nullable: false),
                    OrderedQuantity = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false),
                    RejectedQuantity = table.Column<int>(type: "int", nullable: false),
                    TargetBinId = table.Column<int>(type: "int", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_GrnLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_GrnLineItems_erp_BinRackMasters_TargetBinId",
                        column: x => x.TargetBinId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_BinRackMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_GrnLineItems_erp_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_CatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_GrnLineItems_erp_GoodsReceiptNotes_GrnId",
                        column: x => x.GrnId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_GoodsReceiptNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "erp_InterviewFeedbacks",
                schema: "AITStudent",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterviewId = table.Column<int>(type: "int", nullable: false),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    InterviewerId = table.Column<int>(type: "int", nullable: true),
                    TechnicalRating = table.Column<int>(type: "int", nullable: false),
                    CommunicationRating = table.Column<int>(type: "int", nullable: false),
                    ExperienceRating = table.Column<int>(type: "int", nullable: false),
                    ProblemSolvingRating = table.Column<int>(type: "int", nullable: false),
                    CulturalFitRating = table.Column<int>(type: "int", nullable: false),
                    OverallRating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    Strengths = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Weaknesses = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    Recommendation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsFinalized = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_InterviewFeedbacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_erp_InterviewFeedbacks_erp_InterviewSchedules_InterviewId",
                        column: x => x.InterviewId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_InterviewSchedules",
                        principalColumn: "InterviewId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_InterviewFeedbacks_erp_Users_InterviewerId",
                        column: x => x.InterviewerId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 24, 7, 18, 47, 115, DateTimeKind.Utc).AddTicks(9469));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 24, 7, 5, 47, 115, DateTimeKind.Utc).AddTicks(9479));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 24, 6, 20, 47, 115, DateTimeKind.Utc).AddTicks(9481));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 24, 5, 20, 47, 115, DateTimeKind.Utc).AddTicks(9486));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 24, 4, 20, 47, 115, DateTimeKind.Utc).AddTicks(9488));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Products",
                keyColumn: "ProductId",
                keyValue: 1,
                columns: new[] { "GstRate", "ReservedStockQty", "UnitSellingPrice" },
                values: new object[] { 18.00m, 0, 1500m });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Products",
                keyColumn: "ProductId",
                keyValue: 2,
                columns: new[] { "GstRate", "ReservedStockQty", "UnitSellingPrice" },
                values: new object[] { 18.00m, 0, 1500m });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Products",
                keyColumn: "ProductId",
                keyValue: 3,
                columns: new[] { "GstRate", "ReservedStockQty", "UnitSellingPrice" },
                values: new object[] { 18.00m, 0, 1500m });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Products",
                keyColumn: "ProductId",
                keyValue: 4,
                columns: new[] { "GstRate", "ReservedStockQty", "UnitSellingPrice" },
                values: new object[] { 18.00m, 0, 1500m });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Products",
                keyColumn: "ProductId",
                keyValue: 5,
                columns: new[] { "GstRate", "ReservedStockQty", "UnitSellingPrice" },
                values: new object[] { 18.00m, 0, 1500m });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "BranchName", "DepartmentId", "DepartmentName", "JoiningDate", "PasswordHash", "ReportingManagerId", "ReportingManagerName", "ShiftId" },
                values: new object[] { null, null, null, null, "AQAAAAIAAYagAAAAEDpaRPVgC/f21HBonrURUECGQtXIZ9YL8wYvLAOMcSVY2tRryigAFNG0KZTBMOuKDQ==", null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_erp_Users_DepartmentId",
                schema: "AITStudent",
                table: "erp_Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Users_ShiftId",
                schema: "AITStudent",
                table: "erp_Users",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Payslips_PayrollRunId",
                schema: "AITStudent",
                table: "erp_Payslips",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AssignedRepUserUserId",
                schema: "AITStudent",
                table: "Customers",
                column: "AssignedRepUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFlaggedItems_AuditedByUserUserId",
                schema: "AITStudent",
                table: "AuditFlaggedItems",
                column: "AuditedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_CompanyBankAccountId",
                schema: "AITStudent",
                table: "BankTransactions",
                column: "CompanyBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_AdminBranchHours_BranchId",
                schema: "AITStudent",
                table: "erp_AdminBranchHours",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_BinRackMasters_AssignedCatalogItemId",
                schema: "AITStudent",
                table: "erp_BinRackMasters",
                column: "AssignedCatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_BinRackMasters_WarehouseId",
                schema: "AITStudent",
                table: "erp_BinRackMasters",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_BonusIncentives_PayrollRunId",
                schema: "AITStudent",
                table: "erp_BonusIncentives",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_BonusIncentives_UserId",
                schema: "AITStudent",
                table: "erp_BonusIncentives",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_CandidateApplications_CandidateId",
                schema: "AITStudent",
                table: "erp_CandidateApplications",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_CandidateApplications_JobId",
                schema: "AITStudent",
                table: "erp_CandidateApplications",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_CandidateStageHistories_ApplicationId",
                schema: "AITStudent",
                table: "erp_CandidateStageHistories",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_CandidateStageHistories_ChangedByUserId",
                schema: "AITStudent",
                table: "erp_CandidateStageHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_CatalogItems_CategoryId",
                schema: "AITStudent",
                table: "erp_CatalogItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_CatalogItems_SubCategoryId",
                schema: "AITStudent",
                table: "erp_CatalogItems",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_CatalogItems_UomId",
                schema: "AITStudent",
                table: "erp_CatalogItems",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_CurrencyRateHistories_CurrencyId",
                schema: "AITStudent",
                table: "erp_CurrencyRateHistories",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Departments_BranchId",
                schema: "AITStudent",
                table: "erp_Departments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Departments_HODId",
                schema: "AITStudent",
                table: "erp_Departments",
                column: "HODId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Departments_ParentDepartmentId",
                schema: "AITStudent",
                table: "erp_Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Designations_DepartmentId",
                schema: "AITStudent",
                table: "erp_Designations",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_DispatchLineItems_CatalogItemId",
                schema: "AITStudent",
                table: "erp_DispatchLineItems",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_DispatchLineItems_DispatchId",
                schema: "AITStudent",
                table: "erp_DispatchLineItems",
                column: "DispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_DispatchLineItems_PickedFromBinId",
                schema: "AITStudent",
                table: "erp_DispatchLineItems",
                column: "PickedFromBinId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_EmployeeAppraisals_CycleId",
                schema: "AITStudent",
                table: "erp_EmployeeAppraisals",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_EmployeeAppraisals_EmployeeId",
                schema: "AITStudent",
                table: "erp_EmployeeAppraisals",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_EmployeeAppraisals_ManagerId",
                schema: "AITStudent",
                table: "erp_EmployeeAppraisals",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_EmployeeSalaryAssignments_StructureId",
                schema: "AITStudent",
                table: "erp_EmployeeSalaryAssignments",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_EmployeeSalaryAssignments_UserId",
                schema: "AITStudent",
                table: "erp_EmployeeSalaryAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_ESSExpenseClaims_UserId",
                schema: "AITStudent",
                table: "erp_ESSExpenseClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_ESSSupportTickets_UserId",
                schema: "AITStudent",
                table: "erp_ESSSupportTickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_GoodsReceiptNotes_DestinationWarehouseId",
                schema: "AITStudent",
                table: "erp_GoodsReceiptNotes",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_GoodsReceiptNotes_ReceivedById",
                schema: "AITStudent",
                table: "erp_GoodsReceiptNotes",
                column: "ReceivedById");

            migrationBuilder.CreateIndex(
                name: "IX_erp_GrnLineItems_CatalogItemId",
                schema: "AITStudent",
                table: "erp_GrnLineItems",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_GrnLineItems_GrnId",
                schema: "AITStudent",
                table: "erp_GrnLineItems",
                column: "GrnId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_GrnLineItems_TargetBinId",
                schema: "AITStudent",
                table: "erp_GrnLineItems",
                column: "TargetBinId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Holidays_BranchId",
                schema: "AITStudent",
                table: "erp_Holidays",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_InterviewFeedbacks_InterviewerId",
                schema: "AITStudent",
                table: "erp_InterviewFeedbacks",
                column: "InterviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_InterviewFeedbacks_InterviewId",
                schema: "AITStudent",
                table: "erp_InterviewFeedbacks",
                column: "InterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_InterviewSchedules_ApplicationId",
                schema: "AITStudent",
                table: "erp_InterviewSchedules",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_InterviewSchedules_CandidateId",
                schema: "AITStudent",
                table: "erp_InterviewSchedules",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_InterviewSchedules_InterviewerId",
                schema: "AITStudent",
                table: "erp_InterviewSchedules",
                column: "InterviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_InterviewSchedules_JobId",
                schema: "AITStudent",
                table: "erp_InterviewSchedules",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_JobOpenings_DepartmentId",
                schema: "AITStudent",
                table: "erp_JobOpenings",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_JobOpenings_DesignationId",
                schema: "AITStudent",
                table: "erp_JobOpenings",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_JobOpenings_HiringManagerId",
                schema: "AITStudent",
                table: "erp_JobOpenings",
                column: "HiringManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_JobOpenings_RecruiterId",
                schema: "AITStudent",
                table: "erp_JobOpenings",
                column: "RecruiterId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_KeyResults_OkrId",
                schema: "AITStudent",
                table: "erp_KeyResults",
                column: "OkrId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Kpis_DepartmentId",
                schema: "AITStudent",
                table: "erp_Kpis",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Kpis_DesignationId",
                schema: "AITStudent",
                table: "erp_Kpis",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Kpis_EmployeeId",
                schema: "AITStudent",
                table: "erp_Kpis",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Leads_AssignedExecutiveId",
                schema: "AITStudent",
                table: "erp_Leads",
                column: "AssignedExecutiveId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_MaterialDispatches_SourceWarehouseId",
                schema: "AITStudent",
                table: "erp_MaterialDispatches",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_OfferLetters_ApplicationId",
                schema: "AITStudent",
                table: "erp_OfferLetters",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_OfferLetters_CandidateId",
                schema: "AITStudent",
                table: "erp_OfferLetters",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_OfferLetters_ConvertedToEmployeeId",
                schema: "AITStudent",
                table: "erp_OfferLetters",
                column: "ConvertedToEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_OfferLetters_DepartmentId",
                schema: "AITStudent",
                table: "erp_OfferLetters",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_OfferLetters_DesignationId",
                schema: "AITStudent",
                table: "erp_OfferLetters",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_OfferLetters_JobId",
                schema: "AITStudent",
                table: "erp_OfferLetters",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_OfferLetters_ReportingManagerId",
                schema: "AITStudent",
                table: "erp_OfferLetters",
                column: "ReportingManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Okrs_DepartmentId",
                schema: "AITStudent",
                table: "erp_Okrs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Okrs_DesignationId",
                schema: "AITStudent",
                table: "erp_Okrs",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Okrs_EmployeeId",
                schema: "AITStudent",
                table: "erp_Okrs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_PaymentReceivables_CustomerId",
                schema: "AITStudent",
                table: "erp_PaymentReceivables",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_PayrollRuns_CompanyBankAccountId",
                schema: "AITStudent",
                table: "erp_PayrollRuns",
                column: "CompanyBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_ProcurementCatalogItems_DepartmentId",
                schema: "AITStudent",
                table: "erp_ProcurementCatalogItems",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_ProductSubCategories_CategoryId",
                schema: "AITStudent",
                table: "erp_ProductSubCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_PurchaseRequisitions_CatalogItemId",
                schema: "AITStudent",
                table: "erp_PurchaseRequisitions",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_PurchaseRequisitions_DepartmentId",
                schema: "AITStudent",
                table: "erp_PurchaseRequisitions",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_PurchaseRequisitions_UserId",
                schema: "AITStudent",
                table: "erp_PurchaseRequisitions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalaryStructureMasters_DepartmentId",
                schema: "AITStudent",
                table: "erp_SalaryStructureMasters",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesCoordinatorProfiles_UserUserId",
                schema: "AITStudent",
                table: "erp_SalesCoordinatorProfiles",
                column: "UserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesExecutiveProfiles_AssignedCoordinatorId",
                schema: "AITStudent",
                table: "erp_SalesExecutiveProfiles",
                column: "AssignedCoordinatorId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesExecutiveProfiles_UserUserId",
                schema: "AITStudent",
                table: "erp_SalesExecutiveProfiles",
                column: "UserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesInvoices_CreatedByUserUserId",
                schema: "AITStudent",
                table: "erp_SalesInvoices",
                column: "CreatedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesInvoices_CustomerId",
                schema: "AITStudent",
                table: "erp_SalesInvoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesInvoices_SalesOrderId",
                schema: "AITStudent",
                table: "erp_SalesInvoices",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesLeads_AssignedToUserUserId",
                schema: "AITStudent",
                table: "erp_SalesLeads",
                column: "AssignedToUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesOrderItems_ProductId",
                schema: "AITStudent",
                table: "erp_SalesOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesOrderItems_SalesOrderId",
                schema: "AITStudent",
                table: "erp_SalesOrderItems",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesOrders_CustomerId",
                schema: "AITStudent",
                table: "erp_SalesOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesQuotations_CreatedByUserUserId",
                schema: "AITStudent",
                table: "erp_SalesQuotations",
                column: "CreatedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesReturns_CustomerId",
                schema: "AITStudent",
                table: "erp_SalesReturns",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesTargetAllocations_SalesRepUserUserId",
                schema: "AITStudent",
                table: "erp_SalesTargetAllocations",
                column: "SalesRepUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalesTargets_SalesRepUserUserId",
                schema: "AITStudent",
                table: "erp_SalesTargets",
                column: "SalesRepUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_StockMovementLogs_InventoryItemId",
                schema: "AITStudent",
                table: "erp_StockMovementLogs",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SuperAdminPriceOverrides_ProductId",
                schema: "AITStudent",
                table: "erp_SuperAdminPriceOverrides",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_UomConversions_FromUomId",
                schema: "AITStudent",
                table: "erp_UomConversions",
                column: "FromUomId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_UomConversions_ToUomId",
                schema: "AITStudent",
                table: "erp_UomConversions",
                column: "ToUomId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_UserPermissions_UserId",
                schema: "AITStudent",
                table: "erp_UserPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_WarehouseLocations_SupervisorId",
                schema: "AITStudent",
                table: "erp_WarehouseLocations",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMessages_RecipientUserId",
                schema: "AITStudent",
                table: "InternalMessages",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalMessages_SenderUserId",
                schema: "AITStudent",
                table: "InternalMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeaveTypeId",
                schema: "AITStudent",
                table: "LeaveRequests",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_UserId",
                schema: "AITStudent",
                table: "LeaveRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalarySlips_PayrollRunId",
                schema: "AITStudent",
                table: "SalarySlips",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_SalarySlips_UserId",
                schema: "AITStudent",
                table: "SalarySlips",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StatutoryReturnFilings_CompanyBankAccountId",
                schema: "AITStudent",
                table: "StatutoryReturnFilings",
                column: "CompanyBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemAuditTrails_PerformedByUserUserId",
                schema: "AITStudent",
                table: "SystemAuditTrails",
                column: "PerformedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemNotifications_UserId",
                schema: "AITStudent",
                table: "SystemNotifications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_erp_Users_AssignedRepUserUserId",
                schema: "AITStudent",
                table: "Customers",
                column: "AssignedRepUserUserId",
                principalSchema: "AITStudent",
                principalTable: "erp_Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_Payslips_erp_PayrollRuns_PayrollRunId",
                schema: "AITStudent",
                table: "erp_Payslips",
                column: "PayrollRunId",
                principalSchema: "AITStudent",
                principalTable: "erp_PayrollRuns",
                principalColumn: "PayrollRunId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_Payslips_erp_Users_UserId",
                schema: "AITStudent",
                table: "erp_Payslips",
                column: "UserId",
                principalSchema: "AITStudent",
                principalTable: "erp_Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_Users_erp_Departments_DepartmentId",
                schema: "AITStudent",
                table: "erp_Users",
                column: "DepartmentId",
                principalSchema: "AITStudent",
                principalTable: "erp_Departments",
                principalColumn: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_Users_erp_WorkShifts_ShiftId",
                schema: "AITStudent",
                table: "erp_Users",
                column: "ShiftId",
                principalSchema: "AITStudent",
                principalTable: "erp_WorkShifts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_erp_Users_AssignedRepUserUserId",
                schema: "AITStudent",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_Payslips_erp_PayrollRuns_PayrollRunId",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_Payslips_erp_Users_UserId",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_Users_erp_Departments_DepartmentId",
                schema: "AITStudent",
                table: "erp_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_Users_erp_WorkShifts_ShiftId",
                schema: "AITStudent",
                table: "erp_Users");

            migrationBuilder.DropTable(
                name: "AuditFlaggedItems",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "BankTransactions",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "CompanyHolidays",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_AdminAnnouncements",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_AdminBackupLogs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_AdminBranchHours",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_AdminLoginAudits",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_AdminPasswordResets",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_AllowanceDeductionMasters",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_AuditLogs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_BankReconciliations",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_BonusIncentives",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_CandidateStageHistories",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Contracts",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_CurrencyRateHistories",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_DepartmentTasks",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_DispatchLineItems",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_EmployeeAppraisals",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_EmployeeSalaryAssignments",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_ESSExpenseClaims",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_ESSLeaveApplications",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_ESSPunches",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_ESSSupportTickets",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_ESSTasks",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_ExportAuditLogs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_GrnLineItems",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_HierarchicalTasks",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Holidays",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_HRAttendanceLogs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_HRAttendanceRegularizations",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_HRBiometricDevices",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_HROvertimeRecords",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_HRShiftRosters",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_ImportLogs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_InterviewFeedbacks",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_InvGrns",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_InvScrapWriteOffs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_InvStockAudits",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_InvTransfers",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_InvWarehouses",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_JournalVouchers",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_KeyResults",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Kpis",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Leads",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Offboardings",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_OfferLetters",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Onboardings",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_PaymentReceipts",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_PaymentReceivables",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_PurchaseRequisitions",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Quotations",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_RegionalConfigurations",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalesExecutiveProfiles",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalesInvoices",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalesLeads",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalesOrderItems",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalesQuotations",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalesReturns",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalesTargetAllocations",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalesTargets",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_StatutoryConfigurations",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_StatutoryFilingLogs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_StockMovementLogs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SuperAdminErrorLogs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SuperAdminIntegrations",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SuperAdminMaintenances",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SuperAdminPriceOverrides",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SuperAdminRestorePoints",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_TaxSlabs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Transfers",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_UomConversions",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_UserPermissions",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_WorkShifts",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "InternalMessages",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "LeaveRequests",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "PayrollComponents",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "SalarySlips",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "StatutoryReturnFilings",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "StatutoryRuleConfigs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "SystemAuditTrails",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "SystemMutationLogs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "SystemNotifications",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Currencies",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_MaterialDispatches",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_AppraisalCycles",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalaryStructureMasters",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_BinRackMasters",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_GoodsReceiptNotes",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_InterviewSchedules",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Okrs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_ProcurementCatalogItems",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalesCoordinatorProfiles",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalesOrders",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "LeaveTypes",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_PayrollRuns",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_CatalogItems",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_WarehouseLocations",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_CandidateApplications",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "CompanyBankAccounts",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_ProductSubCategories",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_UnitsOfMeasure",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Candidates",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_JobOpenings",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_ProductCategories",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Designations",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Departments",
                schema: "AITStudent");

            migrationBuilder.DropIndex(
                name: "IX_erp_Users_DepartmentId",
                schema: "AITStudent",
                table: "erp_Users");

            migrationBuilder.DropIndex(
                name: "IX_erp_Users_ShiftId",
                schema: "AITStudent",
                table: "erp_Users");

            migrationBuilder.DropIndex(
                name: "IX_erp_Payslips_PayrollRunId",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropIndex(
                name: "IX_Customers_AssignedRepUserUserId",
                schema: "AITStudent",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BranchName",
                schema: "AITStudent",
                table: "erp_Users");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "AITStudent",
                table: "erp_Users");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                schema: "AITStudent",
                table: "erp_Users");

            migrationBuilder.DropColumn(
                name: "JoiningDate",
                schema: "AITStudent",
                table: "erp_Users");

            migrationBuilder.DropColumn(
                name: "ReportingManagerId",
                schema: "AITStudent",
                table: "erp_Users");

            migrationBuilder.DropColumn(
                name: "ReportingManagerName",
                schema: "AITStudent",
                table: "erp_Users");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                schema: "AITStudent",
                table: "erp_Users");

            migrationBuilder.DropColumn(
                name: "CanApprove",
                schema: "AITStudent",
                table: "erp_RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanCreate",
                schema: "AITStudent",
                table: "erp_RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanDelete",
                schema: "AITStudent",
                table: "erp_RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanEdit",
                schema: "AITStudent",
                table: "erp_RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanView",
                schema: "AITStudent",
                table: "erp_RolePermissions");

            migrationBuilder.DropColumn(
                name: "GstRate",
                schema: "AITStudent",
                table: "erp_Products");

            migrationBuilder.DropColumn(
                name: "ReservedStockQty",
                schema: "AITStudent",
                table: "erp_Products");

            migrationBuilder.DropColumn(
                name: "UnitSellingPrice",
                schema: "AITStudent",
                table: "erp_Products");

            migrationBuilder.DropColumn(
                name: "AbsentDays",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "BonusIncentives",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "ESI",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "EmployerESI",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "EmployerPF",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "GrossSalary",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "LOPDeduction",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "LTA",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "OtherAllowance",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "OvertimeHours",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "OvertimePay",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "PaidLeaveDays",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "PayrollRunId",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "PayslipNumber",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "PresentDays",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "SpecialAllowance",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "TDS",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "TotalCTC",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "TotalDeductions",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "TotalWorkingDays",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "UnpaidLeaveDays",
                schema: "AITStudent",
                table: "erp_Payslips");

            migrationBuilder.DropColumn(
                name: "AssignedRepId",
                schema: "AITStudent",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AssignedRepUserUserId",
                schema: "AITStudent",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                schema: "AITStudent",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "AITStudent",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                schema: "AITStudent",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerCode",
                schema: "AITStudent",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                schema: "AITStudent",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OutstandingBalance",
                schema: "AITStudent",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TaxIdOrGSTIN",
                schema: "AITStudent",
                table: "Customers");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TransportAllowance",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ProvidentFund",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ProfessionalTax",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PayPeriod",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaidDays",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "NetSalary",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MedicalAllowance",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HRA",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BasicSalary",
                schema: "AITStudent",
                table: "erp_Payslips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BranchName",
                schema: "AITStudent",
                table: "erp_Branches",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "AITStudent",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 10, 1, 57, 68, DateTimeKind.Utc).AddTicks(7456));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 9, 48, 57, 68, DateTimeKind.Utc).AddTicks(7462));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 9, 3, 57, 68, DateTimeKind.Utc).AddTicks(7465));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 8, 3, 57, 68, DateTimeKind.Utc).AddTicks(7469));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 7, 3, 57, 68, DateTimeKind.Utc).AddTicks(7471));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEJgsYLPH0PAL4ZhW35nussL+5r0VUOlgW0oRnGrbOjUE1BzmOwVydsy/S4kpZF+scw==");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_Payslips_erp_Users_UserId",
                schema: "AITStudent",
                table: "erp_Payslips",
                column: "UserId",
                principalSchema: "AITStudent",
                principalTable: "erp_Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
