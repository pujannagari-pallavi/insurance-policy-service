namespace PolicyService.Application.Contracts.Policies;

public sealed record CreatePolicyRequest(
    string PolicyNumber,
    Guid CustomerId,
    Guid PolicyTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal PremiumAmount,
    IReadOnlyCollection<CoverageRequest> Coverages,
    string? Remarks);