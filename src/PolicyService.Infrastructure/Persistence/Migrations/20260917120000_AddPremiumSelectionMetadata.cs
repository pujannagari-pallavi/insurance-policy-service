using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyService.Infrastructure.Persistence.Migrations;

public partial class AddPremiumSelectionMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PremiumFrequency",
            table: "Policies",
            type: "character varying(30)",
            maxLength: 30,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PremiumPlanId",
            table: "Policies",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PremiumFrequency",
            table: "Policies");

        migrationBuilder.DropColumn(
            name: "PremiumPlanId",
            table: "Policies");
    }
}
