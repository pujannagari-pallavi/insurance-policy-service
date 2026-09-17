using Microsoft.EntityFrameworkCore;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Infrastructure.Persistence.Repositories;

public sealed class PolicyTypeRepository(PolicyDbContext dbContext) : IPolicyTypeRepository
{
    public Task AddAsync(PolicyType policyType, CancellationToken cancellationToken = default)
    {
        dbContext.PolicyTypes.Add(policyType);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return dbContext.PolicyTypes.AnyAsync(policyType => policyType.Code == code, cancellationToken);
    }

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