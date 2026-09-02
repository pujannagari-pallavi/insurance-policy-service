namespace PolicyService.Application.Contracts.Policies;

public sealed record CreatePolicyRequest(
    Guid CustomerId,
    Guid PolicyTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyCollection<CoverageRequest> Coverages,
    string? Remarks);