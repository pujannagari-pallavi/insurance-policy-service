using PolicyService.Application.Contracts.Policies;

namespace PolicyService.Application.Services;

public interface IPolicyTypeCommandService
{
    Task<PolicyTypeResponse> CreateAsync(CreatePolicyTypeRequest request, CancellationToken cancellationToken = default);

    Task<PolicyTypeResponse> SetAvailabilityAsync(Guid policyTypeId, bool isAvailable, CancellationToken cancellationToken = default);
}