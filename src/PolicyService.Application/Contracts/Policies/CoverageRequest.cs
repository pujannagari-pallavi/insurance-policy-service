namespace PolicyService.Application.Contracts.Policies;

public sealed record CoverageRequest(
    string Name,
    string Description,
    decimal SumInsured,
    decimal Deductible);