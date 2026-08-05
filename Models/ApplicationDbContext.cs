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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Roles (Matching your exact screenshot)
            var roles = new[]
            {
                new Role { RoleId = 1, RoleName = "Super Admin", Description = "Full system control", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") },
                new Role { RoleId = 2, RoleName = "Admin", Description = "Manage system settings", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") },
                new Role { RoleId = 3, RoleName = "HR", Description = "Manage employees", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") },
                new Role { RoleId = 4, RoleName = "Manager", Description = "Manage teams", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") },
                new Role { RoleId = 5, RoleName = "Employee", Description = "Basic access", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") },
                new Role { RoleId = 6, RoleName = "Accountant", Description = "Handle accounts", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") },
                new Role { RoleId = 7, RoleName = "Finance Manager", Description = "Approve finance", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") },
                new Role { RoleId = 8, RoleName = "Inventory Manager", Description = "Manage stock", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") },
                new Role { RoleId = 9, RoleName = "Purchase Manager", Description = "Handle purchases", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") },
                new Role { RoleId = 10, RoleName = "Sales Executive", Description = "Handle sales", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") },
                new Role { RoleId = 11, RoleName = "Sales Manager", Description = "Manage sales", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") },
                new Role { RoleId = 12, RoleName = "Auditor", Description = "Read-only access", CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483") }
            };

            modelBuilder.Entity<Role>().HasData(roles);

            // Seed Admin User (Using Identity Password Hasher)
            var hasher = new PasswordHasher<User>();
            var adminUser = new User
            {
                UserId = 1,
                FullName = "Admin User",
                Email = "admin@erp.com",
                RoleId = 1, // Super Admin
                Company = "ERP Solutions Ltd",
                CreatedAt = DateTime.Parse("2026-08-04T15:53:51.483")
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            modelBuilder.Entity<User>().HasData(adminUser);

            // Seed Products (Matching your Dashboard screenshot for "Top Selling Products")
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, ProductName = "Laptop", SoldQty = 45, Revenue = 450000, StockQty = 180, Status = "In Stock" },
                new Product { ProductId = 2, ProductName = "Smartphone", SoldQty = 85, Revenue = 340000, StockQty = 60, Status = "Low Stock" },
                new Product { ProductId = 3, ProductName = "Headphones", SoldQty = 120, Revenue = 180000, StockQty = 40, Status = "Out of Stock" },
                new Product { ProductId = 4, ProductName = "Keyboard", SoldQty = 60, Revenue = 90000, StockQty = 100, Status = "In Stock" },
                new Product { ProductId = 5, ProductName = "Mouse", SoldQty = 75, Revenue = 75000, StockQty = 120, Status = "In Stock" }
            );

            // Seed Transactions (Matching your Dashboard screenshot for "Recent Transactions")
            modelBuilder.Entity<Transaction>().HasData(
                new Transaction { TransactionId = 1, TransactionNo = "INV-10045", Type = "Sales Invoice", Date = DateTime.Parse("2026-05-31"), PartyName = "Rahul Enterprises", Amount = 25000, Status = "Paid" },
                new Transaction { TransactionId = 2, TransactionNo = "PO-10023", Type = "Purchase Order", Date = DateTime.Parse("2026-05-31"), PartyName = "Sharma Suppliers", Amount = 18500, Status = "Pending" },
                new Transaction { TransactionId = 3, TransactionNo = "INV-10044", Type = "Sales Invoice", Date = DateTime.Parse("2026-05-30"), PartyName = "ABC Corporation", Amount = 15750, Status = "Paid" },
                new Transaction { TransactionId = 4, TransactionNo = "EXP-10012", Type = "Expense Entry", Date = DateTime.Parse("2026-05-30"), PartyName = "Office Supplies", Amount = 2500, Status = "Paid" },
                new Transaction { TransactionId = 5, TransactionNo = "PO-10022", Type = "Purchase Order", Date = DateTime.Parse("2026-05-29"), PartyName = "XYZ Traders", Amount = 22000, Status = "Pending" }
            );

            // Seed Activity Logs (Matching your Dashboard screenshot for "Recent Activities")
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
