namespace PolicyService.Application.Contracts.Policies;

public sealed record CreatePolicyTypeRequest(
    string Code,
    string Name,
    string Description,
    decimal BasePremium);

public sealed record UpdatePolicyTypeAvailabilityRequest(bool IsAvailable);