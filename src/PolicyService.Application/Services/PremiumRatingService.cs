using PolicyService.Application.Contracts.Policies;
using PolicyService.Domain.Entities;

namespace PolicyService.Application.Services;

public sealed class PremiumRatingService : IPremiumRatingService
{
    public decimal CalculateAnnualPremium(PolicyType policyType, IReadOnlyCollection<CoverageRequest> coverages, DateOnly startDate, DateOnly endDate)
    {
        var totalSumInsured = coverages.Sum(coverage => coverage.SumInsured);
        var totalDeductible = coverages.Sum(coverage => coverage.Deductible);
        var referenceSumInsured = policyType.Code switch
        {
            "HEALTH_STANDARD" => 500_000m,
            "AUTO_COMPREHENSIVE" => 800_000m,
            "LIFE_PROTECT" => 1_000_000m,
            _ => 500_000m
        };

        var coverageFactor = decimal.Clamp(0.70m + (totalSumInsured / referenceSumInsured) * 0.30m, 0.70m, 2.50m);
        var deductibleDiscount = totalSumInsured == 0
            ? 0m
            : decimal.Min(totalDeductible / totalSumInsured, 0.20m) * 0.25m;
        var termDays = endDate.DayNumber - startDate.DayNumber + 1;
        var termFactor = termDays / 365m;

        return decimal.Round(policyType.BasePremium * coverageFactor * (1m - deductibleDiscount) * termFactor, 2, MidpointRounding.AwayFromZero);
    }
}