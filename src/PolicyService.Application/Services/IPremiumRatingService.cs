using PolicyService.Application.Contracts.Policies;
using PolicyService.Domain.Entities;

namespace PolicyService.Application.Services;

public interface IPremiumRatingService
{
    decimal CalculateAnnualPremium(PolicyType policyType, IReadOnlyCollection<CoverageRequest> coverages, DateOnly startDate, DateOnly endDate);
}