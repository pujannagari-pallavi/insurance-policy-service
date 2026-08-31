using Microsoft.EntityFrameworkCore;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Infrastructure.Persistence.Repositories;

public sealed class PolicyRepository(PolicyDbContext dbContext) : IPolicyRepository
{
    public Task<Policy?> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        return dbContext.Policies
            .Include(policy => policy.PolicyType)
            .Include(policy => policy.Coverages)
            .Include(policy => policy.History)
            .SingleOrDefaultAsync(policy => policy.Id == policyId, cancellationToken);
    }

    public Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
    {
        return dbContext.Policies
            .Include(policy => policy.PolicyType)
            .Include(policy => policy.Coverages)
            .Include(policy => policy.History)
            .SingleOrDefaultAsync(policy => policy.PolicyNumber == policyNumber.Trim(), cancellationToken);
    }

    public async Task<IReadOnlyCollection<Policy>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Policies
            .AsNoTracking()
            .Include(policy => policy.PolicyType)
            .Include(policy => policy.Coverages)
            .Include(policy => policy.History)
            .Where(policy => policy.CustomerId == customerId)
            .OrderByDescending(policy => policy.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Policy>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Policies
            .AsNoTracking()
            .Include(policy => policy.PolicyType)
            .Include(policy => policy.Coverages)
            .Include(policy => policy.History)
            .OrderByDescending(policy => policy.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        return dbContext.Policies.AddAsync(policy, cancellationToken).AsTask();
    }
}