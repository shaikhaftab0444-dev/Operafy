using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_System.Migrations
{
    /// <inheritdoc />
    public partial class AddSidebarPortalsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only create erp_AccountHeads as it is new.
            // erp_FinancialYears and erp_Suppliers already exist in the database!
            migrationBuilder.CreateTable(
                name: "erp_AccountHeads",
                schema: "AITStudent",
                columns: table => new
                {
                    AccountHeadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HeadCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HeadName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_AccountHeads", x => x.AccountHeadId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_AccountHeads",
                schema: "AITStudent");
        }
    }
}
