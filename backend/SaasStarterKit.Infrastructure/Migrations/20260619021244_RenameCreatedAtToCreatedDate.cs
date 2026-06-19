using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasStarterKit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCreatedAtToCreatedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "RefreshTokens",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "AspNetUsers",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "RefreshTokens",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "AspNetUsers",
                newName: "CreateAt");
        }
    }
}
