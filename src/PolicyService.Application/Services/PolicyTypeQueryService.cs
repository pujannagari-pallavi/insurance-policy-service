using PolicyService.Application.Contracts.Policies;
using PolicyService.Domain.Repositories;

namespace PolicyService.Application.Services;

public sealed class PolicyTypeQueryService(
    IPolicyTypeRepository policyTypeRepository,
    PolicyResponseFactory policyResponseFactory) : IPolicyTypeQueryService
{
    public async Task<IReadOnlyCollection<PolicyTypeResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var policyTypes = await policyTypeRepository.GetAllAsync(cancellationToken);
        return policyTypes.Select(policyResponseFactory.Create).ToArray();
    }
}