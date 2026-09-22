using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Application.Services;

public sealed class PolicyTypeCommandService(
    IPolicyTypeRepository policyTypeRepository,
    PolicyResponseFactory policyResponseFactory,
    IUnitOfWork unitOfWork) : IPolicyTypeCommandService
{
    public async Task<PolicyTypeResponse> CreateAsync(CreatePolicyTypeRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();
        var description = request.Description.Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description) || request.BasePremium <= 0)
            throw new ValidationException("Code, name, description, and a positive base premium are required.");
        if (await policyTypeRepository.ExistsByCodeAsync(code, cancellationToken))
            throw new ValidationException("A policy type with this code already exists.");

        var policyType = new PolicyType(Guid.NewGuid(), code, name, description, request.BasePremium);
        await policyTypeRepository.AddAsync(policyType, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return policyResponseFactory.Create(policyType);
    }

    public async Task<PolicyTypeResponse> SetAvailabilityAsync(Guid policyTypeId, bool isAvailable, CancellationToken cancellationToken = default)
    {
        var policyType = await policyTypeRepository.GetByIdAsync(policyTypeId, cancellationToken)
            ?? throw new NotFoundException("Policy type was not found.");

        policyType.SetAvailability(isAvailable);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return policyResponseFactory.Create(policyType);
    }
}