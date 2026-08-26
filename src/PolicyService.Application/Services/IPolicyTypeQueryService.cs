using PolicyService.Application.Contracts.Policies;

namespace PolicyService.Application.Services;

public interface IPolicyTypeQueryService
{
    Task<IReadOnlyCollection<PolicyTypeResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}