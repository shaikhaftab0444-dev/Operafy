using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;

namespace ERP_System.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            try
            {
                // 1. Ensure SQL Server tables exist in schema AITStudent
                string createTablesSql = @"
                    IF OBJECT_ID('AITStudent.erp_ProcurementCatalogItems', 'U') IS NULL
                    BEGIN
                        CREATE TABLE AITStudent.erp_ProcurementCatalogItems (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            ItemName NVARCHAR(200) NOT NULL,
                            DepartmentId INT NOT NULL,
                            UnitPrice DECIMAL(18,2) NOT NULL,
                            UnitOfMeasure NVARCHAR(50) NOT NULL DEFAULT 'Unit',
                            IsActive BIT NOT NULL DEFAULT 1
                        );
                    END;

                    IF OBJECT_ID('AITStudent.erp_PurchaseRequisitions', 'U') IS NULL
                    BEGIN
                        CREATE TABLE AITStudent.erp_PurchaseRequisitions (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            UserId INT NOT NULL,
                            DepartmentId INT NOT NULL,
                            CatalogItemId INT NOT NULL,
                            Quantity INT NOT NULL DEFAULT 1,
                            UnitPrice DECIMAL(18,2) NOT NULL,
                            EstimatedTotalCost DECIMAL(18,2) NOT NULL,
                            UrgencyLevel NVARCHAR(100) NOT NULL DEFAULT 'Medium Priority',
                            BusinessJustification NVARCHAR(1000) NULL,
                            Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
                            CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
                        );
                    END;
                ";
                await context.Database.ExecuteSqlRawAsync(createTablesSql);

                // 2. Fetch all existing departments
                var departments = await context.Departments.ToListAsync();
                if (!departments.Any())
                {
                    return;
                }

                // 3. Seed Catalog Items mapped to departments
                var existingItems = await context.ProcurementCatalogItems.ToListAsync();

                var defaultCatalog = new List<(string DeptMatch, string ItemName, decimal UnitPrice, string Uom)>
                {
                    // Finance & Accounts
                    ("Finance", "Ledger Binding Paper (Ream)", 450m, "Ream"),
                    ("Finance", "Cheque/Voucher Printer Cartridge", 2400m, "Unit"),
                    ("Finance", "Financial Calculator (12-Digit)", 1150m, "Piece"),
                    ("Finance", "Heavy-Duty 2-Hole Punch", 650m, "Piece"),

                    // Information Technology / IT & Software
                    ("IT", "24-Inch IPS Monitor", 12500m, "Unit"),
                    ("IT", "USB-C Multiport Hub", 2800m, "Unit"),
                    ("IT", "Ergonomic Mechanical Keyboard", 3200m, "Piece"),
                    ("IT", "Cat6 Ethernet Patch Cable 10m", 450m, "Piece"),

                    // Warehouse & Operations / Operations & Logistics
                    ("Operations", "Industrial Barcode Scanner", 4500m, "Unit"),
                    ("Operations", "Heavy Duty Packaging Tape (Pack of 6)", 750m, "Pack"),
                    ("Operations", "Thermal Label Roll", 350m, "Roll"),
                    ("Operations", "Digital Weighing Scale 50kg", 3800m, "Unit"),

                    // Human Resources
                    ("Human Resources", "Employee ID Card Lanyards & Badges (Pack of 50)", 850m, "Pack"),
                    ("Human Resources", "Executive Mesh Office Chair", 6500m, "Unit"),
                    ("Human Resources", "Desktop Whiteboard & Marker Set", 950m, "Set"),

                    // Sales & Marketing
                    ("Sales", "Client Presentation Folder Set (50 pcs)", 1200m, "Pack"),
                    ("Sales", "Laser Presenter with Air Mouse", 1850m, "Unit"),
                    ("Sales", "Retractable Standee Banner", 2100m, "Piece")
                };

                var itemsToAdd = new List<ProcurementCatalogItem>();

                foreach (var dept in departments)
                {
                    // Find catalog items matching this department name
                    var matches = defaultCatalog.Where(c => dept.DepartmentName.Contains(c.DeptMatch, StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    // If no direct keyword match, provide general office defaults
                    if (!matches.Any())
                    {
                        matches = new List<(string DeptMatch, string ItemName, decimal UnitPrice, string Uom)>
                        {
                            (dept.DepartmentName, "A4 Copier Paper (Box of 5 Reams)", 1250m, "Box"),
                            (dept.DepartmentName, "Executive Ballpoint Pens (Box of 20)", 400m, "Box"),
                            (dept.DepartmentName, "Desk Organiser & Document Tray", 750m, "Set")
                        };
                    }

                    foreach (var m in matches)
                    {
                        if (!existingItems.Any(i => i.DepartmentId == dept.DepartmentId && i.ItemName.Equals(m.ItemName, StringComparison.OrdinalIgnoreCase)))
                        {
                            itemsToAdd.Add(new ProcurementCatalogItem
                            {
                                DepartmentId = dept.DepartmentId,
                                ItemName = m.ItemName,
                                UnitPrice = m.UnitPrice,
                                UnitOfMeasure = m.Uom,
                                IsActive = true
                            });
                        }
                    }
                }

                if (itemsToAdd.Any())
                {
                    await context.ProcurementCatalogItems.AddRangeAsync(itemsToAdd);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DbInitializer] Warning during catalog initialization: {ex.Message}");
            }
        }
    }
}
