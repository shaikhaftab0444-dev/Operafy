using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_System.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchAndCategoryToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add BranchId and Category columns to erp_Products table
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                schema: "AITStudent",
                table: "erp_Products",
                type: "int",
                nullable: false,
                defaultValue: 3); // Defaults to Head Office

            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "AITStudent",
                table: "erp_Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "General");

            // 2. Seed existing Products with BranchId = 3, Category = "General"
            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Products",
                keyColumn: "ProductId",
                keyValue: 1,
                columns: new[] { "BranchId", "Category" },
                values: new object[] { 3, "General" });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Products",
                keyColumn: "ProductId",
                keyValue: 2,
                columns: new[] { "BranchId", "Category" },
                values: new object[] { 3, "General" });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Products",
                keyColumn: "ProductId",
                keyValue: 3,
                columns: new[] { "BranchId", "Category" },
                values: new object[] { 3, "General" });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Products",
                keyColumn: "ProductId",
                keyValue: 4,
                columns: new[] { "BranchId", "Category" },
                values: new object[] { 3, "General" });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Products",
                keyColumn: "ProductId",
                keyValue: 5,
                columns: new[] { "BranchId", "Category" },
                values: new object[] { 3, "General" });

            // 3. Create Index and Foreign Key Constraint on Products
            migrationBuilder.CreateIndex(
                name: "IX_erp_Products_BranchId",
                schema: "AITStudent",
                table: "erp_Products",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_Products_erp_Branches_BranchId",
                schema: "AITStudent",
                table: "erp_Products",
                column: "BranchId",
                principalSchema: "AITStudent",
                principalTable: "erp_Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_Products_erp_Branches_BranchId",
                schema: "AITStudent",
                table: "erp_Products");

            migrationBuilder.DropIndex(
                name: "IX_erp_Products_BranchId",
                schema: "AITStudent",
                table: "erp_Products");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "AITStudent",
                table: "erp_Products");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "AITStudent",
                table: "erp_Products");
        }
    }
}
