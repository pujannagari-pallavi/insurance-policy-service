using PolicyService.Domain.Entities;

namespace PolicyService.Domain.Repositories;

public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default);

    Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Policy>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task AddAsync(Policy policy, CancellationToken cancellationToken = default);
}