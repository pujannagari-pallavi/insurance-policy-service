using PolicyService.Application.Abstractions.Validation;
using PolicyService.Application.Contracts.Policies;

namespace PolicyService.Application.Validation.Policies;

public sealed class UpdatePolicyRequestValidator : IValidator<UpdatePolicyRequest>
{
    private readonly CreatePolicyRequestValidator _createPolicyRequestValidator = new();

    public void Validate(UpdatePolicyRequest value)
    {
        _createPolicyRequestValidator.Validate(new CreatePolicyRequest(
            "policy-number",
            value.CustomerId,
            value.PolicyTypeId,
            value.StartDate,
            value.EndDate,
            value.PremiumAmount,
            value.Coverages,
            value.Remarks));
    }
}