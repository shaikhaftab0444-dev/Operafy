using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_System.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.CreateTable(
                name: "erp_Payslips",
                schema: "AITStudent",
                columns: table => new
                {
                    PayslipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PayPeriod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HRA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransportAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MedicalAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProvidentFund = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProfessionalTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidDays = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_Payslips", x => x.PayslipId);
                    table.ForeignKey(
                        name: "FK_erp_Payslips_erp_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "erp_SalaryStructures",
                schema: "AITStudent",
                columns: table => new
                {
                    SalaryStructureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HRA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransportAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MedicalAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProvidentFund = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProfessionalTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_SalaryStructures", x => x.SalaryStructureId);
                    table.ForeignKey(
                        name: "FK_erp_SalaryStructures_erp_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AITStudent",
                        principalTable: "erp_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 8, 5, 2, 12, 198, DateTimeKind.Utc).AddTicks(8874));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 8, 4, 49, 12, 198, DateTimeKind.Utc).AddTicks(8881));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 8, 4, 4, 12, 198, DateTimeKind.Utc).AddTicks(8883));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 8, 3, 4, 12, 198, DateTimeKind.Utc).AddTicks(8887));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 8, 2, 4, 12, 198, DateTimeKind.Utc).AddTicks(8889));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEAtdX9ohP7I1B2v0j4AbukABaUM2mGEXNL3JKAVtpt4r3Ldq0ILHB2XZpATLMYg8mg==");

            migrationBuilder.CreateIndex(
                name: "IX_erp_Payslips_UserId",
                schema: "AITStudent",
                table: "erp_Payslips",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_SalaryStructures_UserId",
                schema: "AITStudent",
                table: "erp_SalaryStructures",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DropTable(
                name: "erp_Payslips",
                schema: "AITStudent");

            migrationBuilder.DropTable(
                name: "erp_SalaryStructures",
                schema: "AITStudent");

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 5, 12, 24, 53, 217, DateTimeKind.Utc).AddTicks(2107));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 5, 12, 11, 53, 217, DateTimeKind.Utc).AddTicks(2113));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 5, 11, 26, 53, 217, DateTimeKind.Utc).AddTicks(2116));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 5, 10, 26, 53, 217, DateTimeKind.Utc).AddTicks(2120));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_ActivityLogs",
                keyColumn: "ActivityLogId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 5, 9, 26, 53, 217, DateTimeKind.Utc).AddTicks(2123));

            migrationBuilder.UpdateData(
                schema: "AITStudent",
                table: "erp_Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEOkJcAU1YEZ50GcXjw9Sn+CYrXr+BWC75/EPUpfVliWCv4Alu/+3memoVLfE2G515w==");
        }
    }
}
