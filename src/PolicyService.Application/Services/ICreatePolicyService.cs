using PolicyService.Application.Contracts.Policies;

namespace PolicyService.Application.Services;

public interface ICreatePolicyService
{
    Task<PolicyResponse> CreateAsync(
        CreatePolicyRequest request,
        PolicyActor actor,
        CancellationToken cancellationToken = default);
}