using PolicyService.Application.Abstractions.Validation;
using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;

namespace PolicyService.Application.Validation.Policies;

public sealed class CreatePolicyRequestValidator : IValidator<CreatePolicyRequest>
{
    public void Validate(CreatePolicyRequest value)
    {
        if (string.IsNullOrWhiteSpace(value.PolicyNumber))
        {
            throw new ValidationException("Policy number is required.");
        }

        if (value.CustomerId == Guid.Empty)
        {
            throw new ValidationException("Customer id is required.");
        }

        if (value.PolicyTypeId == Guid.Empty)
        {
            throw new ValidationException("Policy type id is required.");
        }

        if (value.StartDate > value.EndDate)
        {
            throw new ValidationException("Policy start date cannot be after the end date.");
        }

        if (value.Coverages.Count == 0)
        {
            throw new ValidationException("At least one coverage is required.");
        }

        foreach (var coverage in value.Coverages)
        {
            if (string.IsNullOrWhiteSpace(coverage.Name))
            {
                throw new ValidationException("Coverage name is required.");
            }

            if (coverage.SumInsured <= 0)
            {
                throw new ValidationException("Coverage sum insured must be greater than zero.");
            }

            if (coverage.Deductible < 0)
            {
                throw new ValidationException("Coverage deductible cannot be negative.");
            }
        }
    }
}