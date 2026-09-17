using PolicyService.Application.Contracts.Policies;

namespace PolicyService.Application.Services;

public interface IPolicyTypeCommandService
{
    Task<PolicyTypeResponse> CreateAsync(CreatePolicyTypeRequest request, CancellationToken cancellationToken = default);
}