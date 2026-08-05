using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERP_System.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
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
                    table.PrimaryKey("PK_ActivityLogs", x => x.ActivityLogId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
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
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
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
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    company = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_role_id",
                        column: x => x.role_id,
                        principalTable: "Roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ActivityLogs",
                columns: new[] { "ActivityLogId", "ColorClass", "CreatedAt", "Description", "IconClass", "Title" },
                values: new object[,]
                {
                    { 1, "text-primary", new DateTime(2026, 8, 4, 14, 37, 9, 110, DateTimeKind.Utc).AddTicks(6962), "INV-10045 created", "fa-file-invoice", "New Sales Invoice" },
                    { 2, "text-success", new DateTime(2026, 8, 4, 14, 24, 9, 110, DateTimeKind.Utc).AddTicks(6982), "PO-10023 created", "fa-shopping-cart", "New Purchase Order" },
                    { 3, "text-info", new DateTime(2026, 8, 4, 13, 39, 9, 110, DateTimeKind.Utc).AddTicks(6987), "John Doe added", "fa-user-plus", "New Employee Added" },
                    { 4, "text-warning", new DateTime(2026, 8, 4, 12, 39, 9, 110, DateTimeKind.Utc).AddTicks(6993), "₹25,000 received", "fa-hand-holding-usd", "Payment Received" },
                    { 5, "text-danger", new DateTime(2026, 8, 4, 11, 39, 9, 110, DateTimeKind.Utc).AddTicks(6996), "Product stock updated", "fa-boxes", "Stock Updated" }
                });

            migrationBuilder.InsertData(
                table: "Products",
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
                table: "Roles",
                columns: new[] { "role_id", "created_at", "description", "role_name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Full system control", "Super Admin" },
                    { 2, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Manage system settings", "Admin" },
                    { 3, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Manage employees", "HR" },
                    { 4, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Manage teams", "Manager" },
                    { 5, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Basic access", "Employee" },
                    { 6, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Handle accounts", "Accountant" },
                    { 7, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Approve finance", "Finance Manager" },
                    { 8, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Manage stock", "Inventory Manager" },
                    { 9, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Handle purchases", "Purchase Manager" },
                    { 10, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Handle sales", "Sales Executive" },
                    { 11, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Manage sales", "Sales Manager" },
                    { 12, new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "Read-only access", "Auditor" }
                });

            migrationBuilder.InsertData(
                table: "Transactions",
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
                table: "Users",
                columns: new[] { "user_id", "company", "created_at", "email", "full_name", "password_hash", "role_id" },
                values: new object[] { 1, "ERP Solutions Ltd", new DateTime(2026, 8, 4, 15, 53, 51, 483, DateTimeKind.Unspecified), "admin@erp.com", "Admin User", "AQAAAAIAAYagAAAAEDv5H2L8Edavym2dKKJKbdbHk4h1y0W8ePYxL7VZkC6MZ85tgSmgfS/YZn/PzveN1A==", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Users_role_id",
                table: "Users",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
