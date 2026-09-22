using PolicyService.Domain.Entities;

namespace PolicyService.Application.Contracts.Policies;

public sealed record PolicyResponse(
    Guid Id,
    string PolicyNumber,
    Guid CustomerId,
    PolicyTypeResponse PolicyType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal PremiumAmount,
    PolicyStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyCollection<CoverageResponse> Coverages,
    IReadOnlyCollection<PolicyHistoryResponse> History,
    string? PremiumPlanId = null,
    string? PremiumFrequency = null);

public sealed record PolicyTypeResponse(
    Guid Id,
    string Code,
    string Name,
    string Description,
    decimal BasePremium,
    bool IsAvailable = true);

public sealed record CoverageResponse(
    Guid Id,
    string Name,
    string Description,
    decimal SumInsured,
    decimal Deductible);

public sealed record PolicyHistoryResponse(
    Guid Id,
    string Action,
    PolicyStatus Status,
    string Remarks,
    DateTime ChangedAtUtc);