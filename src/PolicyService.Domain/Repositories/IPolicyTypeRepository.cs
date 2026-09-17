using PolicyService.Domain.Entities;

namespace PolicyService.Domain.Repositories;

public interface IPolicyTypeRepository
{
    Task AddAsync(PolicyType policyType, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<PolicyType?> GetByIdAsync(Guid policyTypeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PolicyType>> GetAllAsync(CancellationToken cancellationToken = default);
}