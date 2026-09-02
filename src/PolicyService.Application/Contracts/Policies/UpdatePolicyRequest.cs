using PolicyService.Domain.Entities;

namespace PolicyService.Application.Contracts.Policies;

public sealed record UpdatePolicyRequest(
    Guid CustomerId,
    Guid PolicyTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    PolicyStatus Status,
    IReadOnlyCollection<CoverageRequest> Coverages,
    string Remarks);