using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;

namespace ERP_System.Models
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
<<<<<<< Updated upstream:Models/ApplicationDbContext.cs
=======
        public DbSet<Customer> Customers { get; set; }
        public DbSet<SalaryStructure> SalaryStructures { get; set; }
        public DbSet<Payslip> Payslips { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<FinancialYear> FinancialYears { get; set; }
        public DbSet<AccountHead> AccountHeads { get; set; }
        public DbSet<StockAdjustment> StockAdjustments { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
>>>>>>> Stashed changes:Data/ApplicationDbContext.cs

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
<<<<<<< Updated upstream:Models/ApplicationDbContext.cs
=======
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<SalaryStructure>().ToTable("erp_SalaryStructures");
            modelBuilder.Entity<Payslip>().ToTable("erp_Payslips");
            modelBuilder.Entity<Branch>().ToTable("erp_Branches");
            modelBuilder.Entity<Supplier>().ToTable("erp_Suppliers");
            modelBuilder.Entity<FinancialYear>().ToTable("erp_FinancialYears");
            modelBuilder.Entity<AccountHead>().ToTable("erp_AccountHeads");
            modelBuilder.Entity<RolePermission>().ToTable("erp_RolePermissions");
            modelBuilder.Entity<StockAdjustment>().ToTable("erp_StockAdjustments");
>>>>>>> Stashed changes:Data/ApplicationDbContext.cs

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
