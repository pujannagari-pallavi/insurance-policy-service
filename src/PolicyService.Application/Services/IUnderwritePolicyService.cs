using PolicyService.Application.Contracts.Policies;

namespace PolicyService.Application.Services;

public interface IUnderwritePolicyService
{
    Task<PolicyResponse> TransitionAsync(
        Guid policyId,
        TransitionPolicyStatusRequest request,
        PolicyActor actor,
        CancellationToken cancellationToken = default);
}