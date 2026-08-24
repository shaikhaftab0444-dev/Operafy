using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;

namespace ERP_System.Data
{
    public static class SeedData
    {
        public static async Task InitializePermissionsAsync(ApplicationDbContext context)
        {
            // If permissions are already set up, do not seed again
            if (await context.RolePermissions.AnyAsync())
            {
                return;
            }

            var roles = await context.Roles.ToListAsync();
            var modules = new[]
            {
                "UserManagement", "Company", "BranchManagement", "EmployeeManagement", "CustomerManagement",
                "SupplierManagement", "ProductManagement", "InventoryManagement", "PurchaseManagement", "SalesManagement",
                "ExpenseManagement", "Accounting", "HRManagement", "Reports", "Settings"
            };

            var permissionsToSeed = new List<RolePermission>();

            foreach (var role in roles)
            {
                foreach (var mod in modules)
                {
                    bool isAllowed = false;

                    // Super Admin and Admin have access to everything by default
                    if (role.RoleName == "Super Admin" || role.RoleName == "Admin")
                    {
                        isAllowed = true;
                    }
                    else if (role.RoleName == "HR")
                    {
                        isAllowed = (mod == "Company" || mod == "EmployeeManagement" || mod == "HRManagement" || mod == "Reports");
                    }
                    else if (role.RoleName == "Manager")
                    {
                        isAllowed = (mod == "Company" || mod == "EmployeeManagement" || mod == "ProductManagement" || mod == "Reports");
                    }
                    else if (role.RoleName == "Employee")
                    {
                        isAllowed = (mod == "EmployeeManagement");
                    }
                    else if (role.RoleName == "Accountant")
                    {
                        isAllowed = (mod == "SalesManagement" || mod == "ExpenseManagement" || mod == "Accounting" || mod == "Reports");
                    }
                    else if (role.RoleName == "Finance Manager")
                    {
                        isAllowed = (mod == "PurchaseManagement" || mod == "SalesManagement" || mod == "ExpenseManagement" || mod == "Accounting" || mod == "Reports");
                    }
                    else if (role.RoleName == "Inventory Manager")
                    {
                        isAllowed = (mod == "SupplierManagement" || mod == "ProductManagement" || mod == "InventoryManagement");
                    }
                    else if (role.RoleName == "Purchase Manager")
                    {
                        isAllowed = (mod == "SupplierManagement" || mod == "InventoryManagement" || mod == "PurchaseManagement");
                    }
                    else if (role.RoleName == "Sales Executive")
                    {
                        isAllowed = (mod == "CustomerManagement" || mod == "ProductManagement" || mod == "SalesManagement");
                    }
                    else if (role.RoleName == "Sales Manager")
                    {
                        isAllowed = (mod == "CustomerManagement" || mod == "ProductManagement" || mod == "SalesManagement" || mod == "Reports");
                    }
                    else if (role.RoleName == "Auditor")
                    {
                        isAllowed = (mod == "Accounting" || mod == "Reports" || mod == "Settings");
                    }

                    permissionsToSeed.Add(new RolePermission
                    {
                        RoleId = role.RoleId,
                        ModuleName = mod,
                        IsAllowed = isAllowed
                    });
                }
            }

            await context.RolePermissions.AddRangeAsync(permissionsToSeed);
            await context.SaveChangesAsync();
        }

        public static async Task InitializeSalesManagementTablesAsync(ApplicationDbContext context)
        {
            // Ensure erp_Leads table exists
            string createLeadsSql = @"
                IF OBJECT_ID('AITStudent.erp_Leads', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Leads (
                        LeadId INT IDENTITY(1,1) PRIMARY KEY,
                        ContactName NVARCHAR(100) NOT NULL,
                        Company NVARCHAR(100) NOT NULL,
                        Source NVARCHAR(100) NOT NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
                        Status NVARCHAR(50) NOT NULL DEFAULT 'New'
                    );
                END";

            // Ensure erp_Quotations table exists
            string createQuotationsSql = @"
                IF OBJECT_ID('AITStudent.erp_Quotations', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Quotations (
                        QuotationId INT IDENTITY(1,1) PRIMARY KEY,
                        QuoteNo NVARCHAR(50) NOT NULL,
                        CustomerName NVARCHAR(100) NOT NULL,
                        ExpiryDate DATETIME NOT NULL,
                        EstimatedAmount DECIMAL(18,2) NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Draft'
                    );
                END";

            // Ensure erp_SalesOrders table exists
            string createOrdersSql = @"
                IF OBJECT_ID('AITStudent.erp_SalesOrders', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_SalesOrders (
                        SalesOrderId INT IDENTITY(1,1) PRIMARY KEY,
                        OrderNo NVARCHAR(50) NOT NULL,
                        CustomerName NVARCHAR(100) NOT NULL,
                        OrderDate DATETIME NOT NULL,
                        OrderTotal DECIMAL(18,2) NOT NULL,
                        DeliveryStatus NVARCHAR(50) NOT NULL DEFAULT 'Processing'
                    );
                END";

            // Ensure erp_SalesReturns table exists
            string createReturnsSql = @"
                IF OBJECT_ID('AITStudent.erp_SalesReturns', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_SalesReturns (
                        SalesReturnId INT IDENTITY(1,1) PRIMARY KEY,
                        ReturnNo NVARCHAR(50) NOT NULL,
                        OriginalInvoiceNo NVARCHAR(50) NOT NULL,
                        CustomerName NVARCHAR(100) NOT NULL,
                        ReturnDate DATETIME NOT NULL,
                        RefundValue DECIMAL(18,2) NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Inspecting'
                    );
                END";

            // Ensure erp_PaymentReceipts table exists
            string createReceiptsSql = @"
                IF OBJECT_ID('AITStudent.erp_PaymentReceipts', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_PaymentReceipts (
                        PaymentReceiptId INT IDENTITY(1,1) PRIMARY KEY,
                        InvoiceNo NVARCHAR(50) NOT NULL,
                        CustomerName NVARCHAR(100) NOT NULL,
                        InvoiceDate DATETIME NOT NULL,
                        DueDate DATETIME NOT NULL,
                        PendingBalance DECIMAL(18,2) NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
                    );
                END";

            // Execute scripts
            await context.Database.ExecuteSqlRawAsync(createLeadsSql);
            await context.Database.ExecuteSqlRawAsync(createQuotationsSql);
            await context.Database.ExecuteSqlRawAsync(createOrdersSql);
            await context.Database.ExecuteSqlRawAsync(createReturnsSql);
            await context.Database.ExecuteSqlRawAsync(createReceiptsSql);

            // Seed initial records if empty
            if (!await context.Leads.AnyAsync())
            {
                await context.Leads.AddRangeAsync(new List<Lead>
                {
                    new Lead { ContactName = "Rajesh Kumar", Company = "Rajesh Corp Ltd", Source = "Website Referral", CreatedAt = DateTime.Parse("2026-08-20"), Status = "New" },
                    new Lead { ContactName = "Sarah Jenkins", Company = "Jenkins Q2 Auditing", Source = "Cold Email", CreatedAt = DateTime.Parse("2026-08-19"), Status = "Contacted" },
                    new Lead { ContactName = "Sunil Mehta", Company = "Mehta Logistics", Source = "Direct Call", CreatedAt = DateTime.Parse("2026-08-18"), Status = "Qualified" },
                    new Lead { ContactName = "Vikram Singh", Company = "Singh & Sons", Source = "LinkedIn", CreatedAt = DateTime.Parse("2026-08-15"), Status = "Lost" }
                });
            }

            if (!await context.Quotations.AnyAsync())
            {
                await context.Quotations.AddRangeAsync(new List<Quotation>
                {
                    new Quotation { QuoteNo = "QTN-5011", CustomerName = "Rajesh Corp Ltd", ExpiryDate = DateTime.Parse("2026-08-30"), EstimatedAmount = 150000, Status = "Approved" },
                    new Quotation { QuoteNo = "QTN-5012", CustomerName = "Mehta Logistics", ExpiryDate = DateTime.Parse("2026-09-15"), EstimatedAmount = 320000, Status = "Sent" },
                    new Quotation { QuoteNo = "QTN-5013", CustomerName = "Global Agencies Ltd", ExpiryDate = DateTime.Parse("2026-08-25"), EstimatedAmount = 75000, Status = "Draft" }
                });
            }

            if (!await context.SalesOrders.AnyAsync())
            {
                await context.SalesOrders.AddRangeAsync(new List<SalesOrder>
                {
                    new SalesOrder { OrderNo = "SO-9041", CustomerName = "Rahul Enterprises", OrderDate = DateTime.Parse("2026-08-18"), OrderTotal = 180000, DeliveryStatus = "Dispatched" },
                    new SalesOrder { OrderNo = "SO-9042", CustomerName = "Sunil Mehta", OrderDate = DateTime.Parse("2026-08-19"), OrderTotal = 425000, DeliveryStatus = "Processing" }
                });
            }

            if (!await context.SalesReturns.AnyAsync())
            {
                await context.SalesReturns.AddRangeAsync(new List<SalesReturn>
                {
                    new SalesReturn { ReturnNo = "SR-7001", OriginalInvoiceNo = "INV-10022", CustomerName = "Rahul Enterprises", ReturnDate = DateTime.Parse("2026-08-15"), RefundValue = 12500, Status = "Refunded" },
                    new SalesReturn { ReturnNo = "SR-7002", OriginalInvoiceNo = "INV-10034", CustomerName = "Mehta Logistics", ReturnDate = DateTime.Parse("2026-08-18"), RefundValue = 25000, Status = "Inspecting" }
                });
            }

            if (!await context.PaymentReceipts.AnyAsync())
            {
                await context.PaymentReceipts.AddRangeAsync(new List<PaymentReceipt>
                {
                    new PaymentReceipt { InvoiceNo = "INV-10041", CustomerName = "Rajesh Corp Ltd", InvoiceDate = DateTime.Parse("2026-08-10"), DueDate = DateTime.Parse("2026-08-25"), PendingBalance = 75000, Status = "Pending" },
                    new PaymentReceipt { InvoiceNo = "INV-10042", CustomerName = "Mehta Logistics", InvoiceDate = DateTime.Parse("2026-08-05"), DueDate = DateTime.Parse("2026-08-20"), PendingBalance = 55250, Status = "Overdue" }
                });
            }

            await context.SaveChangesAsync();
        }

        public static async Task InitializeHRManagementTablesAsync(ApplicationDbContext context)
        {
            // Ensure erp_Onboardings table exists
            string createOnboardingsSql = @"
                IF OBJECT_ID('AITStudent.erp_Onboardings', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Onboardings (
                        OnboardingId INT IDENTITY(1,1) PRIMARY KEY,
                        CandidateName NVARCHAR(150) NOT NULL,
                        Position NVARCHAR(100) NOT NULL,
                        DocumentsStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending (0/4)',
                        BackgroundCheck NVARCHAR(50) NOT NULL DEFAULT 'In Progress',
                        KycProgress INT NOT NULL DEFAULT 0,
                        OrientationStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending'
                    );
                END";

            // Ensure erp_Contracts table exists
            string createContractsSql = @"
                IF OBJECT_ID('AITStudent.erp_Contracts', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Contracts (
                        ContractId INT IDENTITY(1,1) PRIMARY KEY,
                        EmployeeName NVARCHAR(150) NOT NULL,
                        DocumentType NVARCHAR(100) NOT NULL,
                        FileName NVARCHAR(150) NOT NULL,
                        ExpiryDate DATETIME NULL,
                        SigningStatus NVARCHAR(50) NOT NULL DEFAULT 'Draft'
                    );
                END";

            // Ensure erp_Transfers table exists
            string createTransfersSql = @"
                IF OBJECT_ID('AITStudent.erp_Transfers', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Transfers (
                        TransferId INT IDENTITY(1,1) PRIMARY KEY,
                        EmployeeName NVARCHAR(150) NOT NULL,
                        Type NVARCHAR(50) NOT NULL DEFAULT 'Transfer',
                        FromDeptOrDesg NVARCHAR(150) NOT NULL,
                        ToDeptOrDesg NVARCHAR(150) NOT NULL,
                        EffectiveDate DATETIME NOT NULL,
                        ApprovalStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending Approval'
                    );
                END";

            // Ensure erp_Offboardings table exists
            string createOffboardingsSql = @"
                IF OBJECT_ID('AITStudent.erp_Offboardings', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Offboardings (
                        OffboardingId INT IDENTITY(1,1) PRIMARY KEY,
                        EmployeeName NVARCHAR(150) NOT NULL,
                        ResignationDate DATETIME NOT NULL,
                        LastWorkingDay DATETIME NOT NULL,
                        AssetReturn NVARCHAR(50) NOT NULL DEFAULT 'Pending',
                        ITClearance NVARCHAR(50) NOT NULL DEFAULT 'Pending',
                        FinanceClearance NVARCHAR(50) NOT NULL DEFAULT 'Pending',
                        ExitInterview NVARCHAR(50) NOT NULL DEFAULT 'Pending',
                        FfStatus NVARCHAR(50) NOT NULL DEFAULT 'In Progress'
                    );
                END";

            // Execute scripts
            await context.Database.ExecuteSqlRawAsync(createOnboardingsSql);
            await context.Database.ExecuteSqlRawAsync(createContractsSql);
            await context.Database.ExecuteSqlRawAsync(createTransfersSql);
            await context.Database.ExecuteSqlRawAsync(createOffboardingsSql);

            // Seed initial records if empty
            if (!await context.Onboardings.AnyAsync())
            {
                await context.Onboardings.AddRangeAsync(new List<HROnboarding>
                {
                    new HROnboarding { CandidateName = "Amit Patel", Position = "Senior Software Engineer", DocumentsStatus = "Uploaded (4/4)", BackgroundCheck = "Verified", KycProgress = 100, OrientationStatus = "Completed" },
                    new HROnboarding { CandidateName = "Ritu Sharma", Position = "HR Executive", DocumentsStatus = "Pending (2/4)", BackgroundCheck = "In Progress", KycProgress = 50, OrientationStatus = "Pending" }
                });
            }

            if (!await context.Contracts.AnyAsync())
            {
                await context.Contracts.AddRangeAsync(new List<HRContract>
                {
                    new HRContract { EmployeeName = "Numan Sales Man", DocumentType = "Employment Agreement", FileName = "contract_numan.pdf", ExpiryDate = DateTime.Parse("2026-12-31"), SigningStatus = "Signed" },
                    new HRContract { EmployeeName = "Aftab Shaik", DocumentType = "Non-Disclosure Agreement", FileName = "nda_aftab.pdf", SigningStatus = "Signed" }
                });
            }

            if (!await context.Transfers.AnyAsync())
            {
                await context.Transfers.AddRangeAsync(new List<HRTransfer>
                {
                    new HRTransfer { EmployeeName = "Sunil Mehta", Type = "Promotion", FromDeptOrDesg = "Sales Coordinator", ToDeptOrDesg = "Sales Lead", EffectiveDate = DateTime.Parse("2026-09-01"), ApprovalStatus = "Approved" },
                    new HRTransfer { EmployeeName = "Ravi Teja", Type = "Transfer", FromDeptOrDesg = "Hyderabad Branch", ToDeptOrDesg = "Bangalore Branch", EffectiveDate = DateTime.Parse("2026-09-15"), ApprovalStatus = "Pending Approval" }
                });
            }

            if (!await context.Offboardings.AnyAsync())
            {
                await context.Offboardings.AddRangeAsync(new List<HROffboarding>
                {
                    new HROffboarding { EmployeeName = "Vikram Singh", ResignationDate = DateTime.Parse("2026-08-01"), LastWorkingDay = DateTime.Parse("2026-08-31"), AssetReturn = "Returned", ITClearance = "Cleared", FinanceClearance = "Pending Review", ExitInterview = "Done", FfStatus = "In Progress" }
                });
            }

            await context.SaveChangesAsync();
        }

        public static async Task InitializeESSManagementTablesAsync(ApplicationDbContext context)
        {
            // Ensure erp_ESSPunches table exists
            string createESSPunchesSql = @"
                IF OBJECT_ID('AITStudent.erp_ESSPunches', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_ESSPunches (
                        PunchId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        Date DATETIME NOT NULL,
                        CheckInTime DATETIME NULL,
                        CheckOutTime DATETIME NULL,
                        PunchSource NVARCHAR(50) NOT NULL DEFAULT 'Web Clock'
                    );
                END";

            // Ensure erp_ESSLeaveApplications table exists
            string createESSLeaveApplicationsSql = @"
                IF OBJECT_ID('AITStudent.erp_ESSLeaveApplications', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_ESSLeaveApplications (
                        LeaveApplicationId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        LeaveType NVARCHAR(50) NOT NULL DEFAULT 'Casual Leave',
                        StartDate DATETIME NOT NULL,
                        EndDate DATETIME NOT NULL,
                        TotalDays INT NOT NULL,
                        Reason NVARCHAR(255) NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
                    );
                END";

            // Ensure erp_ESSTasks table exists
            string createESSTasksSql = @"
                IF OBJECT_ID('AITStudent.erp_ESSTasks', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_ESSTasks (
                        TaskId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        TaskTitle NVARCHAR(150) NOT NULL,
                        Description NVARCHAR(500) NOT NULL,
                        DueDate DATETIME NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
                    );
                END";

            // Ensure erp_ESSExpenseClaims table exists
            string createESSExpenseClaimsSql = @"
                IF OBJECT_ID('AITStudent.erp_ESSExpenseClaims', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_ESSExpenseClaims (
                        ExpenseClaimId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        ExpenseType NVARCHAR(100) NOT NULL DEFAULT 'Travel',
                        Amount DECIMAL(18,2) NOT NULL,
                        ClaimDate DATETIME NOT NULL DEFAULT GETDATE(),
                        ReceiptFileName NVARCHAR(255) NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
                    );
                END";

            // Ensure erp_ESSSupportTickets table exists
            string createESSSupportTicketsSql = @"
                IF OBJECT_ID('AITStudent.erp_ESSSupportTickets', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_ESSSupportTickets (
                        TicketId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        Department NVARCHAR(50) NOT NULL DEFAULT 'IT Support',
                        Subject NVARCHAR(150) NOT NULL,
                        Description NVARCHAR(1000) NOT NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Open'
                    );
                END";

            // Execute scripts
            await context.Database.ExecuteSqlRawAsync(createESSPunchesSql);
            await context.Database.ExecuteSqlRawAsync(createESSLeaveApplicationsSql);
            await context.Database.ExecuteSqlRawAsync(createESSTasksSql);
            await context.Database.ExecuteSqlRawAsync(createESSExpenseClaimsSql);
            await context.Database.ExecuteSqlRawAsync(createESSSupportTicketsSql);

            // Seed initial records if empty
            if (!await context.ESSPunches.AnyAsync())
            {
                await context.ESSPunches.AddRangeAsync(new List<ESSPunch>
                {
                    new ESSPunch { UserId = 1, Date = DateTime.Today.AddDays(-1), CheckInTime = DateTime.Today.AddDays(-1).AddHours(9), CheckOutTime = DateTime.Today.AddDays(-1).AddHours(18) },
                    new ESSPunch { UserId = 1, Date = DateTime.Today, CheckInTime = DateTime.Today.AddHours(9) }
                });
            }

            if (!await context.ESSLeaveApplications.AnyAsync())
            {
                await context.ESSLeaveApplications.AddRangeAsync(new List<ESSLeaveApplication>
                {
                    new ESSLeaveApplication { UserId = 1, LeaveType = "Casual Leave", StartDate = DateTime.Today.AddDays(5), EndDate = DateTime.Today.AddDays(6), TotalDays = 2, Reason = "Family function", Status = "Pending" },
                    new ESSLeaveApplication { UserId = 1, LeaveType = "Sick Leave", StartDate = DateTime.Today.AddDays(-10), EndDate = DateTime.Today.AddDays(-9), TotalDays = 2, Reason = "Fever", Status = "Approved" }
                });
            }

            if (!await context.ESSTasks.AnyAsync())
            {
                await context.ESSTasks.AddRangeAsync(new List<ESSTask>
                {
                    new ESSTask { UserId = 1, TaskTitle = "Implement ESS Clock-In/Out UI", Description = "Develop the frontend templates for clock-in/out web punch form.", DueDate = DateTime.Today.AddDays(2), Status = "In Progress" },
                    new ESSTask { UserId = 1, TaskTitle = "Fix database seeds", Description = "Seed roles and branches for newly created database catalogs.", DueDate = DateTime.Today.AddDays(-1), Status = "Completed" }
                });
            }

            if (!await context.ESSExpenseClaims.AnyAsync())
            {
                await context.ESSExpenseClaims.AddRangeAsync(new List<ESSExpenseClaim>
                {
                    new ESSExpenseClaim { UserId = 1, ExpenseType = "Internet", Amount = 1500, ClaimDate = DateTime.Today.AddDays(-5), ReceiptFileName = "broadband_bill.pdf", Status = "Approved" },
                    new ESSExpenseClaim { UserId = 1, ExpenseType = "Travel", Amount = 3500, ClaimDate = DateTime.Today, ReceiptFileName = "taxi_receipt.pdf", Status = "Pending" }
                });
            }

            if (!await context.ESSSupportTickets.AnyAsync())
            {
                await context.ESSSupportTickets.AddRangeAsync(new List<ESSSupportTicket>
                {
                    new ESSSupportTicket { UserId = 1, Department = "IT Support", Subject = "Laptop Charger Replacement", Description = "My current laptop charger is overheating. Requesting a replacement charger.", Status = "Open" }
                });
            }

            await context.SaveChangesAsync();
        }

        public static async Task InitializeInventoryManagementTablesAsync(ApplicationDbContext context)
        {
            // Ensure erp_InvWarehouses table exists
            string createInvWarehousesSql = @"
                IF OBJECT_ID('AITStudent.erp_InvWarehouses', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_InvWarehouses (
                        WarehouseId INT IDENTITY(1,1) PRIMARY KEY,
                        Code NVARCHAR(20) NOT NULL,
                        Name NVARCHAR(100) NOT NULL,
                        Location NVARCHAR(150) NOT NULL,
                        IsActive BIT NOT NULL DEFAULT 1
                    );
                END";

            // Ensure erp_InvGrns table exists
            string createInvGrnsSql = @"
                IF OBJECT_ID('AITStudent.erp_InvGrns', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_InvGrns (
                        GrnId INT IDENTITY(1,1) PRIMARY KEY,
                        GrnNo NVARCHAR(50) NOT NULL,
                        SupplierName NVARCHAR(150) NOT NULL,
                        ReceivedDate DATETIME NOT NULL,
                        ReceivedBy NVARCHAR(100) NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Completed'
                    );
                END";

            // Ensure erp_InvTransfers table exists
            string createInvTransfersSql = @"
                IF OBJECT_ID('AITStudent.erp_InvTransfers', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_InvTransfers (
                        TransferId INT IDENTITY(1,1) PRIMARY KEY,
                        TransferNo NVARCHAR(50) NOT NULL,
                        FromWarehouse NVARCHAR(100) NOT NULL,
                        ToWarehouse NVARCHAR(100) NOT NULL,
                        TransferDate DATETIME NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Transferred'
                    );
                END";

            // Ensure erp_InvStockAudits table exists
            string createInvStockAuditsSql = @"
                IF OBJECT_ID('AITStudent.erp_InvStockAudits', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_InvStockAudits (
                        AuditId INT IDENTITY(1,1) PRIMARY KEY,
                        AuditNo NVARCHAR(50) NOT NULL,
                        AuditDate DATETIME NOT NULL,
                        AuditorName NVARCHAR(100) NOT NULL,
                        DiscrepancyFound BIT NOT NULL DEFAULT 0,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Reconciled'
                    );
                END";

            // Ensure erp_InvScrapWriteOffs table exists
            string createInvScrapWriteOffsSql = @"
                IF OBJECT_ID('AITStudent.erp_InvScrapWriteOffs', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_InvScrapWriteOffs (
                        ScrapId INT IDENTITY(1,1) PRIMARY KEY,
                        ScrapNo NVARCHAR(50) NOT NULL,
                        ItemName NVARCHAR(150) NOT NULL,
                        QtyScrapped INT NOT NULL,
                        Reason NVARCHAR(255) NOT NULL,
                        WriteOffDate DATETIME NOT NULL
                    );
                END";

            // Execute scripts
            await context.Database.ExecuteSqlRawAsync(createInvWarehousesSql);
            await context.Database.ExecuteSqlRawAsync(createInvGrnsSql);
            await context.Database.ExecuteSqlRawAsync(createInvTransfersSql);
            await context.Database.ExecuteSqlRawAsync(createInvStockAuditsSql);
            await context.Database.ExecuteSqlRawAsync(createInvScrapWriteOffsSql);

            // Seed initial records if empty
            if (!await context.InvWarehouses.AnyAsync())
            {
                await context.InvWarehouses.AddRangeAsync(new List<InvWarehouse>
                {
                    new InvWarehouse { Code = "WH-001", Name = "Main Central Warehouse", Location = "Plot 24, Industrial Area, Sector 5", IsActive = true },
                    new InvWarehouse { Code = "WH-002", Name = "Transit Logistics Hub", Location = "Freight Terminal A, Airport Road", IsActive = true }
                });
            }

            if (!await context.InvGrns.AnyAsync())
            {
                await context.InvGrns.AddRangeAsync(new List<InvGrn>
                {
                    new InvGrn { GrnNo = "GRN-2026-0001", SupplierName = "Global Electronics Ltd", ReceivedDate = DateTime.Today.AddDays(-5), ReceivedBy = "Numan Sales Man", Status = "Completed" },
                    new InvGrn { GrnNo = "GRN-2026-0002", SupplierName = "Reliable Parts Inc", ReceivedDate = DateTime.Today.AddDays(-2), ReceivedBy = "Aftab Shaik", Status = "Pending Verification" }
                });
            }

            if (!await context.InvTransfers.AnyAsync())
            {
                await context.InvTransfers.AddRangeAsync(new List<InvTransfer>
                {
                    new InvTransfer { TransferNo = "TR-90081", FromWarehouse = "Main Central Warehouse", ToWarehouse = "Transit Logistics Hub", TransferDate = DateTime.Today.AddDays(-3), Status = "Transferred" },
                    new InvTransfer { TransferNo = "TR-90082", FromWarehouse = "Transit Logistics Hub", ToWarehouse = "Main Central Warehouse", TransferDate = DateTime.Today, Status = "In Transit" }
                });
            }

            if (!await context.InvStockAudits.AnyAsync())
            {
                await context.InvStockAudits.AddRangeAsync(new List<InvStockAudit>
                {
                    new InvStockAudit { AuditNo = "AUD-60021", AuditDate = DateTime.Today.AddDays(-10), AuditorName = "Sarah Jenkins", DiscrepancyFound = false, Status = "Reconciled" },
                    new InvStockAudit { AuditNo = "AUD-60022", AuditDate = DateTime.Today.AddDays(-1), AuditorName = "Numan Sales Man", DiscrepancyFound = true, Status = "Pending Review" }
                });
            }

            if (!await context.InvScrapWriteOffs.AnyAsync())
            {
                await context.InvScrapWriteOffs.AddRangeAsync(new List<InvScrapWriteOff>
                {
                    new InvScrapWriteOff { ScrapNo = "SCR-3041", ItemName = "Broken Dell Keyboard", QtyScrapped = 5, Reason = "Liquid damage during handling", WriteOffDate = DateTime.Today.AddDays(-4) },
                    new InvScrapWriteOff { ScrapNo = "SCR-3042", ItemName = "Defective Logistics Box", QtyScrapped = 12, Reason = "Crushed during unloading", WriteOffDate = DateTime.Today.AddDays(-1) }
                });
            }

            await context.SaveChangesAsync();
        }

        public static async Task InitializeAdminManagementTablesAsync(ApplicationDbContext context)
        {
            // Ensure erp_AdminPasswordResets table exists
            string createAdminPasswordResetsSql = @"
                IF OBJECT_ID('AITStudent.erp_AdminPasswordResets', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_AdminPasswordResets (
                        ResetId INT IDENTITY(1,1) PRIMARY KEY,
                        Username NVARCHAR(100) NOT NULL,
                        Email NVARCHAR(100) NOT NULL,
                        RequestDate DATETIME NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
                    );
                END";

            // Ensure erp_AdminBranchHours table exists
            string createAdminBranchHoursSql = @"
                IF OBJECT_ID('AITStudent.erp_AdminBranchHours', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_AdminBranchHours (
                        HourId INT IDENTITY(1,1) PRIMARY KEY,
                        BranchName NVARCHAR(100) NOT NULL,
                        OpeningTime NVARCHAR(20) NOT NULL,
                        ClosingTime NVARCHAR(20) NOT NULL,
                        OffDay NVARCHAR(50) NOT NULL
                    );
                END";

            // Ensure erp_AdminBackupLogs table exists
            string createAdminBackupLogsSql = @"
                IF OBJECT_ID('AITStudent.erp_AdminBackupLogs', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_AdminBackupLogs (
                        BackupId INT IDENTITY(1,1) PRIMARY KEY,
                        Filename NVARCHAR(255) NOT NULL,
                        BackupSize NVARCHAR(50) NOT NULL,
                        CreatedAt DATETIME NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Success'
                    );
                END";

            // Ensure erp_AdminLoginAudits table exists
            string createAdminLoginAuditsSql = @"
                IF OBJECT_ID('AITStudent.erp_AdminLoginAudits', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_AdminLoginAudits (
                        AuditId INT IDENTITY(1,1) PRIMARY KEY,
                        Username NVARCHAR(100) NOT NULL,
                        IpAddress NVARCHAR(50) NOT NULL,
                        LoginTime DATETIME NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Success'
                    );
                END";

            // Ensure erp_AdminAnnouncements table exists
            string createAdminAnnouncementsSql = @"
                IF OBJECT_ID('AITStudent.erp_AdminAnnouncements', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_AdminAnnouncements (
                        AnnouncementId INT IDENTITY(1,1) PRIMARY KEY,
                        Title NVARCHAR(150) NOT NULL,
                        Content NVARCHAR(MAX) NOT NULL,
                        CreatedAt DATETIME NOT NULL,
                        PostedBy NVARCHAR(100) NOT NULL DEFAULT 'System Admin'
                    );
                END";

            // Execute scripts
            await context.Database.ExecuteSqlRawAsync(createAdminPasswordResetsSql);
            await context.Database.ExecuteSqlRawAsync(createAdminBranchHoursSql);
            await context.Database.ExecuteSqlRawAsync(createAdminBackupLogsSql);
            await context.Database.ExecuteSqlRawAsync(createAdminLoginAuditsSql);
            await context.Database.ExecuteSqlRawAsync(createAdminAnnouncementsSql);

            // Seed initial records if empty
            if (!await context.AdminPasswordResets.AnyAsync())
            {
                await context.AdminPasswordResets.AddRangeAsync(new List<AdminPasswordReset>
                {
                    new AdminPasswordReset { Username = "shaikhaftab", Email = "aftab@erp.com", RequestDate = DateTime.Today.AddDays(-1), Status = "Pending" },
                    new AdminPasswordReset { Username = "numan", Email = "numan@erp.com", RequestDate = DateTime.Today.AddDays(-3), Status = "Completed" }
                });
            }

            if (!await context.AdminBranchHours.AnyAsync())
            {
                await context.AdminBranchHours.AddRangeAsync(new List<AdminBranchHour>
                {
                    new AdminBranchHour { BranchName = "Head Office", OpeningTime = "09:00 AM", ClosingTime = "06:00 PM", OffDay = "Sunday" },
                    new AdminBranchHour { BranchName = "Transit Logistics Hub", OpeningTime = "08:00 AM", ClosingTime = "08:00 PM", OffDay = "Sunday" }
                });
            }

            if (!await context.AdminBackupLogs.AnyAsync())
            {
                await context.AdminBackupLogs.AddRangeAsync(new List<AdminBackupLog>
                {
                    new AdminBackupLog { Filename = "backup_db_20260820.bak", BackupSize = "45.2 MB", CreatedAt = DateTime.Today.AddDays(-4), Status = "Success" },
                    new AdminBackupLog { Filename = "backup_db_20260823.bak", BackupSize = "45.8 MB", CreatedAt = DateTime.Today.AddDays(-1), Status = "Success" }
                });
            }

            if (!await context.AdminLoginAudits.AnyAsync())
            {
                await context.AdminLoginAudits.AddRangeAsync(new List<AdminLoginAudit>
                {
                    new AdminLoginAudit { Username = "admin@erp.com", IpAddress = "192.168.1.15", LoginTime = DateTime.Now.AddHours(-2), Status = "Success" },
                    new AdminLoginAudit { Username = "hiring@erp.com", IpAddress = "192.168.1.18", LoginTime = DateTime.Now.AddHours(-5), Status = "Success" }
                });
            }

            if (!await context.AdminAnnouncements.AnyAsync())
            {
                await context.AdminAnnouncements.AddRangeAsync(new List<AdminAnnouncement>
                {
                    new AdminAnnouncement { Title = "Statutory Compliance Updates", Content = "All employees are requested to submit tax declaration forms by end of the month.", CreatedAt = DateTime.Today.AddDays(-2), PostedBy = "System Admin" },
                    new AdminAnnouncement { Title = "System Maintenance Notice", Content = "ERP portal will undergo routine database backup and maintenance this Sunday at 2 AM.", CreatedAt = DateTime.Today, PostedBy = "System Admin" }
                });
            }

            await context.SaveChangesAsync();
        }

        public static async Task InitializeSuperAdminManagementTablesAsync(ApplicationDbContext context)
        {
            // Ensure erp_SuperAdminErrorLogs table exists
            string createSuperAdminErrorLogsSql = @"
                IF OBJECT_ID('AITStudent.erp_SuperAdminErrorLogs', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_SuperAdminErrorLogs (
                        ErrorLogId INT IDENTITY(1,1) PRIMARY KEY,
                        ErrorMessage NVARCHAR(255) NOT NULL,
                        StackTrace NVARCHAR(MAX) NOT NULL,
                        CreatedAt DATETIME NOT NULL
                    );
                END";

            // Ensure erp_SuperAdminMaintenances table exists
            string createSuperAdminMaintenancesSql = @"
                IF OBJECT_ID('AITStudent.erp_SuperAdminMaintenances', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_SuperAdminMaintenances (
                        MaintenanceId INT IDENTITY(1,1) PRIMARY KEY,
                        IsMaintenanceMode BIT NOT NULL DEFAULT 0,
                        CustomMessage NVARCHAR(255) NOT NULL,
                        SetBy NVARCHAR(100) NOT NULL DEFAULT 'Super Admin'
                    );
                END";

            // Ensure erp_SuperAdminIntegrations table exists
            string createSuperAdminIntegrationsSql = @"
                IF OBJECT_ID('AITStudent.erp_SuperAdminIntegrations', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_SuperAdminIntegrations (
                        IntegrationId INT IDENTITY(1,1) PRIMARY KEY,
                        ProviderName NVARCHAR(100) NOT NULL,
                        ApiKey NVARCHAR(255) NOT NULL,
                        ApiUrl NVARCHAR(255) NOT NULL
                    );
                END";

            // Ensure erp_SuperAdminRestorePoints table exists
            string createSuperAdminRestorePointsSql = @"
                IF OBJECT_ID('AITStudent.erp_SuperAdminRestorePoints', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_SuperAdminRestorePoints (
                        RestorePointId INT IDENTITY(1,1) PRIMARY KEY,
                        PointName NVARCHAR(100) NOT NULL,
                        CreatedAt DATETIME NOT NULL,
                        Description NVARCHAR(255) NOT NULL
                    );
                END";

            // Ensure erp_SuperAdminPriceOverrides table exists
            string createSuperAdminPriceOverridesSql = @"
                IF OBJECT_ID('AITStudent.erp_SuperAdminPriceOverrides', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_SuperAdminPriceOverrides (
                        OverrideId INT IDENTITY(1,1) PRIMARY KEY,
                        ProductId INT NOT NULL,
                        VendorName NVARCHAR(100) NOT NULL,
                        CustomPrice DECIMAL(18,2) NOT NULL,
                        ApprovedBy NVARCHAR(100) NOT NULL DEFAULT 'Super Admin'
                    );
                END";

            // Execute scripts
            await context.Database.ExecuteSqlRawAsync(createSuperAdminErrorLogsSql);
            await context.Database.ExecuteSqlRawAsync(createSuperAdminMaintenancesSql);
            await context.Database.ExecuteSqlRawAsync(createSuperAdminIntegrationsSql);
            await context.Database.ExecuteSqlRawAsync(createSuperAdminRestorePointsSql);
            await context.Database.ExecuteSqlRawAsync(createSuperAdminPriceOverridesSql);

            // Seed initial records if empty
            if (!await context.SuperAdminErrorLogs.AnyAsync())
            {
                await context.SuperAdminErrorLogs.AddRangeAsync(new List<SuperAdminErrorLog>
                {
                    new SuperAdminErrorLog { ErrorMessage = "NullReferenceException in Payroll Process", StackTrace = "at ERP_System.Controllers.PayrollController.RunPayroll(Int32 id) in C:\\PayrollController.cs:line 227", CreatedAt = DateTime.Now.AddHours(-1) },
                    new SuperAdminErrorLog { ErrorMessage = "InvalidOperationException: Database Timeout", StackTrace = "at Microsoft.EntityFrameworkCore.DbContext.SaveChanges() in DBContext.cs:line 120", CreatedAt = DateTime.Now.AddDays(-2) }
                });
            }

            if (!await context.SuperAdminMaintenances.AnyAsync())
            {
                await context.SuperAdminMaintenances.AddRangeAsync(new List<SuperAdminMaintenance>
                {
                    new SuperAdminMaintenance { IsMaintenanceMode = false, CustomMessage = "ERP portal is currently undergoing scheduled platform updates. Please check back in 30 minutes.", SetBy = "Super Admin" }
                });
            }

            if (!await context.SuperAdminIntegrations.AnyAsync())
            {
                await context.SuperAdminIntegrations.AddRangeAsync(new List<SuperAdminIntegration>
                {
                    new SuperAdminIntegration { ProviderName = "SendGrid SMTP Mailer", ApiKey = "SG.A1B2C3D4E5F6G7H8I9J0", ApiUrl = "smtp.sendgrid.net" },
                    new SuperAdminIntegration { ProviderName = "Twilio WhatsApp API Gateway", ApiKey = "SK.a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6", ApiUrl = "api.twilio.com" }
                });
            }

            if (!await context.SuperAdminRestorePoints.AnyAsync())
            {
                await context.SuperAdminRestorePoints.AddRangeAsync(new List<SuperAdminRestorePoint>
                {
                    new SuperAdminRestorePoint { PointName = "Pre-Compliance Seed RP", CreatedAt = DateTime.Today.AddDays(-5), Description = "Database snapshot before statutory compliance tables seeding." },
                    new SuperAdminRestorePoint { PointName = "Logistics Launch RP", CreatedAt = DateTime.Today.AddDays(-1), Description = "Database snapshot before logistics branch activation." }
                });
            }

            if (!await context.SuperAdminPriceOverrides.AnyAsync())
            {
                await context.SuperAdminPriceOverrides.AddRangeAsync(new List<SuperAdminPriceOverride>
                {
                    new SuperAdminPriceOverride { ProductId = 1, VendorName = "Global Supplies Ltd", CustomPrice = 42000.00m, ApprovedBy = "Super Admin" },
                    new SuperAdminPriceOverride { ProductId = 4, VendorName = "Reliable Spares Inc", CustomPrice = 750.00m, ApprovedBy = "Super Admin" }
                });
            }

            await context.SaveChangesAsync();
        }
    }
}