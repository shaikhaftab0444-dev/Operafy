using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERP_System.Migrations
{
    /// <inheritdoc />
    public partial class InitialRemote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AITStudent");

            migrationBuilder.CreateTable(
                name: "erp_ActivityLogs",
                schema: "AITStudent",
                columns: table => new
                {
                    ActivityLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ColorClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_ActivityLogs", x => x.ActivityLogId);
                });

            migrationBuilder.CreateTable(
                name: "erp_Products",
                schema: "AITStudent",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoldQty = table.Column<int>(type: "int", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQty = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Products", x => x.ProductId);
                });



            migrationBuilder.CreateTable(
                name: "erp_Transactions",
                schema: "AITStudent",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PartyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Transactions", x => x.TransactionId);
                });

            migrationBuilder.InsertData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                columns: new[] { "ActivityLogId", "ColorClass", "CreatedAt", "Description", "IconClass", "Title" },
                values: new object[,]
                {
                    { 1, "text-primary", new DateTime(2026, 8, 5, 12, 24, 53, 217, DateTimeKind.Utc).AddTicks(2107), "INV-10045 created", "fa-file-invoice", "New Sales Invoice" },
                    { 2, "text-success", new DateTime(2026, 8, 5, 12, 11, 53, 217, DateTimeKind.Utc).AddTicks(2113), "PO-10023 created", "fa-shopping-cart", "New Purchase Order" },
                    { 3, "text-info", new DateTime(2026, 8, 5, 11, 26, 53, 217, DateTimeKind.Utc).AddTicks(2116), "John Doe added", "fa-user-plus", "New Employee Added" },
                    { 4, "text-warning", new DateTime(2026, 8, 5, 10, 26, 53, 217, DateTimeKind.Utc).AddTicks(2120), "₹25,000 received", "fa-hand-holding-usd", "Payment Received" },
                    { 5, "text-danger", new DateTime(2026, 8, 5, 9, 26, 53, 217, DateTimeKind.Utc).AddTicks(2123), "Product stock updated", "fa-boxes", "Stock Updated" }
                });

            migrationBuilder.InsertData(
                schema: "AITStudent",
                table: "erp_Products",
                columns: new[] { "ProductId", "ProductName", "Revenue", "SoldQty", "Status", "StockQty" },
                values: new object[,]
                {
                    { 1, "Laptop", 450000m, 45, "In Stock", 180 },
                    { 2, "Smartphone", 340000m, 85, "Low Stock", 60 },
                    { 3, "Headphones", 180000m, 120, "Out of Stock", 40 },
                    { 4, "Keyboard", 90000m, 60, "In Stock", 100 },
                    { 5, "Mouse", 75000m, 75, "In Stock", 120 }
                });

            migrationBuilder.InsertData(
                schema: "AITStudent",
                table: "erp_Transactions",
                columns: new[] { "TransactionId", "Amount", "Date", "PartyName", "Status", "TransactionNo", "Type" },
                values: new object[,]
                {
                    { 1, 25000m, new DateTime(2026, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rahul Enterprises", "Paid", "INV-10045", "Sales Invoice" },
                    { 2, 18500m, new DateTime(2026, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma Suppliers", "Pending", "PO-10023", "Purchase Order" },
                    { 3, 15750m, new DateTime(2026, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "ABC Corporation", "Paid", "INV-10044", "Sales Invoice" },
                    { 4, 2500m, new DateTime(2026, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Office Supplies", "Paid", "EXP-10012", "Expense Entry" },
                    { 5, 22000m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "XYZ Traders", "Pending", "PO-10022", "Purchase Order" }
                });

            migrationBuilder.InsertData(
                schema: "AITStudent",
                table: "erp_Users",
                columns: new[] { "UserId", "BranchId", "CompanyId", "CreatedAt", "CreatedBy", "Email", "FailedLoginAttempts", "FullName", "IsActive", "IsEmailVerified", "IsLocked", "LastLogin", "LastPasswordChanged", "MobileNumber", "PasswordHash", "PasswordSalt", "ProfilePhoto", "RoleId", "UpdatedAt", "UpdatedBy", "UserCode", "UserName" },
                values: new object[] { 1, 3, 1, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), null, "admin@erp.com", 0, "Admin User", true, false, false, null, null, null, "AQAAAAIAAYagAAAAEOkJcAU1YEZ50GcXjw9Sn+CYrXr+BWC75/EPUpfVliWCv4Alu/+3memoVLfE2G515w==", null, null, 1, null, null, "USR001", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_Users_RoleId",
                schema: "AITStudent",
                table: "erp_Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_ActivityLogs",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Products",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_Transactions",
                schema: "AITStudent");


        }
    }
}
