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
            // Run schema checks to dynamically add columns to erp_RolePermissions if they do not exist
            try
            {
                string checkColumnsSql = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_RolePermissions') AND name = 'CanView')
                    BEGIN
                        ALTER TABLE AITStudent.erp_RolePermissions ADD CanView bit NOT NULL DEFAULT 0;
                        ALTER TABLE AITStudent.erp_RolePermissions ADD CanCreate bit NOT NULL DEFAULT 0;
                        ALTER TABLE AITStudent.erp_RolePermissions ADD CanEdit bit NOT NULL DEFAULT 0;
                        ALTER TABLE AITStudent.erp_RolePermissions ADD CanDelete bit NOT NULL DEFAULT 0;
                        ALTER TABLE AITStudent.erp_RolePermissions ADD CanApprove bit NOT NULL DEFAULT 0;
                    END
                ";
                await context.Database.ExecuteSqlRawAsync(checkColumnsSql);

                // Sync existing IsAllowed to CanView, CanCreate, CanEdit, CanDelete, CanApprove
                string syncSql = @"
                    UPDATE AITStudent.erp_RolePermissions 
                    SET CanView = IsAllowed, CanCreate = IsAllowed, CanEdit = IsAllowed, CanDelete = IsAllowed, CanApprove = IsAllowed
                    WHERE CanView = 0 AND CanCreate = 0 AND CanEdit = 0 AND CanDelete = 0 AND CanApprove = 0 AND IsAllowed = 1
                ";
                await context.Database.ExecuteSqlRawAsync(syncSql);
            }
            catch (Exception)
            {
                // Fallback in case table doesn't exist yet (will be seeded below)
            }

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
                        IsAllowed = isAllowed,
                        CanView = isAllowed,
                        CanCreate = isAllowed,
                        CanEdit = isAllowed,
                        CanDelete = isAllowed,
                        CanApprove = isAllowed
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

            // Dynamically alter erp_ESSLeaveApplications to add columns if they don't exist
            string alterESSLeaveApplicationsSql = @"
                IF OBJECT_ID('AITStudent.erp_ESSLeaveApplications', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSLeaveApplications') AND name = 'EmployeeName')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSLeaveApplications ADD EmployeeName NVARCHAR(150) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSLeaveApplications') AND name = 'ManagerStatus')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSLeaveApplications ADD ManagerStatus NVARCHAR(50) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSLeaveApplications') AND name = 'CreatedAt')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSLeaveApplications ADD CreatedAt DATETIME NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSLeaveApplications') AND name = 'ManagerRemarks')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSLeaveApplications ADD ManagerRemarks NVARCHAR(255) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSLeaveApplications') AND name = 'ReviewedBy')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSLeaveApplications ADD ReviewedBy NVARCHAR(150) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSLeaveApplications') AND name = 'ReviewedAt')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSLeaveApplications ADD ReviewedAt DATETIME NULL;
                    END
                END";
            await context.Database.ExecuteSqlRawAsync(alterESSLeaveApplicationsSql);

            // Dynamically alter erp_ESSExpenseClaims to add columns if they don't exist
            string alterESSExpenseClaimsSql = @"
                IF OBJECT_ID('AITStudent.erp_ESSExpenseClaims', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSExpenseClaims') AND name = 'EmployeeName')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSExpenseClaims ADD EmployeeName NVARCHAR(150) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSExpenseClaims') AND name = 'ManagerStatus')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSExpenseClaims ADD ManagerStatus NVARCHAR(50) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSExpenseClaims') AND name = 'CreatedAt')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSExpenseClaims ADD CreatedAt DATETIME NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSExpenseClaims') AND name = 'ManagerRemarks')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSExpenseClaims ADD ManagerRemarks NVARCHAR(255) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSExpenseClaims') AND name = 'ReviewedBy')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSExpenseClaims ADD ReviewedBy NVARCHAR(150) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSExpenseClaims') AND name = 'ReviewedAt')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSExpenseClaims ADD ReviewedAt DATETIME NULL;
                    END
                END";
            await context.Database.ExecuteSqlRawAsync(alterESSExpenseClaimsSql);

            // Ensure erp_DepartmentTasks table exists
            string createDepartmentTasksSql = @"
                IF OBJECT_ID('AITStudent.erp_DepartmentTasks', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_DepartmentTasks (
                        TaskId INT IDENTITY(1,1) PRIMARY KEY,
                        Title NVARCHAR(150) NOT NULL,
                        Description NVARCHAR(1000) NOT NULL,
                        AssignedToName NVARCHAR(150) NOT NULL,
                        AssignedToEmail NVARCHAR(150) NOT NULL,
                        Priority NVARCHAR(50) NOT NULL DEFAULT 'Medium',
                        DueDate DATETIME NOT NULL,
                        ProgressPercentage INT NOT NULL DEFAULT 0,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'In Progress',
                        AssignedBy NVARCHAR(150) NOT NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
                    );
                END";
            await context.Database.ExecuteSqlRawAsync(createDepartmentTasksSql);

            // Ensure DepartmentTaskId column exists in erp_ESSTasks
            string alterESSTasksSql = @"
                IF OBJECT_ID('AITStudent.erp_ESSTasks', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_ESSTasks') AND name = 'DepartmentTaskId')
                    BEGIN
                        ALTER TABLE AITStudent.erp_ESSTasks ADD DepartmentTaskId INT NULL;
                    END
                END";
            await context.Database.ExecuteSqlRawAsync(alterESSTasksSql);

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
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
                        Token NVARCHAR(MAX) NULL,
                        ExpiryDate DATETIME NULL,
                        RequestType NVARCHAR(100) NULL,
                        DeliveryMethod NVARCHAR(100) NULL
                    );
                END
                ELSE
                BEGIN
                    IF COL_LENGTH('AITStudent.erp_AdminPasswordResets', 'Token') IS NULL
                        ALTER TABLE AITStudent.erp_AdminPasswordResets ADD Token NVARCHAR(MAX) NULL;

                    IF COL_LENGTH('AITStudent.erp_AdminPasswordResets', 'ExpiryDate') IS NULL
                        ALTER TABLE AITStudent.erp_AdminPasswordResets ADD ExpiryDate DATETIME NULL;

                    IF COL_LENGTH('AITStudent.erp_AdminPasswordResets', 'RequestType') IS NULL
                        ALTER TABLE AITStudent.erp_AdminPasswordResets ADD RequestType NVARCHAR(100) NULL;

                    IF COL_LENGTH('AITStudent.erp_AdminPasswordResets', 'DeliveryMethod') IS NULL
                        ALTER TABLE AITStudent.erp_AdminPasswordResets ADD DeliveryMethod NVARCHAR(100) NULL;
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
                        OffDay NVARCHAR(50) NOT NULL,
                        BranchId INT NOT NULL DEFAULT 3,
                        GracePeriod INT NOT NULL DEFAULT 15,
                        BreakDuration INT NOT NULL DEFAULT 45,
                        HalfDayMinHours DECIMAL(18,2) NOT NULL DEFAULT 4.5,
                        IsContinuousShift BIT NOT NULL DEFAULT 0,
                        IsActive BIT NOT NULL DEFAULT 1,
                        EffectiveDate DATETIME NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT FK_erp_AdminBranchHours_erp_Branches FOREIGN KEY (BranchId) REFERENCES AITStudent.erp_Branches(BranchId)
                    );
                END
                ELSE
                BEGIN
                    IF COL_LENGTH('AITStudent.erp_AdminBranchHours', 'BranchId') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminBranchHours ADD BranchId INT NOT NULL DEFAULT 3;
                        ALTER TABLE AITStudent.erp_AdminBranchHours ADD CONSTRAINT FK_erp_AdminBranchHours_erp_Branches FOREIGN KEY (BranchId) REFERENCES AITStudent.erp_Branches(BranchId);
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminBranchHours', 'GracePeriod') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminBranchHours ADD GracePeriod INT NOT NULL DEFAULT 15;
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminBranchHours', 'BreakDuration') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminBranchHours ADD BreakDuration INT NOT NULL DEFAULT 45;
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminBranchHours', 'HalfDayMinHours') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminBranchHours ADD HalfDayMinHours DECIMAL(18,2) NOT NULL DEFAULT 4.5;
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminBranchHours', 'IsContinuousShift') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminBranchHours ADD IsContinuousShift BIT NOT NULL DEFAULT 0;
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminBranchHours', 'IsActive') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminBranchHours ADD IsActive BIT NOT NULL DEFAULT 1;
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminBranchHours', 'EffectiveDate') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminBranchHours ADD EffectiveDate DATETIME NOT NULL DEFAULT GETDATE();
                    END;
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
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Success',
                        BackupType NVARCHAR(50) NOT NULL DEFAULT 'Manual Trigger',
                        TriggeredBy NVARCHAR(100) NOT NULL DEFAULT 'Super Admin',
                        StorageLocation NVARCHAR(100) NOT NULL DEFAULT 'Local Disk'
                    );
                END
                ELSE
                BEGIN
                    IF COL_LENGTH('AITStudent.erp_AdminBackupLogs', 'BackupType') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminBackupLogs ADD BackupType NVARCHAR(50) NOT NULL DEFAULT 'Manual Trigger';
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminBackupLogs', 'TriggeredBy') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminBackupLogs ADD TriggeredBy NVARCHAR(100) NOT NULL DEFAULT 'Super Admin';
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminBackupLogs', 'StorageLocation') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminBackupLogs ADD StorageLocation NVARCHAR(100) NOT NULL DEFAULT 'Local Disk';
                    END;
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
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Success',
                        FullName NVARCHAR(150) NOT NULL DEFAULT 'System User',
                        RoleName NVARCHAR(50) NOT NULL DEFAULT 'Employee',
                        DeviceInfo NVARCHAR(150) NOT NULL DEFAULT 'Chrome / Win11',
                        SessionDuration NVARCHAR(100) NOT NULL DEFAULT 'Active Now'
                    );
                END
                ELSE
                BEGIN
                    IF COL_LENGTH('AITStudent.erp_AdminLoginAudits', 'FullName') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminLoginAudits ADD FullName NVARCHAR(150) NOT NULL DEFAULT 'System User';
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminLoginAudits', 'RoleName') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminLoginAudits ADD RoleName NVARCHAR(50) NOT NULL DEFAULT 'Employee';
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminLoginAudits', 'DeviceInfo') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminLoginAudits ADD DeviceInfo NVARCHAR(150) NOT NULL DEFAULT 'Chrome / Win11';
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminLoginAudits', 'SessionDuration') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminLoginAudits ADD SessionDuration NVARCHAR(100) NOT NULL DEFAULT 'Active Now';
                    END;
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
                        PostedBy NVARCHAR(100) NOT NULL DEFAULT 'System Admin',
                        Priority NVARCHAR(50) NOT NULL DEFAULT 'Normal',
                        Category NVARCHAR(100) NOT NULL DEFAULT 'General',
                        IsPinned BIT NOT NULL DEFAULT 0,
                        AttachmentName NVARCHAR(255) NULL,
                        AttachmentUrl NVARCHAR(255) NULL,
                        TargetAudience NVARCHAR(100) NOT NULL DEFAULT 'All Staff',
                        TargetBranch NVARCHAR(100) NOT NULL DEFAULT 'All Branches',
                        ExpiryDate DATETIME NULL
                    );
                END
                ELSE
                BEGIN
                    IF COL_LENGTH('AITStudent.erp_AdminAnnouncements', 'Priority') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminAnnouncements ADD Priority NVARCHAR(50) NOT NULL DEFAULT 'Normal';
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminAnnouncements', 'Category') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminAnnouncements ADD Category NVARCHAR(100) NOT NULL DEFAULT 'General';
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminAnnouncements', 'IsPinned') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminAnnouncements ADD IsPinned BIT NOT NULL DEFAULT 0;
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminAnnouncements', 'AttachmentName') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminAnnouncements ADD AttachmentName NVARCHAR(255) NULL;
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminAnnouncements', 'AttachmentUrl') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminAnnouncements ADD AttachmentUrl NVARCHAR(255) NULL;
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminAnnouncements', 'TargetAudience') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminAnnouncements ADD TargetAudience NVARCHAR(100) NOT NULL DEFAULT 'All Staff';
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminAnnouncements', 'TargetBranch') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminAnnouncements ADD TargetBranch NVARCHAR(100) NOT NULL DEFAULT 'All Branches';
                    END;
                    IF COL_LENGTH('AITStudent.erp_AdminAnnouncements', 'ExpiryDate') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_AdminAnnouncements ADD ExpiryDate DATETIME NULL;
                    END;
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
                    new AdminPasswordReset { Username = "shaikhaftab", Email = "aftab@erp.com", RequestDate = DateTime.Today.AddDays(-1), Status = "Pending", RequestType = "Automated Self-Service", DeliveryMethod = "Email Magic Link" },
                    new AdminPasswordReset { Username = "numan", Email = "numan@erp.com", RequestDate = DateTime.Today.AddDays(-3), Status = "Manual Override", RequestType = "Admin Override", DeliveryMethod = "Email OTP" },
                    new AdminPasswordReset { Username = "rahul", Email = "rahul@erp.com", RequestDate = DateTime.Today.AddDays(-5), Status = "Token Expired", RequestType = "Automated Self-Service", DeliveryMethod = "Email Magic Link" }
                });
            }

            if (!await context.AdminBranchHours.AnyAsync())
            {
                var defaultBranch = await context.Branches.FirstOrDefaultAsync(b => b.BranchName.Contains("Head") || b.BranchId == 3);
                int headOfficeId = defaultBranch?.BranchId ?? 3;

                await context.AdminBranchHours.AddRangeAsync(new List<AdminBranchHour>
                {
                    new AdminBranchHour { BranchName = "Head Office", BranchId = headOfficeId, OpeningTime = "09:00 AM", ClosingTime = "06:00 PM", OffDay = "Sunday", GracePeriod = 15, BreakDuration = 45, HalfDayMinHours = 4.5m, IsContinuousShift = false, IsActive = true, EffectiveDate = DateTime.Today },
                    new AdminBranchHour { BranchName = "Transit Logistics Hub", BranchId = headOfficeId, OpeningTime = "08:00 AM", ClosingTime = "08:00 PM", OffDay = "Sunday", GracePeriod = 10, BreakDuration = 60, HalfDayMinHours = 5.0m, IsContinuousShift = false, IsActive = true, EffectiveDate = DateTime.Today }
                });
            }

            if (!await context.AdminBackupLogs.AnyAsync())
            {
                await context.AdminBackupLogs.AddRangeAsync(new List<AdminBackupLog>
                {
                    new AdminBackupLog { Filename = "ERP_Prod_Full_20260820.bak", BackupSize = "45.2 MB", CreatedAt = DateTime.Today.AddDays(-4), Status = "Success", BackupType = "Auto Schedule", TriggeredBy = "System Scheduler", StorageLocation = "Azure Storage" },
                    new AdminBackupLog { Filename = "ERP_Prod_Full_20260823.bak", BackupSize = "45.8 MB", CreatedAt = DateTime.Today.AddDays(-1), Status = "Success", BackupType = "Manual Trigger", TriggeredBy = "Super Admin", StorageLocation = "Local Disk" }
                });
            }

            if (!await context.AdminLoginAudits.AnyAsync())
            {
                var today = DateTime.Today;
                await context.AdminLoginAudits.AddRangeAsync(new List<AdminLoginAudit>
                {
                    new AdminLoginAudit 
                    { 
                        Username = "admin@erp.com", 
                        FullName = "Admin User",
                        RoleName = "Super Admin",
                        IpAddress = "192.168.1.1", 
                        DeviceInfo = "Chrome 127 • Win 11",
                        LoginTime = today.AddHours(1).AddMinutes(13), 
                        SessionDuration = "Active Now",
                        Status = "Success" 
                    },
                    new AdminLoginAudit 
                    { 
                        Username = "hiring@erp.com", 
                        FullName = "Sneha Patil",
                        RoleName = "HR Manager",
                        IpAddress = "192.168.1.18", 
                        DeviceInfo = "Edge 126 • Win 11",
                        LoginTime = today.AddDays(-2).AddHours(10).AddMinutes(13), 
                        SessionDuration = "Logged Out (2h 45m)",
                        Status = "Success" 
                    },
                    new AdminLoginAudit 
                    { 
                        Username = "sales1@erp.com", 
                        FullName = "Numan Khan",
                        RoleName = "Sales Executive",
                        IpAddress = "192.168.1.24", 
                        DeviceInfo = "Safari 17 • iOS Mobile",
                        LoginTime = today.AddDays(-2).AddHours(9).AddMinutes(40), 
                        SessionDuration = "Logged Out (4h 10m)",
                        Status = "Success" 
                    },
                    new AdminLoginAudit 
                    { 
                        Username = "unknown_user@erp.com", 
                        FullName = "Unregistered User",
                        RoleName = "External Entity",
                        IpAddress = "45.118.60.2", 
                        DeviceInfo = "Firefox 125 • Linux",
                        LoginTime = today.AddDays(-2).AddHours(3).AddMinutes(22), 
                        SessionDuration = "N/A",
                        Status = "Failed: Bad Password" 
                    },
                    new AdminLoginAudit 
                    { 
                        Username = "payroll_temp@erp.com", 
                        FullName = "Temporary Payroll Clerk",
                        RoleName = "Accountant",
                        IpAddress = "192.168.1.5", 
                        DeviceInfo = "Chrome 127 • Win 11",
                        LoginTime = today.AddDays(-1).AddHours(15).AddMinutes(4), 
                        SessionDuration = "N/A",
                        Status = "Blocked / Locked" 
                    },
                    new AdminLoginAudit 
                    { 
                        Username = "sales2@erp.com", 
                        FullName = "Amit Sharma",
                        RoleName = "Sales Executive",
                        IpAddress = "192.168.1.33", 
                        DeviceInfo = "Chrome 127 • Android",
                        LoginTime = today.AddHours(-10).AddMinutes(30), 
                        SessionDuration = "Session Timeout",
                        Status = "Success" 
                    }
                });
            }

            if (!await context.AdminAnnouncements.AnyAsync())
            {
                await context.AdminAnnouncements.AddRangeAsync(new List<AdminAnnouncement>
                {
                    new AdminAnnouncement 
                    { 
                        Title = "Urgent: Scheduled Server Maintenance & DB Backup", 
                        Content = "The central ERP servers will undergo scheduled infrastructure maintenance and deep database defragmentation. Access will be temporarily suspended.", 
                        CreatedAt = DateTime.Now.AddHours(-2), 
                        PostedBy = "System Admin",
                        Priority = "Urgent / Broadcast",
                        Category = "IT Infrastructure",
                        IsPinned = true,
                        TargetAudience = "All Staff",
                        TargetBranch = "All Branches",
                        ExpiryDate = DateTime.Today.AddDays(4)
                    },
                    new AdminAnnouncement 
                    { 
                        Title = "Statutory Compliance: FY 2026-27 Tax Declarations", 
                        Content = "All employees are requested to submit their tax declaration investments details under section 80C. Download the attached form and submit to finance desk.", 
                        CreatedAt = DateTime.Now.AddDays(-1), 
                        PostedBy = "HR Desk",
                        Priority = "High",
                        Category = "HR Policy",
                        IsPinned = false,
                        AttachmentName = "Tax_Form_12BB.pdf",
                        AttachmentUrl = "/uploads/Tax_Form_12BB.pdf",
                        TargetAudience = "All Staff",
                        TargetBranch = "All Branches",
                        ExpiryDate = DateTime.Today.AddDays(15)
                    },
                    new AdminAnnouncement 
                    { 
                        Title = "Q2 Enterprise Sales Milestone Achieved", 
                        Content = "We are thrilled to announce that AIT Technologies has successfully breached the Q2 sales targets ahead of schedule! Hearty congratulations to the sales force.", 
                        CreatedAt = DateTime.Now.AddDays(-2), 
                        PostedBy = "Corporate Communications",
                        Priority = "Normal",
                        Category = "Holiday & Events",
                        IsPinned = false,
                        TargetAudience = "All Staff",
                        TargetBranch = "All Branches"
                    }
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

            // Ensure erp_Currencies table exists
            string createCurrenciesSql = @"
                IF OBJECT_ID('AITStudent.erp_Currencies', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Currencies (
                        CurrencyId INT IDENTITY(1,1) PRIMARY KEY,
                        CurrencyCode NVARCHAR(3) NOT NULL,
                        CurrencyName NVARCHAR(100) NOT NULL,
                        Symbol NVARCHAR(10) NOT NULL,
                        ExchangeRate DECIMAL(18,6) NOT NULL DEFAULT 1.000000,
                        DecimalPlaces INT NOT NULL DEFAULT 2,
                        IsActive BIT NOT NULL DEFAULT 1,
                        IsBaseCurrency BIT NOT NULL DEFAULT 0,
                        LastUpdated DATETIME NOT NULL DEFAULT GETDATE()
                    );
                END";

            // Ensure erp_CurrencyRateHistories table exists
            string createCurrencyRateHistoriesSql = @"
                IF OBJECT_ID('AITStudent.erp_CurrencyRateHistories', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_CurrencyRateHistories (
                        HistoryId INT IDENTITY(1,1) PRIMARY KEY,
                        CurrencyId INT NOT NULL,
                        ExchangeRate DECIMAL(18,6) NOT NULL,
                        ChangedAt DATETIME NOT NULL DEFAULT GETDATE()
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(createCurrenciesSql);
            await context.Database.ExecuteSqlRawAsync(createCurrencyRateHistoriesSql);

            // Seed currencies if empty
            if (!await context.Currencies.AnyAsync())
            {
                var inr = new Currency { CurrencyCode = "INR", CurrencyName = "Indian Rupee", Symbol = "₹", ExchangeRate = 1.000000m, DecimalPlaces = 2, IsActive = true, IsBaseCurrency = true, LastUpdated = DateTime.Now };
                var usd = new Currency { CurrencyCode = "USD", CurrencyName = "US Dollar", Symbol = "$", ExchangeRate = 83.450000m, DecimalPlaces = 2, IsActive = true, IsBaseCurrency = false, LastUpdated = DateTime.Now };
                var eur = new Currency { CurrencyCode = "EUR", CurrencyName = "Euro", Symbol = "€", ExchangeRate = 90.200000m, DecimalPlaces = 2, IsActive = true, IsBaseCurrency = false, LastUpdated = DateTime.Now };
                var gbp = new Currency { CurrencyCode = "GBP", CurrencyName = "British Pound", Symbol = "£", ExchangeRate = 105.600000m, DecimalPlaces = 2, IsActive = true, IsBaseCurrency = false, LastUpdated = DateTime.Now };

                await context.Currencies.AddRangeAsync(new List<Currency> { inr, usd, eur, gbp });
                await context.SaveChangesAsync(); // save to generate IDs for history

                await context.CurrencyRateHistories.AddRangeAsync(new List<CurrencyRateHistory>
                {
                    new CurrencyRateHistory { CurrencyId = usd.CurrencyId, ExchangeRate = 83.100000m, ChangedAt = DateTime.Now.AddDays(-5) },
                    new CurrencyRateHistory { CurrencyId = usd.CurrencyId, ExchangeRate = 83.300000m, ChangedAt = DateTime.Now.AddDays(-2) },
                    new CurrencyRateHistory { CurrencyId = usd.CurrencyId, ExchangeRate = 83.450000m, ChangedAt = DateTime.Now },
                    new CurrencyRateHistory { CurrencyId = eur.CurrencyId, ExchangeRate = 89.900000m, ChangedAt = DateTime.Now.AddDays(-4) },
                    new CurrencyRateHistory { CurrencyId = eur.CurrencyId, ExchangeRate = 90.200000m, ChangedAt = DateTime.Now }
                });
            }

            // Ensure erp_TaxSlabs table exists
            string createTaxSlabsSql = @"
                IF OBJECT_ID('AITStudent.erp_TaxSlabs', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_TaxSlabs (
                        TaxSlabId INT IDENTITY(1,1) PRIMARY KEY,
                        TaxCode NVARCHAR(50) NOT NULL,
                        Description NVARCHAR(250) NOT NULL,
                        CombinedRate DECIMAL(18,2) NOT NULL,
                        CGST DECIMAL(18,2) NOT NULL DEFAULT 0,
                        SGST DECIMAL(18,2) NOT NULL DEFAULT 0,
                        IGST DECIMAL(18,2) NOT NULL DEFAULT 0,
                        Category NVARCHAR(50) NOT NULL,
                        Regime NVARCHAR(50) NOT NULL,
                        IsRcmActive BIT NOT NULL DEFAULT 0,
                        EffectiveDate DATETIME NOT NULL DEFAULT GETDATE(),
                        IsActive BIT NOT NULL DEFAULT 1
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(createTaxSlabsSql);

            // Seed tax slabs if empty
            if (!await context.TaxSlabs.AnyAsync())
            {
                await context.TaxSlabs.AddRangeAsync(new List<TaxSlab>
                {
                    new TaxSlab { TaxCode = "GST-18", Description = "Goods & Services Tax (Standard)", CombinedRate = 18.00m, CGST = 9.00m, SGST = 9.00m, IGST = 18.00m, Category = "GST", Regime = "GST India", IsRcmActive = false, EffectiveDate = DateTime.Today.AddYears(-1) },
                    new TaxSlab { TaxCode = "GST-5", Description = "Goods & Services Tax (Reduced)", CombinedRate = 5.00m, CGST = 2.50m, SGST = 2.50m, IGST = 5.00m, Category = "GST", Regime = "GST India", IsRcmActive = false, EffectiveDate = DateTime.Today.AddYears(-1) },
                    new TaxSlab { TaxCode = "VAT-15", Description = "Value Added Tax Standard", CombinedRate = 15.00m, CGST = 0.00m, SGST = 0.00m, IGST = 0.00m, Category = "VAT", Regime = "VAT", IsRcmActive = false, EffectiveDate = DateTime.Today.AddYears(-1) }
                });
            }

            // Ensure erp_Departments table exists
            string createDepartmentsSql = @"
                IF OBJECT_ID('AITStudent.erp_Departments', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Departments (
                        DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
                        DepartmentCode NVARCHAR(50) NOT NULL,
                        DepartmentName NVARCHAR(100) NOT NULL,
                        HODId INT NULL,
                        BranchId INT NOT NULL,
                        ParentDepartmentId INT NULL,
                        IsActive BIT NOT NULL DEFAULT 1,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(createDepartmentsSql);

            // Ensure erp_Users table has department, manager, and branch columns
            string alterUsersSql = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_Users') AND name = 'DepartmentId')
                    ALTER TABLE AITStudent.erp_Users ADD DepartmentId INT NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_Users') AND name = 'DepartmentName')
                    ALTER TABLE AITStudent.erp_Users ADD DepartmentName NVARCHAR(150) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_Users') AND name = 'ReportingManagerId')
                    ALTER TABLE AITStudent.erp_Users ADD ReportingManagerId NVARCHAR(50) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_Users') AND name = 'ReportingManagerName')
                    ALTER TABLE AITStudent.erp_Users ADD ReportingManagerName NVARCHAR(150) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_Users') AND name = 'BranchName')
                    ALTER TABLE AITStudent.erp_Users ADD BranchName NVARCHAR(150) NULL;
            ";
            await context.Database.ExecuteSqlRawAsync(alterUsersSql);

            // Seed departments if missing
            var requiredDepartments = new List<(string Code, string Name)>
            {
                ("IT-DEPT", "IT & Software"),
                ("SAL-DEPT", "Sales & Marketing"),
                ("HR-DEPT", "Human Resources"),
                ("FIN-DEPT", "Finance & Accounts"),
                ("OPS-DEPT", "Operations & Logistics")
            };

            var existingDepts = await context.Departments.ToListAsync();
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@erp.com");
            var adminUserId = adminUser?.UserId;

            foreach (var (code, name) in requiredDepartments)
            {
                if (!existingDepts.Any(d => d.DepartmentName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    await context.Departments.AddAsync(new Department
                    {
                        DepartmentCode = code,
                        DepartmentName = name,
                        HODId = adminUserId,
                        BranchId = 3,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        public static async Task InitializeTransactionsAsync(ApplicationDbContext context)
        {
            if (await context.Transactions.AnyAsync())
            {
                return;
            }

            var now = DateTime.UtcNow;
            var rand = new Random();

            var partyNames = new[] {
                "Acme Corp", "Tech Solutions", "Apex Industries", "Global Trade LLC",
                "Metro Systems", "Delta Logistics", "Alpha Distributors", "Zenith Retailers"
            };

            var transactionsToSeed = new List<Transaction>();

            // Seed historical transactions for the last 8 months to populate charts beautifully
            for (int monthOffset = 8; monthOffset >= 0; monthOffset--)
            {
                var monthDate = now.AddMonths(-monthOffset);
                var startOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1);
                
                // Create 3-5 sales invoices per month
                int salesCount = rand.Next(3, 6);
                for (int i = 1; i <= salesCount; i++)
                {
                    decimal amount = rand.Next(150000, 650000);
                    transactionsToSeed.Add(new Transaction
                    {
                        TransactionNo = $"SINV-{monthDate:yyyyMM}-{i:D3}",
                        Type = "Sales Invoice",
                        Date = startOfMonth.AddDays(rand.Next(1, 28)),
                        PartyName = partyNames[rand.Next(partyNames.Length)],
                        Amount = amount,
                        Status = rand.Next(10) < 8 ? "Paid" : "Pending"
                    });
                }

                // Create 2-4 purchase orders per month
                int purchaseCount = rand.Next(2, 5);
                for (int i = 1; i <= purchaseCount; i++)
                {
                    decimal amount = rand.Next(80000, 350000);
                    transactionsToSeed.Add(new Transaction
                    {
                        TransactionNo = $"PO-{monthDate:yyyyMM}-{i:D3}",
                        Type = "Purchase Order",
                        Date = startOfMonth.AddDays(rand.Next(1, 28)),
                        PartyName = partyNames[rand.Next(partyNames.Length)],
                        Amount = amount,
                        Status = rand.Next(10) < 7 ? "Paid" : "Pending"
                    });
                }

                // Create 3-5 expenses per month
                int expenseCount = rand.Next(3, 6);
                for (int i = 1; i <= expenseCount; i++)
                {
                    decimal amount = rand.Next(10000, 75000);
                    transactionsToSeed.Add(new Transaction
                    {
                        TransactionNo = $"EXP-{monthDate:yyyyMM}-{i:D3}",
                        Type = "Expense Entry",
                        Date = startOfMonth.AddDays(rand.Next(1, 28)),
                        PartyName = "General Operations",
                        Amount = amount,
                        Status = "Paid"
                    });
                }
            }

            // Seed a "Capital Contribution" transaction to represent base equity
            transactionsToSeed.Add(new Transaction
            {
                TransactionNo = "CAP-2026-001",
                Type = "Capital Contribution",
                Date = now.AddMonths(-9),
                PartyName = "Shareholders",
                Amount = 1800000m,
                Status = "Paid"
            });

            await context.Transactions.AddRangeAsync(transactionsToSeed);
            await context.SaveChangesAsync();
        }

        public static async Task InitializeDesignationsAsync(ApplicationDbContext context)
        {
            // Ensure erp_Designations table exists
            string createDesignationsSql = @"
                IF OBJECT_ID('AITStudent.erp_Designations', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Designations (
                        DesignationId INT IDENTITY(1,1) PRIMARY KEY,
                        DesignationCode NVARCHAR(10) NOT NULL,
                        JobTitle NVARCHAR(100) NOT NULL,
                        DepartmentId INT NOT NULL,
                        HierarchyLevel NVARCHAR(50) NOT NULL,
                        MinCTC DECIMAL(18,2) NOT NULL,
                        MaxCTC DECIMAL(18,2) NOT NULL,
                        JobDescription NVARCHAR(500) NULL,
                        IsActive BIT NOT NULL DEFAULT 1,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT FK_erp_Designations_erp_Departments FOREIGN KEY (DepartmentId) REFERENCES AITStudent.erp_Departments(DepartmentId)
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(createDesignationsSql);

            // Seed designations if empty
            if (!await context.Designations.AnyAsync())
            {
                var defaultDept = await context.Departments.FirstOrDefaultAsync();
                var hrDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "HR-DEPT" || d.DepartmentName.Contains("HR") || d.DepartmentName.Contains("Resource"));
                var itDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "IT-DEPT" || d.DepartmentName.Contains("IT") || d.DepartmentName.Contains("Tech"));
                var salesDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "SAL-DEPT" || d.DepartmentName.Contains("Sale"));
                var financeDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName.Contains("Acc") || d.DepartmentName.Contains("Fin"));

                int hrId = hrDept?.DepartmentId ?? (defaultDept?.DepartmentId ?? 3);
                int itId = itDept?.DepartmentId ?? (defaultDept?.DepartmentId ?? 3);
                int salesId = salesDept?.DepartmentId ?? (defaultDept?.DepartmentId ?? 3);
                int financeId = financeDept?.DepartmentId ?? (defaultDept?.DepartmentId ?? 3);

                await context.Designations.AddRangeAsync(new List<Designation>
                {
                    new Designation 
                    { 
                        DesignationCode = "HR-MGR", 
                        JobTitle = "Human Resources Manager", 
                        DepartmentId = hrId, 
                        HierarchyLevel = "L5 - Management / Executive", 
                        MinCTC = 850000m, 
                        MaxCTC = 1200000m, 
                        JobDescription = "Oversees human resources operations, policies, and talent acquisition.", 
                        IsActive = true, 
                        CreatedAt = DateTime.UtcNow 
                    },
                    new Designation 
                    { 
                        DesignationCode = "SYS-ADM", 
                        JobTitle = "System & Infrastructure Admin", 
                        DepartmentId = itId, 
                        HierarchyLevel = "L3 - Mid-Level", 
                        MinCTC = 600000m, 
                        MaxCTC = 900000m, 
                        JobDescription = "Manages corporate servers, networks, systems, and security policies.", 
                        IsActive = true, 
                        CreatedAt = DateTime.UtcNow 
                    },
                    new Designation 
                    { 
                        DesignationCode = "SDE-II", 
                        JobTitle = "Software Development Engineer II", 
                        DepartmentId = itId, 
                        HierarchyLevel = "L3 - Mid-Level", 
                        MinCTC = 900000m, 
                        MaxCTC = 1400000m, 
                        JobDescription = "Designs, builds, and maintains custom enterprise applications and databases.", 
                        IsActive = true, 
                        CreatedAt = DateTime.UtcNow 
                    },
                    new Designation 
                    { 
                        DesignationCode = "SAL-EXEC", 
                        JobTitle = "Senior Sales Executive", 
                        DepartmentId = salesId, 
                        HierarchyLevel = "L2 - Junior Associate", 
                        MinCTC = 450000m, 
                        MaxCTC = 700000m, 
                        JobDescription = "Drives client sales, closes deals, and manages business relations.", 
                        IsActive = true, 
                        CreatedAt = DateTime.UtcNow 
                    },
                    new Designation 
                    { 
                        DesignationCode = "ACC-MGR", 
                        JobTitle = "Lead Financial Accountant", 
                        DepartmentId = financeId, 
                        HierarchyLevel = "L4 - Senior Specialist", 
                        MinCTC = 750000m, 
                        MaxCTC = 1100000m, 
                        JobDescription = "Directs financial bookkeeping, audit logs, taxation, and general accounts.", 
                        IsActive = true, 
                        CreatedAt = DateTime.UtcNow 
                    }
                });

                await context.SaveChangesAsync();
            }
        }

        public static async Task InitializeHolidaysAsync(ApplicationDbContext context)
        {
            string createOrUpdateHolidaysSql = @"
                IF OBJECT_ID('AITStudent.erp_Holidays', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Holidays (
                        HolidayId INT IDENTITY(1,1) PRIMARY KEY,
                        HolidayName NVARCHAR(150) NOT NULL,
                        Date DATETIME NOT NULL,
                        Type NVARCHAR(50) NOT NULL DEFAULT 'National Holiday',
                        BranchId INT NULL,
                        IsPaid BIT NOT NULL DEFAULT 1,
                        IsActive BIT NOT NULL DEFAULT 1,
                        Description NVARCHAR(300) NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        UpdatedAt DATETIME NULL,
                        CONSTRAINT FK_erp_Holidays_erp_Branches FOREIGN KEY (BranchId) REFERENCES AITStudent.erp_Branches(BranchId)
                    );
                END
                ELSE
                BEGIN
                    IF COL_LENGTH('AITStudent.erp_Holidays', 'BranchId') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_Holidays ADD BranchId INT NULL;
                        ALTER TABLE AITStudent.erp_Holidays ADD CONSTRAINT FK_erp_Holidays_erp_Branches FOREIGN KEY (BranchId) REFERENCES AITStudent.erp_Branches(BranchId);
                    END;
                    IF COL_LENGTH('AITStudent.erp_Holidays', 'IsPaid') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_Holidays ADD IsPaid BIT NOT NULL DEFAULT 1;
                    END;
                    IF COL_LENGTH('AITStudent.erp_Holidays', 'IsActive') IS NULL
                    BEGIN
                        ALTER TABLE AITStudent.erp_Holidays ADD IsActive BIT NOT NULL DEFAULT 1;
                    END;
                END";

            await context.Database.ExecuteSqlRawAsync(createOrUpdateHolidaysSql);

            if (!await context.Holidays.AnyAsync())
            {
                var defaultBranch = await context.Branches.FirstOrDefaultAsync(b => b.BranchName.Contains("Head") || b.BranchId == 3);
                int headOfficeId = defaultBranch?.BranchId ?? 3;

                var holidays = new List<HRHoliday>
                {
                    // 2026 Holidays
                    new HRHoliday { HolidayName = "Republic Day", Date = new DateTime(2026, 1, 26), Type = "National Holiday", BranchId = null, IsPaid = true, IsActive = true, Description = "Constitution of India celebration - Paid Off" },
                    new HRHoliday { HolidayName = "Maharashtra Day / Labour Day", Date = new DateTime(2026, 5, 1), Type = "Gazetted / Public", BranchId = headOfficeId, IsPaid = true, IsActive = true, Description = "State celebration of Maharashtra Day" },
                    new HRHoliday { HolidayName = "Independence Day", Date = new DateTime(2026, 8, 15), Type = "National Holiday", BranchId = null, IsPaid = true, IsActive = true, Description = "India Independence Day celebration" },
                    new HRHoliday { HolidayName = "Gandhi Jayanti", Date = new DateTime(2026, 10, 2), Type = "National Holiday", BranchId = null, IsPaid = true, IsActive = true, Description = "Mahatma Gandhi Birth Anniversary" },
                    new HRHoliday { HolidayName = "Diwali (Laxmi Pujan)", Date = new DateTime(2026, 11, 8), Type = "Optional / Restricted", BranchId = null, IsPaid = true, IsActive = true, Description = "Festive celebration of Diwali" },
                    new HRHoliday { HolidayName = "Christmas Day", Date = new DateTime(2026, 12, 25), Type = "Gazetted / Public", BranchId = null, IsPaid = true, IsActive = true, Description = "Christmas celebration holiday" },
                    
                    // 2027 Holidays
                    new HRHoliday { HolidayName = "New Year's Day", Date = new DateTime(2027, 1, 1), Type = "Company Foundation Day", BranchId = null, IsPaid = true, IsActive = true, Description = "New Year celebration and company event" },
                    new HRHoliday { HolidayName = "Republic Day", Date = new DateTime(2027, 1, 26), Type = "National Holiday", BranchId = null, IsPaid = true, IsActive = true, Description = "Constitution of India celebration - Paid Off" },
                    new HRHoliday { HolidayName = "Maharashtra Day / Labour Day", Date = new DateTime(2027, 5, 1), Type = "Gazetted / Public", BranchId = headOfficeId, IsPaid = true, IsActive = true, Description = "State celebration of Maharashtra Day" },
                    new HRHoliday { HolidayName = "Independence Day", Date = new DateTime(2027, 8, 15), Type = "National Holiday", BranchId = null, IsPaid = true, IsActive = true, Description = "India Independence Day celebration" },
                    new HRHoliday { HolidayName = "Gandhi Jayanti", Date = new DateTime(2027, 10, 2), Type = "National Holiday", BranchId = null, IsPaid = true, IsActive = true, Description = "Mahatma Gandhi Birth Anniversary" },
                    new HRHoliday { HolidayName = "Diwali (Laxmi Pujan)", Date = new DateTime(2027, 11, 8), Type = "Optional / Restricted", BranchId = null, IsPaid = true, IsActive = true, Description = "Festive celebration of Diwali" },
                    new HRHoliday { HolidayName = "Christmas Day", Date = new DateTime(2027, 12, 25), Type = "Gazetted / Public", BranchId = null, IsPaid = true, IsActive = true, Description = "Christmas celebration holiday" }
                };

                await context.Holidays.AddRangeAsync(holidays);
                await context.SaveChangesAsync();
            }
        }

        public static async Task InitializeRegionalSettingsAsync(ApplicationDbContext context)
        {
            string createTableSql = @"
                IF OBJECT_ID('AITStudent.erp_RegionalConfigurations', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_RegionalConfigurations (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Country NVARCHAR(100) NOT NULL,
                        CurrencyCode NVARCHAR(10) NOT NULL,
                        CurrencySymbol NVARCHAR(10) NOT NULL,
                        NumberSystem NVARCHAR(50) NOT NULL,
                        DateFormat NVARCHAR(50) NOT NULL,
                        Timezone NVARCHAR(100) NOT NULL,
                        TaxSystem NVARCHAR(100) NOT NULL,
                        FinancialYearCycle NVARCHAR(50) NOT NULL,
                        LastUpdated DATETIME NOT NULL DEFAULT GETDATE()
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(createTableSql);

            if (!await context.RegionalConfigurations.AnyAsync())
            {
                await context.RegionalConfigurations.AddAsync(new RegionalConfiguration
                {
                    Country = "India",
                    CurrencyCode = "INR",
                    CurrencySymbol = "₹",
                    NumberSystem = "Lakhs/Crores",
                    DateFormat = "DD/MM/YYYY",
                    Timezone = "India Standard Time",
                    TaxSystem = "GST",
                    FinancialYearCycle = "April 1 - March 31",
                    LastUpdated = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }

        public static async Task InitializeImportLogsAsync(ApplicationDbContext context)
        {
            string createTableSql = @"
                IF OBJECT_ID('AITStudent.erp_ImportLogs', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_ImportLogs (
                        LogId INT IDENTITY(1,1) PRIMARY KEY,
                        ModuleType NVARCHAR(100) NOT NULL,
                        Filename NVARCHAR(255) NOT NULL,
                        TotalRows INT NOT NULL,
                        SuccessRows INT NOT NULL,
                        FailedRows INT NOT NULL,
                        Status NVARCHAR(50) NOT NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        LogFileUrl NVARCHAR(255) NULL
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(createTableSql);

            if (!await context.ImportLogs.AnyAsync())
            {
                await context.ImportLogs.AddRangeAsync(new List<ImportLog>
                {
                    new ImportLog { ModuleType = "Items & SKUs Catalog", Filename = "items_import_v1.xlsx", TotalRows = 120, SuccessRows = 115, FailedRows = 5, Status = "Partial", CreatedAt = DateTime.UtcNow.AddDays(-5), LogFileUrl = "/logs/items_import_v1_errors.txt" },
                    new ImportLog { ModuleType = "Customer Directory", Filename = "customers_2026.xlsx", TotalRows = 45, SuccessRows = 45, FailedRows = 0, Status = "Success", CreatedAt = DateTime.UtcNow.AddDays(-2), LogFileUrl = null }
                });
                await context.SaveChangesAsync();
            }
        }

        public static async Task InitializeExportLogsAsync(ApplicationDbContext context)
        {
            string createTableSql = @"
                IF OBJECT_ID('AITStudent.erp_ExportAuditLogs', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_ExportAuditLogs (
                        AuditId INT IDENTITY(1,1) PRIMARY KEY,
                        DatasetName NVARCHAR(100) NOT NULL,
                        FileFormat NVARCHAR(50) NOT NULL,
                        RecordsCount INT NOT NULL,
                        ExportedBy NVARCHAR(100) NOT NULL,
                        ExportedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        IpAddress NVARCHAR(50) NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Success'
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(createTableSql);

            if (!await context.ExportAuditLogs.AnyAsync())
            {
                await context.ExportAuditLogs.AddRangeAsync(new List<ExportAuditLog>
                {
                    new ExportAuditLog { DatasetName = "Employee Directory Master", FileFormat = "XLSX", RecordsCount = 124, ExportedBy = "Super Admin", ExportedAt = DateTime.UtcNow.AddDays(-3), IpAddress = "192.168.1.15", Status = "Success" },
                    new ExportAuditLog { DatasetName = "Items & SKUs Catalog", FileFormat = "CSV", RecordsCount = 1450, ExportedBy = "Super Admin", ExportedAt = DateTime.UtcNow.AddDays(-1), IpAddress = "192.168.1.15", Status = "Success" }
                });
                await context.SaveChangesAsync();
            }
        }

        public static async Task InitializeRecruitmentTablesAsync(ApplicationDbContext context)
        {
            string sqlScript = @"
                IF OBJECT_ID('AITStudent.erp_JobOpenings', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_JobOpenings (
                        JobId INT IDENTITY(1,1) PRIMARY KEY,
                        JobCode NVARCHAR(50) NOT NULL,
                        JobTitle NVARCHAR(150) NOT NULL,
                        DepartmentId INT NOT NULL,
                        DesignationId INT NULL,
                        HiringManagerId INT NULL,
                        RecruiterId INT NULL,
                        EmploymentType NVARCHAR(50) NOT NULL DEFAULT 'Full-Time',
                        Vacancies INT NOT NULL DEFAULT 1,
                        JobLocation NVARCHAR(100) NOT NULL DEFAULT 'Headquarters',
                        WorkMode NVARCHAR(50) NOT NULL DEFAULT 'On-site',
                        ExperienceRequired NVARCHAR(50) NULL,
                        MinimumEducation NVARCHAR(100) NULL,
                        RequiredSkills NVARCHAR(500) NULL,
                        JobDescription NVARCHAR(2000) NULL,
                        Responsibilities NVARCHAR(2000) NULL,
                        Requirements NVARCHAR(2000) NULL,
                        MinimumSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
                        MaximumSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
                        Currency NVARCHAR(10) NOT NULL DEFAULT 'INR',
                        PostingDate DATETIME NOT NULL DEFAULT GETDATE(),
                        ClosingDate DATETIME NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Open',
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        UpdatedAt DATETIME NULL,
                        CreatedBy INT NULL
                    );
                END

                IF OBJECT_ID('AITStudent.erp_Candidates', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Candidates (
                        CandidateId INT IDENTITY(1,1) PRIMARY KEY,
                        FullName NVARCHAR(150) NOT NULL,
                        Email NVARCHAR(150) NOT NULL,
                        Phone NVARCHAR(30) NOT NULL,
                        Address NVARCHAR(250) NULL,
                        Education NVARCHAR(150) NULL,
                        Experience NVARCHAR(50) NULL,
                        Skills NVARCHAR(500) NULL,
                        CurrentCompany NVARCHAR(150) NULL,
                        CurrentSalary DECIMAL(18,2) NULL,
                        ExpectedSalary DECIMAL(18,2) NULL,
                        NoticePeriod NVARCHAR(50) NULL,
                        ResumePath NVARCHAR(300) NULL,
                        LinkedIn NVARCHAR(250) NULL,
                        Portfolio NVARCHAR(250) NULL,
                        ApplicationSource NVARCHAR(100) NOT NULL DEFAULT 'Portal',
                        ReferredBy NVARCHAR(150) NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        UpdatedAt DATETIME NULL
                    );
                END

                IF OBJECT_ID('AITStudent.erp_CandidateApplications', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_CandidateApplications (
                        ApplicationId INT IDENTITY(1,1) PRIMARY KEY,
                        CandidateId INT NOT NULL,
                        JobId INT NOT NULL,
                        ApplicationDate DATETIME NOT NULL DEFAULT GETDATE(),
                        Stage NVARCHAR(50) NOT NULL DEFAULT 'Applied',
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
                        MatchScore INT NOT NULL DEFAULT 80,
                        Notes NVARCHAR(1000) NULL,
                        Tags NVARCHAR(200) NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        UpdatedAt DATETIME NULL
                    );
                END

                IF OBJECT_ID('AITStudent.erp_CandidateStageHistories', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_CandidateStageHistories (
                        HistoryId INT IDENTITY(1,1) PRIMARY KEY,
                        ApplicationId INT NOT NULL,
                        PreviousStage NVARCHAR(50) NOT NULL,
                        NewStage NVARCHAR(50) NOT NULL,
                        ChangedByUserId INT NULL,
                        ChangeDate DATETIME NOT NULL DEFAULT GETDATE(),
                        ReasonNotes NVARCHAR(500) NULL
                    );
                END

                IF OBJECT_ID('AITStudent.erp_InterviewSchedules', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_InterviewSchedules (
                        InterviewId INT IDENTITY(1,1) PRIMARY KEY,
                        ApplicationId INT NOT NULL,
                        CandidateId INT NOT NULL,
                        JobId INT NOT NULL,
                        InterviewRound NVARCHAR(100) NOT NULL DEFAULT 'HR Interview',
                        InterviewType NVARCHAR(50) NOT NULL DEFAULT 'HR Interview',
                        InterviewMode NVARCHAR(50) NOT NULL DEFAULT 'Online',
                        ScheduledDate DATETIME NOT NULL DEFAULT GETDATE(),
                        StartTime NVARCHAR(20) NOT NULL DEFAULT '10:00 AM',
                        EndTime NVARCHAR(20) NOT NULL DEFAULT '11:00 AM',
                        Location NVARCHAR(250) NULL,
                        MeetingLink NVARCHAR(500) NULL,
                        InterviewerId INT NULL,
                        InterviewerNames NVARCHAR(250) NULL,
                        Notes NVARCHAR(1000) NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Scheduled',
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        UpdatedAt DATETIME NULL
                    );
                END

                IF OBJECT_ID('AITStudent.erp_InterviewFeedbacks', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_InterviewFeedbacks (
                        FeedbackId INT IDENTITY(1,1) PRIMARY KEY,
                        InterviewId INT NOT NULL,
                        CandidateId INT NOT NULL,
                        JobId INT NOT NULL,
                        InterviewerId INT NULL,
                        TechnicalRating INT NOT NULL DEFAULT 3,
                        CommunicationRating INT NOT NULL DEFAULT 3,
                        ExperienceRating INT NOT NULL DEFAULT 3,
                        ProblemSolvingRating INT NOT NULL DEFAULT 3,
                        CulturalFitRating INT NOT NULL DEFAULT 3,
                        OverallRating DECIMAL(3,2) NOT NULL DEFAULT 3.0,
                        Strengths NVARCHAR(1000) NULL,
                        Weaknesses NVARCHAR(1000) NULL,
                        Comments NVARCHAR(1500) NULL,
                        Recommendation NVARCHAR(100) NOT NULL DEFAULT 'Recommend',
                        IsFinalized BIT NOT NULL DEFAULT 1,
                        SubmittedAt DATETIME NOT NULL DEFAULT GETDATE()
                    );
                END

                IF OBJECT_ID('AITStudent.erp_OfferLetters', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_OfferLetters (
                        OfferId INT IDENTITY(1,1) PRIMARY KEY,
                        OfferCode NVARCHAR(50) NOT NULL,
                        ApplicationId INT NOT NULL,
                        CandidateId INT NOT NULL,
                        JobId INT NOT NULL,
                        DesignationId INT NULL,
                        DepartmentId INT NULL,
                        EmploymentType NVARCHAR(50) NOT NULL DEFAULT 'Full-Time',
                        ProposedJoiningDate DATETIME NOT NULL DEFAULT GETDATE(),
                        ReportingManagerId INT NULL,
                        OfferedCTC DECIMAL(18,2) NOT NULL DEFAULT 0,
                        SalaryStructure NVARCHAR(1500) NULL,
                        OfferExpiryDate DATETIME NOT NULL DEFAULT GETDATE(),
                        TermsAndConditions NVARCHAR(2000) NULL,
                        AdditionalNotes NVARCHAR(1000) NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Draft',
                        SentAt DATETIME NULL,
                        RespondedAt DATETIME NULL,
                        ConvertedToEmployeeId INT NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        CreatedBy INT NULL
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(sqlScript);

            // Seed initial Job Openings if empty
            if (!await context.JobOpenings.AnyAsync())
            {
                var salesDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName.Contains("Sales")) ?? await context.Departments.FirstOrDefaultAsync();
                var engDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName.Contains("Software") || d.DepartmentName.Contains("IT") || d.DepartmentName.Contains("Engine")) ?? await context.Departments.FirstOrDefaultAsync();

                int salesDeptId = salesDept?.DepartmentId ?? 1;
                int engDeptId = engDept?.DepartmentId ?? 1;

                var job1 = new JobOpening
                {
                    JobCode = "JOB-1021",
                    JobTitle = "Junior Sales Representative",
                    DepartmentId = salesDeptId,
                    EmploymentType = "Full-Time",
                    Vacancies = 3,
                    JobLocation = "Headquarters",
                    WorkMode = "On-site",
                    ExperienceRequired = "1-2 Years",
                    MinimumEducation = "Bachelor's Degree",
                    RequiredSkills = "Communication, Negotiation, CRM Tools",
                    JobDescription = "We are seeking an energetic Sales Representative to expand customer pipeline.",
                    MinimumSalary = 350000,
                    MaximumSalary = 500000,
                    Currency = "INR",
                    PostingDate = DateTime.Now.AddDays(-20),
                    Status = "Open"
                };

                var job2 = new JobOpening
                {
                    JobCode = "JOB-1022",
                    JobTitle = "Lead Full-stack Developer",
                    DepartmentId = engDeptId,
                    EmploymentType = "Full-Time",
                    Vacancies = 2,
                    JobLocation = "Bangalore Office",
                    WorkMode = "Hybrid",
                    ExperienceRequired = "5+ Years",
                    MinimumEducation = "B.Tech / B.E. / M.C.A.",
                    RequiredSkills = ".NET Core, C#, React, SQL Server, Microservices",
                    JobDescription = "Lead technical development of enterprise ERP modules.",
                    MinimumSalary = 1200000,
                    MaximumSalary = 1800000,
                    Currency = "INR",
                    PostingDate = DateTime.Now.AddDays(-15),
                    Status = "Open"
                };

                await context.JobOpenings.AddRangeAsync(job1, job2);
                await context.SaveChangesAsync();

                // Seed Candidates
                var candidate1 = new Candidate
                {
                    FullName = "Arun Kumar",
                    Email = "arun.kumar@example.com",
                    Phone = "+91 9876543210",
                    Address = "Hyderabad, Telangana",
                    Education = "B.Com Sales & Marketing",
                    Experience = "2 Years",
                    Skills = "Salesforce, Cold Calling, Client Presentations",
                    CurrentCompany = "TechRetail India",
                    CurrentSalary = 300000,
                    ExpectedSalary = 420000,
                    NoticePeriod = "15 Days",
                    ApplicationSource = "Portal",
                    ResumePath = "/uploads/resumes/resume_arun.pdf"
                };

                var candidate2 = new Candidate
                {
                    FullName = "Pooja Patel",
                    Email = "pooja.patel@example.com",
                    Phone = "+91 9812345678",
                    Address = "Bangalore, Karnataka",
                    Education = "B.Tech Computer Science",
                    Experience = "6 Years",
                    Skills = "C#, .NET Core, SQL Server, ReactJS, Web API",
                    CurrentCompany = "InfoSys Ltd",
                    CurrentSalary = 1200000,
                    ExpectedSalary = 1600000,
                    NoticePeriod = "30 Days",
                    ApplicationSource = "LinkedIn",
                    ResumePath = "/uploads/resumes/pooja_cv.pdf"
                };

                var candidate3 = new Candidate
                {
                    FullName = "Ravi Teja",
                    Email = "ravi.teja@example.com",
                    Phone = "+91 9700112233",
                    Address = "Hyderabad, Telangana",
                    Education = "M.C.A Computer Applications",
                    Experience = "4 Years",
                    Skills = "C#, ASP.NET MVC, EF Core",
                    CurrentCompany = "Wipro Solutions",
                    CurrentSalary = 650000,
                    ExpectedSalary = 850000,
                    NoticePeriod = "Immediate",
                    ApplicationSource = "Referral",
                    ResumePath = "/uploads/resumes/ravi_teja_cv.pdf"
                };

                await context.Candidates.AddRangeAsync(candidate1, candidate2, candidate3);
                await context.SaveChangesAsync();

                // Seed Candidate Applications
                var app1 = new CandidateApplication
                {
                    CandidateId = candidate1.CandidateId,
                    JobId = job1.JobId,
                    Stage = "HR Interview",
                    Status = "Active",
                    MatchScore = 85,
                    Notes = "Good communication skills, relevant sales background.",
                    Tags = "High Potential, Immediate Joiner"
                };

                var app2 = new CandidateApplication
                {
                    CandidateId = candidate2.CandidateId,
                    JobId = job2.JobId,
                    Stage = "Technical Interview",
                    Status = "Active",
                    MatchScore = 92,
                    Notes = "Strong technical architecture knowledge.",
                    Tags = "Lead Potential"
                };

                var app3 = new CandidateApplication
                {
                    CandidateId = candidate3.CandidateId,
                    JobId = job2.JobId,
                    Stage = "Selected",
                    Status = "Active",
                    MatchScore = 88,
                    Notes = "Passed all rounds successfully. Ready for offer generation.",
                    Tags = "Selected Candidate"
                };

                await context.CandidateApplications.AddRangeAsync(app1, app2, app3);
                await context.SaveChangesAsync();

                // Seed Stage Histories
                await context.CandidateStageHistories.AddRangeAsync(
                    new CandidateStageHistory { ApplicationId = app1.ApplicationId, PreviousStage = "Applied", NewStage = "Screening", ChangeDate = DateTime.Now.AddDays(-10), ReasonNotes = "Screened profile" },
                    new CandidateStageHistory { ApplicationId = app1.ApplicationId, PreviousStage = "Screening", NewStage = "HR Interview", ChangeDate = DateTime.Now.AddDays(-5), ReasonNotes = "Shortlisted for HR round" },
                    new CandidateStageHistory { ApplicationId = app2.ApplicationId, PreviousStage = "Applied", NewStage = "Technical Interview", ChangeDate = DateTime.Now.AddDays(-3), ReasonNotes = "Directly scheduled for technical round" }
                );

                // Seed Interview Schedules
                var interview1 = new InterviewSchedule
                {
                    ApplicationId = app1.ApplicationId,
                    CandidateId = candidate1.CandidateId,
                    JobId = job1.JobId,
                    InterviewRound = "HR Interview",
                    InterviewType = "HR Interview",
                    InterviewMode = "Online",
                    ScheduledDate = DateTime.Now.AddDays(1),
                    StartTime = "03:00 PM",
                    EndTime = "03:45 PM",
                    MeetingLink = "https://meet.google.com/abc-defg-hij",
                    InterviewerNames = "Aftab Shaik (HR)",
                    Notes = "Assess culture fit and salary expectations.",
                    Status = "Scheduled"
                };

                var interview2 = new InterviewSchedule
                {
                    ApplicationId = app2.ApplicationId,
                    CandidateId = candidate2.CandidateId,
                    JobId = job2.JobId,
                    InterviewRound = "Technical Round 1",
                    InterviewType = "Technical Interview",
                    InterviewMode = "Online",
                    ScheduledDate = DateTime.Now.AddDays(2),
                    StartTime = "11:30 AM",
                    EndTime = "12:30 PM",
                    MeetingLink = "https://meet.google.com/xyz-uvwx-rst",
                    InterviewerNames = "Rajesh Kumar (Tech Lead)",
                    Notes = "Evaluate system design and C# coding capabilities.",
                    Status = "Scheduled"
                };

                await context.InterviewSchedules.AddRangeAsync(interview1, interview2);
                await context.SaveChangesAsync();

                // Seed Offer Letter
                var offer1 = new OfferLetter
                {
                    OfferCode = "OFF-2026-001",
                    ApplicationId = app3.ApplicationId,
                    CandidateId = candidate3.CandidateId,
                    JobId = job2.JobId,
                    EmploymentType = "Full-Time",
                    ProposedJoiningDate = DateTime.Now.AddDays(14),
                    OfferedCTC = 850000,
                    SalaryStructure = "Basic: ₹4,25,000, HRA: ₹2,12,500, Special Allowance: ₹2,12,500",
                    OfferExpiryDate = DateTime.Now.AddDays(7),
                    TermsAndConditions = "Standard employment contract with 90 days probation period.",
                    Status = "Accepted",
                    SentAt = DateTime.Now.AddDays(-5),
                    RespondedAt = DateTime.Now.AddDays(-1)
                };

                await context.OfferLetters.AddAsync(offer1);
                await context.SaveChangesAsync();
            }
        }

        public static async Task InitializePerformanceTablesAsync(ApplicationDbContext context)
        {
            string sqlScript = @"
                IF OBJECT_ID('AITStudent.erp_Okrs', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Okrs (
                        OkrId INT IDENTITY(1,1) PRIMARY KEY,
                        ObjectiveTitle NVARCHAR(200) NOT NULL,
                        Description NVARCHAR(1000) NULL,
                        DepartmentId INT NULL,
                        DesignationId INT NULL,
                        EmployeeId INT NULL,
                        StartDate DATETIME NOT NULL DEFAULT GETDATE(),
                        EndDate DATETIME NOT NULL DEFAULT GETDATE(),
                        Priority NVARCHAR(50) NOT NULL DEFAULT 'Medium',
                        Weightage DECIMAL(5,2) NOT NULL DEFAULT 100.0,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'In Progress',
                        OverallProgressPercentage DECIMAL(5,2) NOT NULL DEFAULT 0.0,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        UpdatedAt DATETIME NULL
                    );
                END

                IF OBJECT_ID('AITStudent.erp_KeyResults', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_KeyResults (
                        KeyResultId INT IDENTITY(1,1) PRIMARY KEY,
                        OkrId INT NOT NULL,
                        KeyResultName NVARCHAR(200) NOT NULL,
                        Description NVARCHAR(500) NULL,
                        TargetValue DECIMAL(18,2) NOT NULL DEFAULT 100.0,
                        CurrentValue DECIMAL(18,2) NOT NULL DEFAULT 0.0,
                        MeasurementUnit NVARCHAR(50) NOT NULL DEFAULT 'Percentage',
                        Weightage DECIMAL(5,2) NOT NULL DEFAULT 100.0,
                        ProgressPercentage DECIMAL(5,2) NOT NULL DEFAULT 0.0,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'In Progress'
                    );
                END

                IF OBJECT_ID('AITStudent.erp_Kpis', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Kpis (
                        KpiId INT IDENTITY(1,1) PRIMARY KEY,
                        KpiName NVARCHAR(150) NOT NULL,
                        Description NVARCHAR(500) NULL,
                        DepartmentId INT NULL,
                        DesignationId INT NULL,
                        EmployeeId INT NULL,
                        TargetValue DECIMAL(18,2) NOT NULL DEFAULT 100.0,
                        ActualAchievement DECIMAL(18,2) NOT NULL DEFAULT 0.0,
                        Weightage DECIMAL(5,2) NOT NULL DEFAULT 100.0,
                        MeasurementType NVARCHAR(50) NOT NULL DEFAULT 'Percentage',
                        ReviewPeriod NVARCHAR(50) NOT NULL DEFAULT 'Quarterly',
                        StartDate DATETIME NOT NULL DEFAULT GETDATE(),
                        EndDate DATETIME NOT NULL DEFAULT GETDATE(),
                        AchievementPercentage DECIMAL(5,2) NOT NULL DEFAULT 0.0,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
                    );
                END

                IF OBJECT_ID('AITStudent.erp_AppraisalCycles', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_AppraisalCycles (
                        CycleId INT IDENTITY(1,1) PRIMARY KEY,
                        CycleName NVARCHAR(150) NOT NULL,
                        Description NVARCHAR(500) NULL,
                        ReviewType NVARCHAR(50) NOT NULL DEFAULT 'Annual',
                        StartDate DATETIME NOT NULL DEFAULT GETDATE(),
                        EndDate DATETIME NOT NULL DEFAULT GETDATE(),
                        SelfReviewDeadline DATETIME NOT NULL DEFAULT GETDATE(),
                        ManagerReviewDeadline DATETIME NOT NULL DEFAULT GETDATE(),
                        ApplicableDepartmentIds NVARCHAR(250) NOT NULL DEFAULT 'All',
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
                    );
                END

                IF OBJECT_ID('AITStudent.erp_EmployeeAppraisals', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_EmployeeAppraisals (
                        AppraisalId INT IDENTITY(1,1) PRIMARY KEY,
                        CycleId INT NOT NULL,
                        EmployeeId INT NOT NULL,
                        ManagerId INT NULL,
                        SelfReviewSubmitted BIT NOT NULL DEFAULT 0,
                        SelfRating DECIMAL(3,2) NULL,
                        SelfComments NVARCHAR(1500) NULL,
                        SelfSubmittedAt DATETIME NULL,
                        ManagerReviewSubmitted BIT NOT NULL DEFAULT 0,
                        ManagerRating DECIMAL(3,2) NULL,
                        ManagerComments NVARCHAR(1500) NULL,
                        ManagerSubmittedAt DATETIME NULL,
                        HRComments NVARCHAR(1500) NULL,
                        GoalScore DECIMAL(3,2) NULL,
                        KpiScore DECIMAL(3,2) NULL,
                        FinalScore DECIMAL(3,2) NULL,
                        FinalRatingBand NVARCHAR(100) NOT NULL DEFAULT 'Pending Evaluation',
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Self Review Pending',
                        EmployeeAcknowledged BIT NOT NULL DEFAULT 0,
                        AcknowledgedAt DATETIME NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        UpdatedAt DATETIME NULL
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(sqlScript);

            // Seed initial OKRs and KPIs if empty
            if (!await context.OkrObjectives.AnyAsync())
            {
                var salesDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName.Contains("Sales")) ?? await context.Departments.FirstOrDefaultAsync();
                var engDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName.Contains("Software") || d.DepartmentName.Contains("IT") || d.DepartmentName.Contains("Engine")) ?? await context.Departments.FirstOrDefaultAsync();

                int salesDeptId = salesDept?.DepartmentId ?? 1;
                int engDeptId = engDept?.DepartmentId ?? 1;

                var okr1 = new OkrObjective
                {
                    ObjectiveTitle = "Boost Q3 sales pipeline expansion",
                    Description = "Expand customer reach across enterprise accounts",
                    DepartmentId = salesDeptId,
                    Priority = "High",
                    StartDate = new DateTime(2026, 7, 1),
                    EndDate = new DateTime(2026, 9, 30),
                    Status = "In Progress",
                    OverallProgressPercentage = 75.0m
                };

                var okr2 = new OkrObjective
                {
                    ObjectiveTitle = "Improve core ERP module performance",
                    Description = "Optimize database queries and API response times across modules",
                    DepartmentId = engDeptId,
                    Priority = "Critical",
                    StartDate = new DateTime(2026, 7, 1),
                    EndDate = new DateTime(2026, 9, 15),
                    Status = "In Progress",
                    OverallProgressPercentage = 40.0m
                };

                await context.OkrObjectives.AddRangeAsync(okr1, okr2);
                await context.SaveChangesAsync();

                // Key Results
                var kr1 = new KeyResult
                {
                    OkrId = okr1.OkrId,
                    KeyResultName = "Achieve total sales of ₹15,00,000 in Q3",
                    TargetValue = 1500000,
                    CurrentValue = 1125000,
                    MeasurementUnit = "Amount",
                    ProgressPercentage = 75.0m,
                    Status = "In Progress"
                };

                var kr2 = new KeyResult
                {
                    OkrId = okr2.OkrId,
                    KeyResultName = "Reduce response times to < 200ms",
                    TargetValue = 200,
                    CurrentValue = 350,
                    MeasurementUnit = "Number",
                    ProgressPercentage = 40.0m,
                    Status = "In Progress"
                };

                await context.KeyResults.AddRangeAsync(kr1, kr2);

                // KPIs
                var kpi1 = new KpiItem
                {
                    KpiName = "Quarterly Sales Target Realization",
                    Description = "Percentage of quarterly sales quota achieved",
                    DepartmentId = salesDeptId,
                    TargetValue = 100,
                    ActualAchievement = 88,
                    MeasurementType = "Percentage",
                    ReviewPeriod = "Quarterly",
                    AchievementPercentage = 88.0m,
                    Status = "Active"
                };

                var kpi2 = new KpiItem
                {
                    KpiName = "Code Quality & Bug Resolution Rate",
                    Description = "Resolution rate of reported P1/P2 issues within SLA",
                    DepartmentId = engDeptId,
                    TargetValue = 95,
                    ActualAchievement = 92,
                    MeasurementType = "Percentage",
                    ReviewPeriod = "Quarterly",
                    AchievementPercentage = 96.84m,
                    Status = "Active"
                };

                await context.Kpis.AddRangeAsync(kpi1, kpi2);
                await context.SaveChangesAsync();
            }

            // Seed Appraisal Cycles & Employee Appraisals if empty
            if (!await context.AppraisalCycles.AnyAsync())
            {
                var cycle1 = new AppraisalCycle
                {
                    CycleName = "Mid-Year Review 2026",
                    Description = "Performance evaluation for H1 2026",
                    ReviewType = "Half-Yearly",
                    StartDate = new DateTime(2026, 6, 1),
                    EndDate = new DateTime(2026, 6, 30),
                    SelfReviewDeadline = new DateTime(2026, 6, 15),
                    ManagerReviewDeadline = new DateTime(2026, 6, 25),
                    ApplicableDepartmentIds = "All",
                    Status = "Completed"
                };

                var cycle2 = new AppraisalCycle
                {
                    CycleName = "Annual Appraisal Cycle 2026",
                    Description = "Annual comprehensive appraisal for all company staff",
                    ReviewType = "Annual",
                    StartDate = new DateTime(2026, 12, 1),
                    EndDate = new DateTime(2026, 12, 31),
                    SelfReviewDeadline = new DateTime(2026, 12, 15),
                    ManagerReviewDeadline = new DateTime(2026, 12, 25),
                    ApplicableDepartmentIds = "All",
                    Status = "Active"
                };

                await context.AppraisalCycles.AddRangeAsync(cycle1, cycle2);
                await context.SaveChangesAsync();

                // Find existing users to attach appraisals
                var numanUser = await context.Users.FirstOrDefaultAsync(u => u.FullName.Contains("Numan")) ?? await context.Users.FirstOrDefaultAsync(u => u.UserId > 1);
                var aftabUser = await context.Users.FirstOrDefaultAsync(u => u.FullName.Contains("Aftab") || u.UserName.Contains("admin")) ?? await context.Users.FirstOrDefaultAsync();

                if (numanUser != null)
                {
                    await context.EmployeeAppraisals.AddAsync(new EmployeeAppraisal
                    {
                        CycleId = cycle1.CycleId,
                        EmployeeId = numanUser.UserId,
                        SelfReviewSubmitted = true,
                        SelfRating = 4.5m,
                        SelfComments = "Successfully expanded client base by 25% and hit quarterly targets.",
                        SelfSubmittedAt = new DateTime(2026, 6, 14),
                        ManagerReviewSubmitted = true,
                        ManagerRating = 4.5m,
                        ManagerComments = "Consistently achieves targets and leads the sales pipeline expansion successfully.",
                        ManagerSubmittedAt = new DateTime(2026, 6, 23),
                        HRComments = "Approved for merit bonus recommendation.",
                        GoalScore = 4.5m,
                        KpiScore = 4.5m,
                        FinalScore = 4.5m,
                        FinalRatingBand = "Exceeds Expectations (A)",
                        Status = "Completed"
                    });
                }

                if (aftabUser != null)
                {
                    await context.EmployeeAppraisals.AddAsync(new EmployeeAppraisal
                    {
                        CycleId = cycle1.CycleId,
                        EmployeeId = aftabUser.UserId,
                        SelfReviewSubmitted = true,
                        SelfRating = 4.0m,
                        SelfComments = "Managed recruitment pipelines efficiently and maintained 100% SLA compliance.",
                        SelfSubmittedAt = new DateTime(2026, 6, 15),
                        ManagerReviewSubmitted = true,
                        ManagerRating = 4.0m,
                        ManagerComments = "Successfully handles recruitment pipelines and onboarding checklists.",
                        ManagerSubmittedAt = new DateTime(2026, 6, 24),
                        HRComments = "Great progress overall.",
                        GoalScore = 4.0m,
                        KpiScore = 4.0m,
                        FinalScore = 4.0m,
                        FinalRatingBand = "Meets Expectations (B)",
                        Status = "Completed"
                    });
                }

                await context.SaveChangesAsync();
            }
        }

        public static async Task InitializeHRAttendanceTablesAsync(ApplicationDbContext context)
        {
            string sqlScript = @"
                IF OBJECT_ID('AITStudent.erp_HRAttendanceLogs', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_HRAttendanceLogs (
                        LogId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        EmployeeCode NVARCHAR(50) NOT NULL,
                        EmployeeName NVARCHAR(150) NOT NULL,
                        Date DATETIME NOT NULL,
                        CheckInTime DATETIME NULL,
                        CheckOutTime DATETIME NULL,
                        WorkHours NVARCHAR(50) NOT NULL DEFAULT '0h 0m',
                        PunchSource NVARCHAR(100) NOT NULL DEFAULT 'Web Clock',
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Present (On Time)',
                        Remarks NVARCHAR(255) NULL
                    );
                END

                IF OBJECT_ID('AITStudent.erp_HRBiometricDevices', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_HRBiometricDevices (
                        DeviceId INT IDENTITY(1,1) PRIMARY KEY,
                        DeviceName NVARCHAR(150) NOT NULL,
                        IpOrLocation NVARCHAR(150) NOT NULL,
                        ConnectionStatus NVARCHAR(50) NOT NULL DEFAULT 'Connected',
                        LastSyncTime DATETIME NOT NULL DEFAULT GETDATE(),
                        TodaySyncCount INT NOT NULL DEFAULT 0,
                        IsActive BIT NOT NULL DEFAULT 1
                    );
                END

                IF OBJECT_ID('AITStudent.erp_HRShiftRosters', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_HRShiftRosters (
                        RosterId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        EmployeeName NVARCHAR(150) NOT NULL,
                        ShiftName NVARCHAR(100) NOT NULL DEFAULT 'General Shift (Day)',
                        Timings NVARCHAR(100) NOT NULL DEFAULT '09:00 AM - 06:00 PM',
                        WeeklyOffs NVARCHAR(100) NOT NULL DEFAULT 'Sunday',
                        EffectiveDate DATETIME NOT NULL DEFAULT GETDATE(),
                        Notes NVARCHAR(255) NULL
                    );
                END

                IF OBJECT_ID('AITStudent.erp_HROvertimeRecords', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_HROvertimeRecords (
                        OvertimeId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        EmployeeName NVARCHAR(150) NOT NULL,
                        MonthYear NVARCHAR(50) NOT NULL,
                        StandardHours INT NOT NULL DEFAULT 160,
                        HoursLogged INT NOT NULL DEFAULT 160,
                        OvertimeHours INT NOT NULL DEFAULT 0,
                        Multiplier NVARCHAR(20) NOT NULL DEFAULT '1.5x',
                        HourlyRate DECIMAL(18,2) NOT NULL DEFAULT 250.00,
                        TotalOvertimePay DECIMAL(18,2) NOT NULL DEFAULT 0.00,
                        PayoutStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending Monthly Cycle'
                    );
                END

                IF OBJECT_ID('AITStudent.erp_HRAttendanceRegularizations', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_HRAttendanceRegularizations (
                        RequestId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        EmployeeName NVARCHAR(150) NOT NULL,
                        CorrectionDate DATETIME NOT NULL,
                        IncorrectPunch NVARCHAR(100) NOT NULL DEFAULT 'Missing Check-out',
                        RequestedCorrectTime NVARCHAR(100) NOT NULL DEFAULT '06:00 PM Check-out',
                        Reason NVARCHAR(500) NOT NULL,
                        RequestDate DATETIME NOT NULL DEFAULT GETDATE(),
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending Review',
                        AdminRemarks NVARCHAR(255) NULL
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(sqlScript);

            // Dynamically alter erp_HRAttendanceRegularizations to add columns if they don't exist
            string alterHRAttendanceRegularizationsSql = @"
                IF OBJECT_ID('AITStudent.erp_HRAttendanceRegularizations', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_HRAttendanceRegularizations') AND name = 'ManagerStatus')
                    BEGIN
                        ALTER TABLE AITStudent.erp_HRAttendanceRegularizations ADD ManagerStatus NVARCHAR(50) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_HRAttendanceRegularizations') AND name = 'CreatedAt')
                    BEGIN
                        ALTER TABLE AITStudent.erp_HRAttendanceRegularizations ADD CreatedAt DATETIME NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_HRAttendanceRegularizations') AND name = 'ManagerRemarks')
                    BEGIN
                        ALTER TABLE AITStudent.erp_HRAttendanceRegularizations ADD ManagerRemarks NVARCHAR(255) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_HRAttendanceRegularizations') AND name = 'ReviewedBy')
                    BEGIN
                        ALTER TABLE AITStudent.erp_HRAttendanceRegularizations ADD ReviewedBy NVARCHAR(150) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AITStudent.erp_HRAttendanceRegularizations') AND name = 'ReviewedAt')
                    BEGIN
                        ALTER TABLE AITStudent.erp_HRAttendanceRegularizations ADD ReviewedAt DATETIME NULL;
                    END
                END";
            await context.Database.ExecuteSqlRawAsync(alterHRAttendanceRegularizationsSql);

            // Seed initial attendance logs for users if empty
            if (!await context.HRAttendanceLogs.AnyAsync())
            {
                var users = await context.Users.Where(u => u.IsActive).ToListAsync();
                var logsList = new List<HRAttendanceLog>();

                var today = DateTime.Today;
                for (int day = 1; day <= 25; day++)
                {
                    var logDate = new DateTime(today.Year, today.Month, Math.Min(day, DateTime.DaysInMonth(today.Year, today.Month)));
                    if (logDate.DayOfWeek == DayOfWeek.Sunday) continue;

                    foreach (var user in users)
                    {
                        string status = "Present (On Time)";
                        DateTime checkIn = logDate.AddHours(9).AddMinutes(new Random(user.UserId + day).Next(0, 10));
                        DateTime checkOut = logDate.AddHours(18).AddMinutes(new Random(user.UserId + day).Next(0, 30));
                        string workHours = "9h 0m";

                        // Introduce realistic late check-ins and absences for test coverage
                        if (day % 7 == 0 && user.UserId % 2 == 0)
                        {
                            status = "Late Check-in";
                            checkIn = logDate.AddHours(9).AddMinutes(45); // 45 mins late
                            workHours = "8h 15m";
                        }
                        else if (day % 11 == 0 && user.UserId % 3 == 0)
                        {
                            status = "Absent";
                            checkIn = default;
                            checkOut = default;
                            workHours = "0h 0m";
                        }

                        logsList.Add(new HRAttendanceLog
                        {
                            UserId = user.UserId,
                            EmployeeCode = user.UserCode,
                            EmployeeName = user.FullName,
                            Date = logDate,
                            CheckInTime = status == "Absent" ? null : checkIn,
                            CheckOutTime = status == "Absent" ? null : checkOut,
                            WorkHours = workHours,
                            PunchSource = "Web Clock",
                            Status = status,
                            Remarks = status == "Late Check-in" ? "Traffic delay" : null
                        });
                    }
                }

                await context.HRAttendanceLogs.AddRangeAsync(logsList);
                await context.SaveChangesAsync();
            }
        }

        public static async Task InitializeLeaveApplicationsAsync(ApplicationDbContext context)
        {
            string sqlScript = @"
                IF OBJECT_ID('AITStudent.erp_ESSLeaveApplications', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_ESSLeaveApplications (
                        LeaveApplicationId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        LeaveType NVARCHAR(50) NOT NULL DEFAULT 'Casual Leave',
                        StartDate DATETIME NOT NULL,
                        EndDate DATETIME NOT NULL,
                        TotalDays INT NOT NULL DEFAULT 1,
                        Reason NVARCHAR(255) NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(sqlScript);

            if (!await context.ESSLeaveApplications.AnyAsync())
            {
                var users = await context.Users.Where(u => u.IsActive).ToListAsync();
                var leaves = new List<ESSLeaveApplication>();

                foreach (var user in users)
                {
                    leaves.Add(new ESSLeaveApplication
                    {
                        UserId = user.UserId,
                        LeaveType = "Casual Leave",
                        StartDate = DateTime.Today.AddDays(-20),
                        EndDate = DateTime.Today.AddDays(-19),
                        TotalDays = 2,
                        Reason = "Personal family event",
                        Status = "Approved"
                    });

                    leaves.Add(new ESSLeaveApplication
                    {
                        UserId = user.UserId,
                        LeaveType = "Sick Leave",
                        StartDate = DateTime.Today.AddDays(-10),
                        EndDate = DateTime.Today.AddDays(-10),
                        TotalDays = 1,
                        Reason = "Fever and doctor visit",
                        Status = "Approved"
                    });

                    leaves.Add(new ESSLeaveApplication
                    {
                        UserId = user.UserId,
                        LeaveType = "Earned Leave",
                        StartDate = DateTime.Today.AddDays(5),
                        EndDate = DateTime.Today.AddDays(7),
                        TotalDays = 3,
                        Reason = "Annual vacation",
                        Status = "Pending"
                    });
                }

                await context.ESSLeaveApplications.AddRangeAsync(leaves);
                await context.SaveChangesAsync();
            }
        }

        public static async Task InitializePayrollRunsAndPayslipsAsync(ApplicationDbContext context)
        {
            string sqlScript = @"
                IF OBJECT_ID('AITStudent.erp_PayrollRuns', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_PayrollRuns (
                        PayrollRunId INT IDENTITY(1,1) PRIMARY KEY,
                        PayPeriod NVARCHAR(50) NOT NULL,
                        Month INT NOT NULL,
                        Year INT NOT NULL,
                        Department NVARCHAR(100) NOT NULL DEFAULT 'All Departments',
                        TotalEmployees INT NOT NULL DEFAULT 0,
                        TotalGrossSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
                        TotalDeductions DECIMAL(18,2) NOT NULL DEFAULT 0,
                        TotalNetSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
                        TotalEmployerPF DECIMAL(18,2) NOT NULL DEFAULT 0,
                        TotalEmployerESI DECIMAL(18,2) NOT NULL DEFAULT 0,
                        TotalCTC DECIMAL(18,2) NOT NULL DEFAULT 0,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Draft',
                        ProcessedByUserId INT NULL,
                        ProcessedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        ApprovedByUserId INT NULL,
                        ApprovedAt DATETIME NULL,
                        PaidByUserId INT NULL,
                        PaidAt DATETIME NULL,
                        Remarks NVARCHAR(500) NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        UpdatedAt DATETIME NULL
                    );
                END

                IF OBJECT_ID('AITStudent.erp_Payslips', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_Payslips (
                        PayslipId INT IDENTITY(1,1) PRIMARY KEY,
                        PayrollRunId INT NULL,
                        UserId INT NOT NULL,
                        PayPeriod NVARCHAR(50) NOT NULL,
                        PayslipNumber NVARCHAR(50) NOT NULL,
                        BasicSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
                        HRA DECIMAL(18,2) NOT NULL DEFAULT 0,
                        TransportAllowance DECIMAL(18,2) NOT NULL DEFAULT 0,
                        MedicalAllowance DECIMAL(18,2) NOT NULL DEFAULT 0,
                        LTA DECIMAL(18,2) NOT NULL DEFAULT 0,
                        SpecialAllowance DECIMAL(18,2) NOT NULL DEFAULT 0,
                        OtherAllowance DECIMAL(18,2) NOT NULL DEFAULT 0,
                        BonusIncentives DECIMAL(18,2) NOT NULL DEFAULT 0,
                        OvertimePay DECIMAL(18,2) NOT NULL DEFAULT 0,
                        GrossSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
                        ProvidentFund DECIMAL(18,2) NOT NULL DEFAULT 0,
                        ESI DECIMAL(18,2) NOT NULL DEFAULT 0,
                        ProfessionalTax DECIMAL(18,2) NOT NULL DEFAULT 0,
                        TDS DECIMAL(18,2) NOT NULL DEFAULT 0,
                        LOPDeduction DECIMAL(18,2) NOT NULL DEFAULT 0,
                        TotalDeductions DECIMAL(18,2) NOT NULL DEFAULT 0,
                        NetSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
                        EmployerPF DECIMAL(18,2) NOT NULL DEFAULT 0,
                        EmployerESI DECIMAL(18,2) NOT NULL DEFAULT 0,
                        TotalCTC DECIMAL(18,2) NOT NULL DEFAULT 0,
                        TotalWorkingDays INT NOT NULL DEFAULT 30,
                        PresentDays INT NOT NULL DEFAULT 30,
                        AbsentDays INT NOT NULL DEFAULT 0,
                        PaidLeaveDays INT NOT NULL DEFAULT 0,
                        UnpaidLeaveDays INT NOT NULL DEFAULT 0,
                        PaidDays INT NOT NULL DEFAULT 30,
                        OvertimeHours INT NOT NULL DEFAULT 0,
                        PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
                        Status NVARCHAR(30) NOT NULL DEFAULT 'Paid'
                    );
                END

                IF OBJECT_ID('AITStudent.erp_StatutoryConfigurations', 'U') IS NULL
                BEGIN
                    CREATE TABLE AITStudent.erp_StatutoryConfigurations (
                        StatutoryId INT IDENTITY(1,1) PRIMARY KEY,
                        RuleType NVARCHAR(50) NOT NULL,
                        EmployeeRate DECIMAL(18,2) NOT NULL DEFAULT 0,
                        EmployerRate DECIMAL(18,2) NOT NULL DEFAULT 0,
                        WageCeilingLimit DECIMAL(18,2) NOT NULL DEFAULT 0,
                        StandardDeductionAnnual DECIMAL(18,2) NOT NULL DEFAULT 75000.00,
                        DefaultTaxRegime NVARCHAR(50) NOT NULL DEFAULT 'New Tax Regime',
                        ConfigurationDetailsJson NVARCHAR(2000) NULL,
                        IsActive BIT NOT NULL DEFAULT 1,
                        EffectiveDate DATETIME NOT NULL DEFAULT GETDATE(),
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        UpdatedAt DATETIME NULL
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(sqlScript);

            // Seed initial statutory configs if empty
            if (!await context.StatutoryConfigurations.AnyAsync())
            {
                await context.StatutoryConfigurations.AddRangeAsync(new List<StatutoryConfiguration>
                {
                    new StatutoryConfiguration { RuleType = "PF", EmployeeRate = 12.0m, EmployerRate = 12.0m, WageCeilingLimit = 15000.0m, DefaultTaxRegime = "New Tax Regime" },
                    new StatutoryConfiguration { RuleType = "ESI", EmployeeRate = 0.75m, EmployerRate = 3.25m, WageCeilingLimit = 21000.0m, DefaultTaxRegime = "New Tax Regime" },
                    new StatutoryConfiguration { RuleType = "PT", EmployeeRate = 200.0m, EmployerRate = 0.0m, WageCeilingLimit = 0.0m, DefaultTaxRegime = "New Tax Regime" },
                    new StatutoryConfiguration { RuleType = "TDS", EmployeeRate = 10.0m, EmployerRate = 0.0m, WageCeilingLimit = 500000.0m, DefaultTaxRegime = "New Tax Regime" }
                });
                await context.SaveChangesAsync();
            }

            // Seed initial Payroll Runs & Payslips if empty
            if (!await context.PayrollRuns.AnyAsync())
            {
                var users = await context.Users.Where(u => u.IsActive).ToListAsync();

                var runAug = new PayrollRun
                {
                    PayPeriod = "August 2026",
                    Month = 8,
                    Year = 2026,
                    Department = "All Departments",
                    TotalEmployees = users.Count,
                    TotalGrossSalary = users.Count * 65000,
                    TotalDeductions = users.Count * 8500,
                    TotalNetSalary = users.Count * 56500,
                    TotalEmployerPF = users.Count * 1800,
                    TotalEmployerESI = users.Count * 500,
                    TotalCTC = users.Count * 67300,
                    Status = "Paid",
                    ProcessedAt = DateTime.Now.AddDays(-2),
                    PaidAt = DateTime.Now.AddDays(-1)
                };

                var runJuly = new PayrollRun
                {
                    PayPeriod = "July 2026",
                    Month = 7,
                    Year = 2026,
                    Department = "All Departments",
                    TotalEmployees = users.Count,
                    TotalGrossSalary = users.Count * 64000,
                    TotalDeductions = users.Count * 8300,
                    TotalNetSalary = users.Count * 55700,
                    TotalEmployerPF = users.Count * 1800,
                    TotalEmployerESI = users.Count * 500,
                    TotalCTC = users.Count * 66300,
                    Status = "Paid",
                    ProcessedAt = DateTime.Now.AddDays(-32),
                    PaidAt = DateTime.Now.AddDays(-31)
                };

                await context.PayrollRuns.AddRangeAsync(runAug, runJuly);
                await context.SaveChangesAsync();

                var payslipsList = new List<Payslip>();
                int slipCount = 1000;

                foreach (var user in users)
                {
                    decimal basic = 35000m;
                    decimal hra = 17500m;
                    decimal special = 12500m;
                    decimal gross = basic + hra + special;

                    decimal pf = 1800m;
                    decimal esi = gross <= 21000 ? gross * 0.0075m : 0m;
                    decimal pt = 200m;
                    decimal tds = gross > 50000 ? 4500m : 1200m;
                    decimal totDeductions = pf + esi + pt + tds;
                    decimal net = gross - totDeductions;

                    payslipsList.Add(new Payslip
                    {
                        PayrollRunId = runAug.PayrollRunId,
                        UserId = user.UserId,
                        PayPeriod = "August 2026",
                        PayslipNumber = $"PAY-202608-{(++slipCount)}",
                        BasicSalary = basic,
                        HRA = hra,
                        SpecialAllowance = special,
                        GrossSalary = gross,
                        ProvidentFund = pf,
                        ESI = esi,
                        ProfessionalTax = pt,
                        TDS = tds,
                        TotalDeductions = totDeductions,
                        NetSalary = net,
                        EmployerPF = pf,
                        EmployerESI = gross <= 21000 ? gross * 0.0325m : 0m,
                        TotalCTC = gross + pf,
                        Status = "Paid",
                        PaymentDate = DateTime.Now.AddDays(-1)
                    });

                    payslipsList.Add(new Payslip
                    {
                        PayrollRunId = runJuly.PayrollRunId,
                        UserId = user.UserId,
                        PayPeriod = "July 2026",
                        PayslipNumber = $"PAY-202607-{(++slipCount)}",
                        BasicSalary = basic,
                        HRA = hra,
                        SpecialAllowance = special,
                        GrossSalary = gross,
                        ProvidentFund = pf,
                        ESI = esi,
                        ProfessionalTax = pt,
                        TDS = tds,
                        TotalDeductions = totDeductions,
                        NetSalary = net,
                        EmployerPF = pf,
                        EmployerESI = gross <= 21000 ? gross * 0.0325m : 0m,
                        TotalCTC = gross + pf,
                        Status = "Paid",
                        PaymentDate = DateTime.Now.AddDays(-31)
                    });
                }

                await context.Payslips.AddRangeAsync(payslipsList);
                await context.SaveChangesAsync();
            }
        }


        
    }
}