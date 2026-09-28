using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyService.Infrastructure.Persistence.Migrations;

public partial class ArchiveMalformedSgddPolicyType : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE "PolicyTypes"
            SET "IsAvailable" = FALSE
            WHERE "Code" = 'SGDD'
              AND "Name" = 'cftdtr_hg';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE "PolicyTypes"
            SET "IsAvailable" = TRUE
            WHERE "Code" = 'SGDD'
              AND "Name" = 'cftdtr_hg';
            """);
    }
}