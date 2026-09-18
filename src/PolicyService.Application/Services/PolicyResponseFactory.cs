using PolicyService.Application.Contracts.Policies;
using PolicyService.Domain.Entities;

namespace PolicyService.Application.Services;

public sealed class PolicyResponseFactory
{
    public PolicyResponse Create(Policy policy)
    {
        var policyType = policy.PolicyType ?? throw new InvalidOperationException("Policy type must be loaded before mapping a policy response.");

        return new PolicyResponse(
            policy.Id,
            policy.PolicyNumber,
            policy.CustomerId,
            new PolicyTypeResponse(
                policyType.Id,
                policyType.Code,
                policyType.Name,
                policyType.Description,
                policyType.BasePremium),
            policy.StartDate,
            policy.EndDate,
            policy.PremiumAmount,
            policy.Status,
            policy.CreatedAtUtc,
            policy.UpdatedAtUtc,
            policy.Coverages
                .Select(coverage => new CoverageResponse(
                    coverage.Id,
                    coverage.Name,
                    coverage.Description,
                    coverage.SumInsured,
                    coverage.Deductible))
                .ToArray(),
            policy.History
                .OrderBy(history => history.ChangedAtUtc)
                .Select(history => new PolicyHistoryResponse(
                    history.Id,
                    history.Action,
                    history.Status,
                    history.Remarks,
                    history.ChangedAtUtc))
                .ToArray(),
            policy.PremiumPlanId,
            policy.PremiumFrequency);
    }

    public PolicyTypeResponse Create(PolicyType policyType)
    {
        return new PolicyTypeResponse(
            policyType.Id,
            policyType.Code,
            policyType.Name,
            policyType.Description,
            policyType.BasePremium);
    }
}