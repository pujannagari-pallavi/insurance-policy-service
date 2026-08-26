using PolicyService.Domain.Repositories;

namespace PolicyService.Infrastructure.Persistence;

public sealed class PolicyUnitOfWork(PolicyDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}