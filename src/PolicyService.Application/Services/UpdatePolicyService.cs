using PolicyService.Application.Abstractions.Customers;
using PolicyService.Application.Abstractions.Validation;
using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Application.Services;

public sealed class UpdatePolicyService(
    IPolicyRepository policyRepository,
    IPolicyTypeRepository policyTypeRepository,
    IUnitOfWork unitOfWork,
    IValidator<UpdatePolicyRequest> validator,
    ICustomerAccessValidator customerAccessValidator,
    PolicyResponseFactory policyResponseFactory) : IUpdatePolicyService
{
    public async Task<PolicyResponse> UpdateAsync(
        Guid policyId,
        UpdatePolicyRequest request,
        PolicyActor actor,
        CancellationToken cancellationToken = default)
    {
        validator.Validate(request);

        var policy = await policyRepository.GetByIdAsync(policyId, cancellationToken)
            ?? throw new NotFoundException("Policy was not found.");

        await customerAccessValidator.EnsureAccessAsync(
            request.CustomerId,
            actor.IdentityUserId,
            actor.CanManageAnyPolicy,
            cancellationToken);

        var policyType = await policyTypeRepository.GetByIdAsync(request.PolicyTypeId, cancellationToken)
            ?? throw new NotFoundException("Policy type was not found.");

        policy.AttachPolicyType(policyType);
        policy.Update(
            request.CustomerId,
            request.PolicyTypeId,
            request.StartDate,
            request.EndDate,
            request.PremiumAmount,
            request.Status,
            request.Remarks);
        policy.SetCoverages(MapCoverages(request.Coverages));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return policyResponseFactory.Create(policy);
    }

    private static IReadOnlyCollection<Coverage> MapCoverages(IEnumerable<CoverageRequest> requests)
    {
        return requests
            .Select(request => new Coverage(Guid.NewGuid(), request.Name, request.Description, request.SumInsured, request.Deductible))
            .ToArray();
    }
}