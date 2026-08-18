using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_System.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "erp_RolePermissions",
                schema: "AITStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsAllowed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_RolePermissions_erp_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_StockAdjustments",
                schema: "AITStudent",
                columns: table => new
                {
                    StockAdjustmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AdjustmentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PreviousQty = table.Column<int>(type: "int", nullable: false),
                    QuantityChange = table.Column<int>(type: "int", nullable: false),
                    NewQty = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_StockAdjustments", x => x.StockAdjustmentId);
                    table.ForeignKey(
                        name: "FK_erp_StockAdjustments_erp_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 10, 1, 57, 68, DateTimeKind.Utc).AddTicks(7456));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 9, 48, 57, 68, DateTimeKind.Utc).AddTicks(7462));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 9, 3, 57, 68, DateTimeKind.Utc).AddTicks(7465));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 8, 3, 57, 68, DateTimeKind.Utc).AddTicks(7469));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 7, 3, 57, 68, DateTimeKind.Utc).AddTicks(7471));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEJgsYLPH0PAL4ZhW35nussL+5r0VUOlgW0oRnGrbOjUE1BzmOwVydsy/S4kpZF+scw==");

            migrationBuilder.CreateIndex(
                name: "IX_erp_RolePermissions_RoleId",
                schema: "AITStudent",
                table: "erp_RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_StockAdjustments_ProductId",
                schema: "AITStudent",
                table: "erp_StockAdjustments",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_RolePermissions",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_StockAdjustments",
                schema: "AITStudent");

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 8, 13, 17, 8, 704, DateTimeKind.Utc).AddTicks(8070));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 8, 13, 4, 8, 704, DateTimeKind.Utc).AddTicks(8080));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 8, 12, 19, 8, 704, DateTimeKind.Utc).AddTicks(8082));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 8, 11, 19, 8, 704, DateTimeKind.Utc).AddTicks(8087));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 8, 10, 19, 8, 704, DateTimeKind.Utc).AddTicks(8089));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEc7PbjF2C2skaKeo57w4w4faaIN2dy1k/Jve63zNCFwMpz3OJ7Wp/EgNYkPXwEkvA==");
        }
    }
}
