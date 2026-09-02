using PolicyService.Application.Abstractions.Customers;
using PolicyService.Application.Abstractions.Validation;
using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Application.Services;

public sealed class CreatePolicyService(
    IPolicyRepository policyRepository,
    IPolicyTypeRepository policyTypeRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreatePolicyRequest> validator,
    ICustomerAccessValidator customerAccessValidator,
    IPremiumRatingService premiumRatingService,
    PolicyResponseFactory policyResponseFactory) : ICreatePolicyService
{
    public async Task<PolicyResponse> CreateAsync(
        CreatePolicyRequest request,
        PolicyActor actor,
        CancellationToken cancellationToken = default)
    {
        validator.Validate(request);

        await customerAccessValidator.EnsureAccessAsync(
            request.CustomerId,
            actor.IdentityUserId,
            actor.CanManageAnyPolicy,
            requireVerifiedKyc: true,
            cancellationToken);

        var policyType = await policyTypeRepository.GetByIdAsync(request.PolicyTypeId, cancellationToken)
            ?? throw new NotFoundException("Policy type was not found.");

        var premiumAmount = premiumRatingService.CalculateAnnualPremium(
            policyType,
            request.Coverages,
            request.StartDate,
            request.EndDate);

        var policy = new Policy(
            Guid.NewGuid(),
            GeneratePolicyNumber(),
            request.CustomerId,
            request.PolicyTypeId,
            request.StartDate,
            request.EndDate,
            premiumAmount);

        policy.AttachPolicyType(policyType);
        policy.SetCoverages(MapCoverages(request.Coverages));
        policy.AddHistory("Created", policy.Status, request.Remarks ?? "Policy created.");

        await policyRepository.AddAsync(policy, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return policyResponseFactory.Create(policy);
    }

    private static IReadOnlyCollection<Coverage> MapCoverages(IEnumerable<CoverageRequest> requests)
    {
        return requests
            .Select(request => new Coverage(Guid.NewGuid(), request.Name, request.Description, request.SumInsured, request.Deductible))
            .ToArray();
    }

    private static string GeneratePolicyNumber()
    {
        return $"SC-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";
    }
}