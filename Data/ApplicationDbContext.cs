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
        public DbSet<UserPermission> UserPermissions { get; set; }
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
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<CompanyHoliday> CompanyHolidays { get; set; }
        public DbSet<ESSLeaveApplication> ESSLeaveRequests { get { return ESSLeaveApplications; } set { ESSLeaveApplications = value; } }
        public DbSet<HRAttendanceRegularization> AttendanceRegularizations { get { return HRAttendanceRegularizations; } set { HRAttendanceRegularizations = value; } }
        public DbSet<HRAttendanceRegularization> AttendanceRegularizationRequests { get { return HRAttendanceRegularizations; } set { HRAttendanceRegularizations = value; } }
        public DbSet<ESSExpenseClaim> ExpenseClaims { get { return ESSExpenseClaims; } set { ESSExpenseClaims = value; } }
        public DbSet<ESSSupportTicket> SupportTickets { get { return ESSSupportTickets; } set { ESSSupportTickets = value; } }
        public DbSet<HRAttendanceLog> AttendanceLogs { get { return HRAttendanceLogs; } set { HRAttendanceLogs = value; } }
        public DbSet<SalesTarget> SalesTargets { get; set; }
        public DbSet<SalesLead> SalesLeads { get; set; }
        public DbSet<SalesQuotation> SalesQuotations { get; set; }
        public DbSet<SalesInvoice> SalesInvoices { get; set; }
        public DbSet<PaymentReceivable> PaymentReceivables { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }
        public DbSet<SalesExecutiveProfile> SalesExecutiveProfiles { get; set; }
        public DbSet<SalesCoordinatorProfile> SalesCoordinatorProfiles { get; set; }
        public DbSet<SalesTargetAllocation> SalesTargetAllocations { get; set; }
        public DbSet<HierarchicalTask> HierarchicalTasks { get; set; }
        public DbSet<HierarchicalTask> Tasks { get => HierarchicalTasks; set => HierarchicalTasks = value; }
        public DbSet<DepartmentTask> DepartmentTasks { get; set; }
        public DbSet<WorkShift> WorkShifts { get; set; }

        public DbSet<SalaryStructureMaster> SalaryStructureMasters { get; set; }
        public DbSet<SalaryStructureMaster> SalaryStructureTemplates { get => SalaryStructureMasters; set => SalaryStructureMasters = value; }
        public DbSet<EmployeeSalaryAssignment> EmployeeSalaryAssignments { get; set; }
        public DbSet<AllowanceDeductionMaster> AllowanceDeductionMasters { get; set; }
        public DbSet<PayrollComponent> PayrollComponents { get; set; }
        public DbSet<StatutoryConfiguration> StatutoryConfigurations { get; set; }
        public DbSet<StatutoryFilingLog> StatutoryFilingLogs { get; set; }
        public DbSet<StatutoryReturnFiling> StatutoryReturnFilings { get; set; }
        public DbSet<StatutoryRuleConfig> StatutoryRuleConfigs { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }
        public DbSet<BonusIncentive> BonusIncentives { get; set; }
        public DbSet<PayrollRun> PayrollRuns { get; set; }
        public DbSet<SalarySlip> SalarySlips { get; set; }
        public DbSet<InternalMessage> InternalMessages { get; set; }
        public DbSet<SystemNotification> SystemNotifications { get; set; }

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

        // Auditor Command Center DbSets
        public DbSet<SystemAuditTrail> SystemAuditTrails { get; set; }
        public DbSet<AuditFlaggedItem> AuditFlaggedItems { get; set; }
        public DbSet<SystemMutationLog> SystemMutationLogs { get; set; }

        // Accountant Hub DbSets
        public DbSet<JournalVoucher> JournalVouchers { get; set; }
        public DbSet<BankReconciliationItem> BankReconciliations { get; set; }
        // Company Bank Master
        public DbSet<CompanyBankAccount> CompanyBankAccounts { get; set; }

        // Procurement Catalog & Purchase Requisitions
        public DbSet<ProcurementCatalogItem> ProcurementCatalogItems { get; set; }
        public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }

        // Product & Catalog Master DbSets
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductSubCategory> ProductSubCategories { get; set; }
        public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
        public DbSet<UomConversion> UomConversions { get; set; }
        public DbSet<CatalogItem> CatalogItems { get; set; }

        // Warehouse Operations DbSets
        public DbSet<WarehouseLocation> WarehouseLocations { get; set; }
        public DbSet<BinRackMaster> BinRackMasters { get; set; }
        public DbSet<GoodsReceiptNote> GoodsReceiptNotes { get; set; }
        public DbSet<GrnLineItem> GrnLineItems { get; set; }
        public DbSet<MaterialDispatch> MaterialDispatches { get; set; }
        public DbSet<DispatchLineItem> DispatchLineItems { get; set; }
        public DbSet<StockMovementLog> StockMovementLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure default schema to match your database
            modelBuilder.HasDefaultSchema("AITStudent");

            // Explicitly ignore ApplicationUser so EF Core treats User as a standalone entity without TPH Discriminator
            modelBuilder.Ignore<ApplicationUser>();

            // Product & Catalog Master Mappings
            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("erp_ProductCategories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DefaultGstRate).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<ProductSubCategory>(entity =>
            {
                entity.ToTable("erp_ProductSubCategories");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UnitOfMeasure>(entity =>
            {
                entity.ToTable("erp_UnitsOfMeasure");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<UomConversion>(entity =>
            {
                entity.ToTable("erp_UomConversions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ConversionFactor).HasColumnType("decimal(18,4)");
                entity.HasOne(e => e.FromUom)
                      .WithMany()
                      .HasForeignKey(e => e.FromUomId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ToUom)
                      .WithMany()
                      .HasForeignKey(e => e.ToUomId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CatalogItem>(entity =>
            {
                entity.ToTable("erp_CatalogItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SellingPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Items)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.SubCategory)
                      .WithMany(s => s.Items)
                      .HasForeignKey(e => e.SubCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Uom)
                      .WithMany()
                      .HasForeignKey(e => e.UomId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Warehouse Operations Mappings
            modelBuilder.Entity<WarehouseLocation>(entity =>
            {
                entity.ToTable("erp_WarehouseLocations");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Supervisor)
                      .WithMany()
                      .HasForeignKey(e => e.SupervisorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BinRackMaster>(entity =>
            {
                entity.ToTable("erp_BinRackMasters");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Warehouse)
                      .WithMany(w => w.StoragePositions)
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.AssignedItem)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedCatalogItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GoodsReceiptNote>(entity =>
            {
                entity.ToTable("erp_GoodsReceiptNotes");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.DestinationWarehouse)
                      .WithMany()
                      .HasForeignKey(e => e.DestinationWarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ReceivedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ReceivedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GrnLineItem>(entity =>
            {
                entity.ToTable("erp_GrnLineItems");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Grn)
                      .WithMany(g => g.LineItems)
                      .HasForeignKey(e => e.GrnId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CatalogItem)
                      .WithMany()
                      .HasForeignKey(e => e.CatalogItemId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.TargetBin)
                      .WithMany()
                      .HasForeignKey(e => e.TargetBinId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MaterialDispatch>(entity =>
            {
                entity.ToTable("erp_MaterialDispatches");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.SourceWarehouse)
                      .WithMany()
                      .HasForeignKey(e => e.SourceWarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DispatchLineItem>(entity =>
            {
                entity.ToTable("erp_DispatchLineItems");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Dispatch)
                      .WithMany(d => d.LineItems)
                      .HasForeignKey(e => e.DispatchId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CatalogItem)
                      .WithMany()
                      .HasForeignKey(e => e.CatalogItemId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.PickedFromBin)
                      .WithMany()
                      .HasForeignKey(e => e.PickedFromBinId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StockMovementLog>(entity =>
            {
                entity.ToTable("erp_StockMovementLogs");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.InventoryItem)
                      .WithMany()
                      .HasForeignKey(e => e.InventoryItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Map entities to table names
            modelBuilder.Entity<ProcurementCatalogItem>().ToTable("erp_ProcurementCatalogItems");
            modelBuilder.Entity<PurchaseRequisition>().ToTable("erp_PurchaseRequisitions");
            modelBuilder.Entity<Role>().ToTable("erp_Roles");
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("erp_Users");
                entity.HasNoDiscriminator();
            });
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
            modelBuilder.Entity<UserPermission>().ToTable("erp_UserPermissions");
            modelBuilder.Entity<StockAdjustment>().ToTable("erp_StockAdjustments");
            modelBuilder.Entity<Lead>().ToTable("erp_Leads");
            modelBuilder.Entity<Quotation>().ToTable("erp_Quotations");
            modelBuilder.Entity<SalesOrder>().ToTable("erp_SalesOrders");
            modelBuilder.Entity<SalesReturn>().ToTable("erp_SalesReturns");
            modelBuilder.Entity<PaymentReceipt>().ToTable("erp_PaymentReceipts");
            modelBuilder.Entity<SalesCoordinatorProfile>(entity =>
            {
                entity.ToTable("erp_SalesCoordinatorProfiles");
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<SalesExecutiveProfile>(entity =>
            {
                entity.ToTable("erp_SalesExecutiveProfiles");
                entity.Property(e => e.MonthlyTarget).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CommissionPercentage).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.AssignedCoordinator)
                      .WithMany(c => c.AssignedExecutives)
                      .HasForeignKey(e => e.AssignedCoordinatorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            modelBuilder.Entity<SalesTargetAllocation>(entity =>
            {
                entity.ToTable("erp_SalesTargetAllocations");
                entity.Property(t => t.TargetValue).HasColumnType("decimal(18,2)");
                entity.Property(t => t.AchievedValue).HasColumnType("decimal(18,2)");
                entity.HasOne(t => t.SalesRep)
                      .WithMany()
                      .HasForeignKey(t => t.SalesRepUserUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<HROnboarding>().ToTable("erp_Onboardings");
            modelBuilder.Entity<HRContract>().ToTable("erp_Contracts");
            modelBuilder.Entity<HRTransfer>().ToTable("erp_Transfers");
            modelBuilder.Entity<HROffboarding>().ToTable("erp_Offboardings");
            modelBuilder.Entity<HRHoliday>().ToTable("erp_Holidays");
            modelBuilder.Entity<CompanyHoliday>().ToTable("CompanyHolidays");
            modelBuilder.Entity<LeaveType>().ToTable("LeaveTypes");
            modelBuilder.Entity<LeaveRequest>().ToTable("LeaveRequests");
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
            modelBuilder.Entity<Lead>(entity =>
            {
                entity.ToTable("erp_Leads");
                entity.HasKey(e => e.LeadId);
                entity.Property(e => e.LeadId).HasColumnName("LeadId");
            });

            modelBuilder.Entity<SalesOrder>(entity =>
            {
                entity.ToTable("erp_SalesOrders");
                entity.HasKey(e => e.SalesOrderId);
                entity.Property(e => e.SalesOrderId).HasColumnName("SalesOrderId");
            });

            modelBuilder.Entity<SalesTarget>().ToTable("erp_SalesTargets");
            modelBuilder.Entity<SalesLead>().ToTable("erp_SalesLeads");
            modelBuilder.Entity<SalesQuotation>().ToTable("erp_SalesQuotations");
            modelBuilder.Entity<SalesInvoice>().ToTable("erp_SalesInvoices");

            modelBuilder.Entity<PaymentReceivable>(entity =>
            {
                entity.ToTable("erp_PaymentReceivables");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PendingBalance).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SalesOrderItem>(entity =>
            {
                entity.ToTable("erp_SalesOrderItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.SalesOrder)
                      .WithMany(o => o.Items)
                      .HasForeignKey(e => e.SalesOrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<HierarchicalTask>().ToTable("erp_HierarchicalTasks");
            modelBuilder.Entity<JournalVoucher>().ToTable("erp_JournalVouchers");
            modelBuilder.Entity<BankReconciliationItem>().ToTable("erp_BankReconciliations");
            modelBuilder.Entity<SystemMutationLog>().ToTable("SystemMutationLogs");

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