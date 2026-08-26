using PolicyService.Application.Contracts.Policies;

namespace PolicyService.Application.Services;

public interface IUpdatePolicyService
{
    Task<PolicyResponse> UpdateAsync(
        Guid policyId,
        UpdatePolicyRequest request,
        PolicyActor actor,
        CancellationToken cancellationToken = default);
}