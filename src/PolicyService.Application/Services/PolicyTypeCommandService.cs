using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;
using System.Text.RegularExpressions;

namespace PolicyService.Application.Services;

public sealed class PolicyTypeCommandService(
    IPolicyTypeRepository policyTypeRepository,
    PolicyResponseFactory policyResponseFactory,
    IUnitOfWork unitOfWork) : IPolicyTypeCommandService
{
    private const decimal MinimumBasePremium = 100m;
    private const decimal MaximumBasePremium = 1_000_000m;
    private static readonly Regex ProductCodePattern = new("^[A-Z][A-Z0-9_]{2,49}$", RegexOptions.Compiled);

    public async Task<PolicyTypeResponse> CreateAsync(CreatePolicyTypeRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();
        var description = request.Description.Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description) || request.BasePremium <= 0)
            throw new ValidationException("Code, name, description, and a positive base premium are required.");
        if (!ProductCodePattern.IsMatch(code))
            throw new ValidationException("Policy product code must be 3-50 characters and use uppercase letters, numbers, or underscores.");
        if (name.Length < 3 || name.Length > 100 || description.Length < 10 || description.Length > 256)
            throw new ValidationException("Policy product name must be 3-100 characters and description must be 10-256 characters.");
        if (request.BasePremium < MinimumBasePremium || request.BasePremium > MaximumBasePremium)
            throw new ValidationException("Policy product base premium must be between INR 100 and INR 1,000,000.");
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