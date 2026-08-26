using Microsoft.EntityFrameworkCore;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Infrastructure.Persistence.Repositories;

public sealed class PolicyTypeRepository(PolicyDbContext dbContext) : IPolicyTypeRepository
{
    public Task<PolicyType?> GetByIdAsync(Guid policyTypeId, CancellationToken cancellationToken = default)
    {
        return dbContext.PolicyTypes.SingleOrDefaultAsync(policyType => policyType.Id == policyTypeId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PolicyType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.PolicyTypes
            .OrderBy(policyType => policyType.Name)
            .ToArrayAsync(cancellationToken);
    }
}