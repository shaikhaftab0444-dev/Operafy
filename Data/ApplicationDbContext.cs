using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ERP_System.Models;
using System;
using ERP_System.Controllers;

namespace ERP_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<SalaryStructure> SalaryStructures { get; set; }
        public DbSet<Payslip> Payslips { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<FinancialYear> FinancialYears { get; set; }
        public DbSet<AccountHead> AccountHeads { get; set; }
        public DbSet<StockAdjustment> StockAdjustments { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<RegionalConfiguration> RegionalConfigurations { get; set; }
        public DbSet<ImportLog> ImportLogs { get; set; }
        public DbSet<ExportAuditLog> ExportAuditLogs { get; set; }
        public DbSet<AuditLogEntry> AuditLogs { get; set; }
        public DbSet<Lead> Leads { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesReturn> SalesReturns { get; set; }
        public DbSet<PaymentReceipt> PaymentReceipts { get; set; }
        public DbSet<HROnboarding> Onboardings { get; set; }
        public DbSet<HRContract> Contracts { get; set; }
        public DbSet<HRTransfer> Transfers { get; set; }
        public DbSet<HROffboarding> Offboardings { get; set; }
        public DbSet<HRHoliday> Holidays { get; set; }
        public DbSet<ESSPunch> ESSPunches { get; set; }
        public DbSet<ESSLeaveApplication> ESSLeaveApplications { get; set; }
        public DbSet<ESSTask> ESSTasks { get; set; }
        public DbSet<ESSExpenseClaim> ESSExpenseClaims { get; set; }
        public DbSet<ESSSupportTicket> ESSSupportTickets { get; set; }
        public DbSet<InvWarehouse> InvWarehouses { get; set; }
        public DbSet<InvGrn> InvGrns { get; set; }
        public DbSet<InvTransfer> InvTransfers { get; set; }
        public DbSet<InvStockAudit> InvStockAudits { get; set; }
        public DbSet<InvScrapWriteOff> InvScrapWriteOffs { get; set; }
        public DbSet<AdminPasswordReset> AdminPasswordResets { get; set; }
        public DbSet<AdminBranchHour> AdminBranchHours { get; set; }
        public DbSet<AdminBackupLog> AdminBackupLogs { get; set; }
        public DbSet<AdminLoginAudit> AdminLoginAudits { get; set; }
        public DbSet<AdminAnnouncement> AdminAnnouncements { get; set; }
        public DbSet<SuperAdminErrorLog> SuperAdminErrorLogs { get; set; }
        public DbSet<SuperAdminMaintenance> SuperAdminMaintenances { get; set; }
        public DbSet<SuperAdminIntegration> SuperAdminIntegrations { get; set; }
        public DbSet<SuperAdminRestorePoint> SuperAdminRestorePoints { get; set; }
        public DbSet<SuperAdminPriceOverride> SuperAdminPriceOverrides { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<CurrencyRateHistory> CurrencyRateHistories { get; set; }
        public DbSet<TaxSlab> TaxSlabs { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<HRAttendanceLog> HRAttendanceLogs { get; set; }
        public DbSet<HRBiometricDevice> HRBiometricDevices { get; set; }
        public DbSet<HRShiftRoster> HRShiftRosters { get; set; }
        public DbSet<HROvertimeRecord> HROvertimeRecords { get; set; }
        public DbSet<HRAttendanceRegularization> HRAttendanceRegularizations { get; set; }

        public DbSet<SalaryStructureMaster> SalaryStructureMasters { get; set; }
        public DbSet<EmployeeSalaryAssignment> EmployeeSalaryAssignments { get; set; }
        public DbSet<AllowanceDeductionMaster> AllowanceDeductionMasters { get; set; }
        public DbSet<StatutoryConfiguration> StatutoryConfigurations { get; set; }
        public DbSet<StatutoryFilingLog> StatutoryFilingLogs { get; set; }
        public DbSet<BonusIncentive> BonusIncentives { get; set; }
        public DbSet<PayrollRun> PayrollRuns { get; set; }

        // ATS & Recruitment DbSets
        public DbSet<JobOpening> JobOpenings { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<CandidateApplication> CandidateApplications { get; set; }
        public DbSet<CandidateStageHistory> CandidateStageHistories { get; set; }
        public DbSet<InterviewSchedule> InterviewSchedules { get; set; }
        public DbSet<InterviewFeedback> InterviewFeedbacks { get; set; }
        public DbSet<OfferLetter> OfferLetters { get; set; }

        // Performance & Appraisals DbSets
        public DbSet<OkrObjective> OkrObjectives { get; set; }
        public DbSet<KeyResult> KeyResults { get; set; }
        public DbSet<KpiItem> Kpis { get; set; }
        public DbSet<AppraisalCycle> AppraisalCycles { get; set; }
        public DbSet<EmployeeAppraisal> EmployeeAppraisals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure default schema to match your database
            modelBuilder.HasDefaultSchema("AITStudent");

            // Map entities to table names
            modelBuilder.Entity<Role>().ToTable("erp_Roles");
            modelBuilder.Entity<User>().ToTable("erp_Users");
            modelBuilder.Entity<Transaction>().ToTable("erp_Transactions");
            modelBuilder.Entity<Product>().ToTable("erp_Products");
            modelBuilder.Entity<ActivityLog>().ToTable("erp_ActivityLogs");
            modelBuilder.Entity<Company>().ToTable("erp_Companies");
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<SalaryStructure>().ToTable("erp_SalaryStructures");
            modelBuilder.Entity<Payslip>().ToTable("erp_Payslips");
            modelBuilder.Entity<Branch>().ToTable("erp_Branches");
            modelBuilder.Entity<Supplier>().ToTable("erp_Suppliers");
            modelBuilder.Entity<FinancialYear>().ToTable("erp_FinancialYears");
            modelBuilder.Entity<AccountHead>().ToTable("erp_AccountHeads");
            modelBuilder.Entity<RolePermission>().ToTable("erp_RolePermissions");
            modelBuilder.Entity<StockAdjustment>().ToTable("erp_StockAdjustments");
            modelBuilder.Entity<Lead>().ToTable("erp_Leads");
            modelBuilder.Entity<Quotation>().ToTable("erp_Quotations");
            modelBuilder.Entity<SalesOrder>().ToTable("erp_SalesOrders");
            modelBuilder.Entity<SalesReturn>().ToTable("erp_SalesReturns");
            modelBuilder.Entity<PaymentReceipt>().ToTable("erp_PaymentReceipts");
            modelBuilder.Entity<HROnboarding>().ToTable("erp_Onboardings");
            modelBuilder.Entity<HRContract>().ToTable("erp_Contracts");
            modelBuilder.Entity<HRTransfer>().ToTable("erp_Transfers");
            modelBuilder.Entity<HROffboarding>().ToTable("erp_Offboardings");
            modelBuilder.Entity<HRHoliday>().ToTable("erp_Holidays");
            modelBuilder.Entity<HRAttendanceLog>().ToTable("erp_HRAttendanceLogs");
            modelBuilder.Entity<HRBiometricDevice>().ToTable("erp_HRBiometricDevices");
            modelBuilder.Entity<HRShiftRoster>().ToTable("erp_HRShiftRosters");
            modelBuilder.Entity<HROvertimeRecord>().ToTable("erp_HROvertimeRecords");
            modelBuilder.Entity<HRAttendanceRegularization>().ToTable("erp_HRAttendanceRegularizations");
            modelBuilder.Entity<Designation>().ToTable("erp_Designations");
            modelBuilder.Entity<RegionalConfiguration>().ToTable("erp_RegionalConfigurations");
            modelBuilder.Entity<ImportLog>().ToTable("erp_ImportLogs");
            modelBuilder.Entity<ExportAuditLog>().ToTable("erp_ExportAuditLogs");
            modelBuilder.Entity<AuditLogEntry>().ToTable("erp_AuditLogs");

            modelBuilder.Entity<SalaryStructureMaster>().ToTable("erp_SalaryStructureMasters");
            modelBuilder.Entity<EmployeeSalaryAssignment>().ToTable("erp_EmployeeSalaryAssignments");
            modelBuilder.Entity<AllowanceDeductionMaster>().ToTable("erp_AllowanceDeductionMasters");
            modelBuilder.Entity<StatutoryConfiguration>().ToTable("erp_StatutoryConfigurations");
            modelBuilder.Entity<StatutoryFilingLog>().ToTable("erp_StatutoryFilingLogs");
            modelBuilder.Entity<BonusIncentive>().ToTable("erp_BonusIncentives");
            modelBuilder.Entity<PayrollRun>().ToTable("erp_PayrollRuns");

            modelBuilder.Entity<JobOpening>().ToTable("erp_JobOpenings");
            modelBuilder.Entity<Candidate>().ToTable("erp_Candidates");
            modelBuilder.Entity<CandidateApplication>().ToTable("erp_CandidateApplications");
            modelBuilder.Entity<CandidateStageHistory>().ToTable("erp_CandidateStageHistories");
            modelBuilder.Entity<InterviewSchedule>().ToTable("erp_InterviewSchedules");
            modelBuilder.Entity<InterviewFeedback>().ToTable("erp_InterviewFeedbacks");
            modelBuilder.Entity<OfferLetter>().ToTable("erp_OfferLetters");

            modelBuilder.Entity<OkrObjective>().ToTable("erp_Okrs");
            modelBuilder.Entity<KeyResult>().ToTable("erp_KeyResults");
            modelBuilder.Entity<KpiItem>().ToTable("erp_Kpis");
            modelBuilder.Entity<AppraisalCycle>().ToTable("erp_AppraisalCycles");
            modelBuilder.Entity<EmployeeAppraisal>().ToTable("erp_EmployeeAppraisals");

            // Seed Admin User (Using Identity Password Hasher)
            var hasher = new PasswordHasher<User>();
            var adminUser = new User
            {
                UserId = 1,
                CompanyId = 1,                // Mapped to existing AIT Technologies Pvt Ltd
                BranchId = 3,                 // Mapped to existing Head Office branch
                UserCode = "USR001",
                UserName = "admin",
                FullName = "Admin User",
                Email = "admin@erp.com",
                RoleId = 1,                   // Super Admin (exists in erp_Roles as RoleId 1)
                IsActive = true,
                CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483")
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Monitor@2026");

            modelBuilder.Entity<User>().HasData(adminUser);

            // Seed Products (For "Top Selling Products" dashboard table)
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, ProductName = "Laptop", SoldQty = 45, Revenue = 450000, StockQty = 180, Status = "In Stock" },
                new Product { ProductId = 2, ProductName = "Smartphone", SoldQty = 85, Revenue = 340000, StockQty = 60, Status = "Low Stock" },
                new Product { ProductId = 3, ProductName = "Headphones", SoldQty = 120, Revenue = 180000, StockQty = 40, Status = "Out of Stock" },
                new Product { ProductId = 4, ProductName = "Keyboard", SoldQty = 60, Revenue = 90000, StockQty = 100, Status = "In Stock" },
                new Product { ProductId = 5, ProductName = "Mouse", SoldQty = 75, Revenue = 75000, StockQty = 120, Status = "In Stock" }
            );

            // Seed Transactions (For "Recent Transactions" dashboard table)
            modelBuilder.Entity<Transaction>().HasData(
                new Transaction { TransactionId = 1, TransactionNo = "INV-10045", Type = "Sales Invoice", Date = DateTime.Parse("2026-05-31"), PartyName = "Rahul Enterprises", Amount = 25000, Status = "Paid" },
                new Transaction { TransactionId = 2, TransactionNo = "PO-10023", Type = "Purchase Order", Date = DateTime.Parse("2026-05-31"), PartyName = "Sharma Suppliers", Amount = 18500, Status = "Pending" },
                new Transaction { TransactionId = 3, TransactionNo = "INV-10044", Type = "Sales Invoice", Date = DateTime.Parse("2026-05-30"), PartyName = "ABC Corporation", Amount = 15750, Status = "Paid" },
                new Transaction { TransactionId = 4, TransactionNo = "EXP-10012", Type = "Expense Entry", Date = DateTime.Parse("2026-05-30"), PartyName = "Office Supplies", Amount = 2500, Status = "Paid" },
                new Transaction { TransactionId = 5, TransactionNo = "PO-10022", Type = "Purchase Order", Date = DateTime.Parse("2026-05-29"), PartyName = "XYZ Traders", Amount = 22000, Status = "Pending" }
            );

            // Seed Activity Logs (For "Recent Activities" dashboard log)
            modelBuilder.Entity<ActivityLog>().HasData(
                new ActivityLog { ActivityLogId = 1, Title = "New Sales Invoice", Description = "INV-10045 created", CreatedAt = DateTime.UtcNow.AddMinutes(-2), IconClass = "fa-file-invoice", ColorClass = "text-primary" },
                new ActivityLog { ActivityLogId = 2, Title = "New Purchase Order", Description = "PO-10023 created", CreatedAt = DateTime.UtcNow.AddMinutes(-15), IconClass = "fa-shopping-cart", ColorClass = "text-success" },
                new ActivityLog { ActivityLogId = 3, Title = "New Employee Added", Description = "John Doe added", CreatedAt = DateTime.UtcNow.AddHours(-1), IconClass = "fa-user-plus", ColorClass = "text-info" },
                new ActivityLog { ActivityLogId = 4, Title = "Payment Received", Description = "₹25,000 received", CreatedAt = DateTime.UtcNow.AddHours(-2), IconClass = "fa-hand-holding-usd", ColorClass = "text-warning" },
                new ActivityLog { ActivityLogId = 5, Title = "Stock Updated", Description = "Product stock updated", CreatedAt = DateTime.UtcNow.AddHours(-3), IconClass = "fa-boxes", ColorClass = "text-danger" }
            );
        }
    }
}