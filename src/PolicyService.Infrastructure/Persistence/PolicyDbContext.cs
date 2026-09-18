using Microsoft.EntityFrameworkCore;
using PolicyService.Domain.Entities;

namespace PolicyService.Infrastructure.Persistence;

public sealed class PolicyDbContext(DbContextOptions<PolicyDbContext> options) : DbContext(options)
{
    internal static readonly Guid HealthPolicyTypeId = Guid.Parse("4FBA5C78-2DE5-4857-9E29-0A091572A001");
    internal static readonly Guid AutoPolicyTypeId = Guid.Parse("4FBA5C78-2DE5-4857-9E29-0A091572A002");
    internal static readonly Guid LifePolicyTypeId = Guid.Parse("4FBA5C78-2DE5-4857-9E29-0A091572A003");

    public DbSet<Policy> Policies => Set<Policy>();

    public DbSet<PolicyType> PolicyTypes => Set<PolicyType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurePolicy(modelBuilder);
        ConfigurePolicyType(modelBuilder);
        SeedPolicyTypes(modelBuilder);
    }

    private static void ConfigurePolicy(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Policy>();

        builder.ToTable("Policies");
        builder.HasKey(policy => policy.Id);
        builder.Property(policy => policy.PolicyNumber).HasMaxLength(50).IsRequired();
        builder.Property(policy => policy.PremiumAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(policy => policy.PremiumPlanId).HasMaxLength(100);
        builder.Property(policy => policy.PremiumFrequency).HasMaxLength(30);
        builder.Property(policy => policy.Status).IsRequired();
        builder.Property(policy => policy.CreatedAtUtc).IsRequired();
        builder.HasIndex(policy => policy.PolicyNumber).IsUnique();

        builder.HasOne(policy => policy.PolicyType)
            .WithMany()
            .HasForeignKey(policy => policy.PolicyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata.FindNavigation(nameof(Policy.Coverages))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Policy.History))?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(policy => policy.Coverages, coverageBuilder =>
        {
            coverageBuilder.ToTable("PolicyCoverages");
            coverageBuilder.WithOwner().HasForeignKey("PolicyId");
            coverageBuilder.HasKey(coverage => coverage.Id);
            coverageBuilder.Property(coverage => coverage.Id).ValueGeneratedNever();
            coverageBuilder.Property(coverage => coverage.Name).HasMaxLength(100).IsRequired();
            coverageBuilder.Property(coverage => coverage.Description).HasMaxLength(256).IsRequired();
            coverageBuilder.Property(coverage => coverage.SumInsured).HasColumnType("decimal(18,2)").IsRequired();
            coverageBuilder.Property(coverage => coverage.Deductible).HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsMany(policy => policy.History, historyBuilder =>
        {
            historyBuilder.ToTable("PolicyHistory");
            historyBuilder.WithOwner().HasForeignKey("PolicyId");
            historyBuilder.HasKey(history => history.Id);
            historyBuilder.Property(history => history.Id).ValueGeneratedNever();
            historyBuilder.Property(history => history.Action).HasMaxLength(100).IsRequired();
            historyBuilder.Property(history => history.Remarks).HasMaxLength(512).IsRequired();
            historyBuilder.Property(history => history.Status).IsRequired();
            historyBuilder.Property(history => history.ChangedAtUtc).IsRequired();
        });
    }

    private static void ConfigurePolicyType(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<PolicyType>();

        builder.ToTable("PolicyTypes");
        builder.HasKey(policyType => policyType.Id);
        builder.Property(policyType => policyType.Code).HasMaxLength(50).IsRequired();
        builder.Property(policyType => policyType.Name).HasMaxLength(100).IsRequired();
        builder.Property(policyType => policyType.Description).HasMaxLength(256).IsRequired();
        builder.Property(policyType => policyType.BasePremium).HasColumnType("decimal(18,2)").IsRequired();
        builder.HasIndex(policyType => policyType.Code).IsUnique();
    }

    private static void SeedPolicyTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PolicyType>().HasData(
            new
            {
                Id = HealthPolicyTypeId,
                Code = "HEALTH_STANDARD",
                Name = "Health Standard",
                Description = "Standard individual health policy.",
                BasePremium = 12500m
            },
            new
            {
                Id = AutoPolicyTypeId,
                Code = "AUTO_COMPREHENSIVE",
                Name = "Auto Comprehensive",
                Description = "Comprehensive motor insurance coverage.",
                BasePremium = 9300m
            },
            new
            {
                Id = LifePolicyTypeId,
                Code = "LIFE_PROTECT",
                Name = "Life Protect",
                Description = "Long-term life protection policy.",
                BasePremium = 15000m
            });
    }
}