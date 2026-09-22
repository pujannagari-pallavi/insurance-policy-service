using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyTypeAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "PolicyTypes",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "PolicyTypes",
                keyColumn: "Id",
                keyValue: new Guid("4fba5c78-2de5-4857-9e29-0a091572a001"),
                column: "IsAvailable",
                value: true);

            migrationBuilder.UpdateData(
                table: "PolicyTypes",
                keyColumn: "Id",
                keyValue: new Guid("4fba5c78-2de5-4857-9e29-0a091572a002"),
                column: "IsAvailable",
                value: true);

            migrationBuilder.UpdateData(
                table: "PolicyTypes",
                keyColumn: "Id",
                keyValue: new Guid("4fba5c78-2de5-4857-9e29-0a091572a003"),
                column: "IsAvailable",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "PolicyTypes");
        }
    }
}
