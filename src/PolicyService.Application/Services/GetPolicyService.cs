using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;
using PolicyService.Domain.Repositories;

namespace PolicyService.Application.Services;

public sealed class GetPolicyService(
    IPolicyRepository policyRepository,
    PolicyResponseFactory policyResponseFactory) : IGetPolicyService
{
    public async Task<PolicyResponse> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        var policy = await policyRepository.GetByIdAsync(policyId, cancellationToken)
            ?? throw new NotFoundException("Policy was not found.");

        return policyResponseFactory.Create(policy);
    }

    public async Task<IReadOnlyCollection<PolicyResponse>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var policies = await policyRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return policies.Select(policyResponseFactory.Create).ToArray();
    }

    public async Task<IReadOnlyCollection<PolicyResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var policies = await policyRepository.GetAllAsync(cancellationToken);
        return policies.Select(policyResponseFactory.Create).ToArray();
    }
}