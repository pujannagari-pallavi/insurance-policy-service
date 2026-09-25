using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PolicyService.Infrastructure.Persistence;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 10;

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolicyDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("PolicyDatabaseMigration");

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
                await dbContext.EnsureSeedPolicyTypesAsync(cancellationToken);
                logger.LogInformation("Policy database migration completed.");
                return;
            }
            catch (Exception exception) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    exception,
                    "Policy database migration attempt {Attempt} failed. Retrying.",
                    attempt);

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        await dbContext.Database.MigrateAsync(cancellationToken);
        await dbContext.EnsureSeedPolicyTypesAsync(cancellationToken);
    }

    private static async Task EnsureSeedPolicyTypesAsync(this PolicyDbContext dbContext, CancellationToken cancellationToken)
    {
        var seedPolicyTypes = new[]
        {
            new PolicyService.Domain.Entities.PolicyType(
                PolicyDbContext.HealthPolicyTypeId,
                "HEALTH_STANDARD",
                "Health Standard",
                "Standard individual health policy.",
                12500m),
            new PolicyService.Domain.Entities.PolicyType(
                PolicyDbContext.AutoPolicyTypeId,
                "AUTO_COMPREHENSIVE",
                "Auto Comprehensive",
                "Comprehensive motor insurance coverage.",
                9300m),
            new PolicyService.Domain.Entities.PolicyType(
                PolicyDbContext.LifePolicyTypeId,
                "LIFE_PROTECT",
                "Life Protect",
                "Long-term life protection policy.",
                15000m)
        };

        var existingIds = await dbContext.PolicyTypes
            .Where(policyType => seedPolicyTypes.Select(seed => seed.Id).Contains(policyType.Id))
            .Select(policyType => policyType.Id)
            .ToArrayAsync(cancellationToken);

        var missingPolicyTypes = seedPolicyTypes
            .Where(seed => !existingIds.Contains(seed.Id))
            .ToArray();

        if (missingPolicyTypes.Length == 0) return;

        dbContext.PolicyTypes.AddRange(missingPolicyTypes);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}