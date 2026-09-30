using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PolicyService.Infrastructure.Persistence;

#nullable disable

namespace PolicyService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PolicyDbContext))]
[Migration("20260930110000_ArchiveMalformedPolicyTypes")]
public partial class ArchiveMalformedPolicyTypes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE "PolicyTypes"
            SET "IsAvailable" = FALSE
            WHERE ("Code" = 'SGDD' AND "Name" = 'cftdtr_hg')
               OR ("Code" = 'HOUSE' AND "Name" = 'Housing_loanstuffs');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE "PolicyTypes"
            SET "IsAvailable" = TRUE
            WHERE ("Code" = 'SGDD' AND "Name" = 'cftdtr_hg')
               OR ("Code" = 'HOUSE' AND "Name" = 'Housing_loanstuffs');
            """);
    }
}
