using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasStarterKit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "NewValues",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OldValues",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "EntityName",
                table: "AuditLogs",
                newName: "EventName");

            migrationBuilder.RenameColumn(
                name: "ChangedAt",
                table: "AuditLogs",
                newName: "ChangedDate");

            migrationBuilder.AlterColumn<string>(
                name: "ChangedBy",
                table: "AuditLogs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AuditLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "EventName",
                table: "AuditLogs",
                newName: "EntityName");

            migrationBuilder.RenameColumn(
                name: "ChangedDate",
                table: "AuditLogs",
                newName: "ChangedAt");

            migrationBuilder.AlterColumn<string>(
                name: "ChangedBy",
                table: "AuditLogs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NewValues",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValues",
                table: "AuditLogs",
                type: "text",
                nullable: true);
        }
    }
}
