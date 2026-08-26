using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PolicyService.Infrastructure.Persistence;

public sealed class PolicyDbContextFactory : IDesignTimeDbContextFactory<PolicyDbContext>
{
    public PolicyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PolicyDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=InsurancePlatformPolicyDb;Username=postgres;Password=admin");

        return new PolicyDbContext(optionsBuilder.Options);
    }
}