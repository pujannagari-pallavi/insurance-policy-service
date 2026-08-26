using PolicyService.Application.Contracts.Policies;

namespace PolicyService.Application.Services;

public interface IGetPolicyService
{
    Task<PolicyResponse> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PolicyResponse>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}