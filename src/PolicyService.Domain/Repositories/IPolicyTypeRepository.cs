using PolicyService.Domain.Entities;

namespace PolicyService.Domain.Repositories;

public interface IPolicyTypeRepository
{
    Task<PolicyType?> GetByIdAsync(Guid policyTypeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PolicyType>> GetAllAsync(CancellationToken cancellationToken = default);
}