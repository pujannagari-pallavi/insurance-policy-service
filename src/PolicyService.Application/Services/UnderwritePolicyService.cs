using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Application.Services;

public sealed class UnderwritePolicyService(
    IPolicyRepository policyRepository,
    IUnitOfWork unitOfWork,
    PolicyResponseFactory policyResponseFactory) : IUnderwritePolicyService
{
    public async Task<PolicyResponse> TransitionAsync(
        Guid policyId,
        TransitionPolicyStatusRequest request,
        PolicyActor actor,
        CancellationToken cancellationToken = default)
    {
        if (!actor.CanManageAnyPolicy)
        {
            throw new ForbiddenException("Only an underwriter can change a policy status.");
        }

        if (string.IsNullOrWhiteSpace(request.Remarks))
        {
            throw new ValidationException("Underwriting remarks are required.");
        }

        var policy = await policyRepository.GetByIdAsync(policyId, cancellationToken)
            ?? throw new NotFoundException("Policy was not found.");

        if (!Policy.CanTransition(policy.Status, request.Status))
        {
            throw new ValidationException($"A policy cannot transition from {policy.Status} to {request.Status}.");
        }

        policy.TransitionTo(request.Status, request.Remarks);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return policyResponseFactory.Create(policy);
    }
}