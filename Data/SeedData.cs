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
    }
}
