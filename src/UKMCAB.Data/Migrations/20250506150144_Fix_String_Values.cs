using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKMCAB.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fix_String_Values : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "WorkflowTasks",
                type: "varchar(36)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "UserAccounts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "UserAccountRequests",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SubCategories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PurposeOfAppointments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProtectionAgainstRisks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Procedures",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PpeProductTypes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PpeCategories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "LegislativeAreas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Regulation",
                table: "LegislativeAreas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "LegislativeAreas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "URLSlug",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "UKASReference",
                table: "Documents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1024)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubStatus",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "StatusValue",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Documents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1024)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserGroup",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "CABNumber",
                table: "Documents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1024)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CABId",
                table: "Documents",
                type: "varchar(36)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "Documents",
                type: "varchar(36)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DesignatedStandards",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AreaOfCompetencies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "WorkflowTasks",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "UserAccounts",
                type: "varchar(1000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "UserAccountRequests",
                type: "varchar(1000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SubCategories",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PurposeOfAppointments",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProtectionAgainstRisks",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Procedures",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PpeProductTypes",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PpeCategories",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "LegislativeAreas",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Regulation",
                table: "LegislativeAreas",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "LegislativeAreas",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "Documents",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "URLSlug",
                table: "Documents",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UKASReference",
                table: "Documents",
                type: "varchar(1024)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubStatus",
                table: "Documents",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "StatusValue",
                table: "Documents",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Documents",
                type: "varchar(1024)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserGroup",
                table: "Documents",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CABNumber",
                table: "Documents",
                type: "varchar(1024)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CABId",
                table: "Documents",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "Documents",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DesignatedStandards",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AreaOfCompetencies",
                type: "varchar(1024)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
