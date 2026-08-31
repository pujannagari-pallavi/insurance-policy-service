using PolicyService.Domain.Entities;

namespace PolicyService.Application.Contracts.Policies;

public sealed record TransitionPolicyStatusRequest(PolicyStatus Status, string Remarks);