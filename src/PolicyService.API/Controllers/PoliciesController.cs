using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolicyService.Application.Abstractions.Customers;
using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Services;

namespace PolicyService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class PoliciesController(
    ICreatePolicyService createPolicyService,
    IGetPolicyService getPolicyService,
    IUpdatePolicyService updatePolicyService,
    IUnderwritePolicyService underwritePolicyService,
    IPolicyTypeQueryService policyTypeQueryService,
    IPolicyTypeCommandService policyTypeCommandService,
    ICustomerAccessValidator customerAccessValidator) : ControllerBase
{
    [HttpGet("mine")]
    [ProducesResponseType<IReadOnlyCollection<PolicyResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine([FromQuery] Guid customerId, CancellationToken cancellationToken)
    {
        var actor = GetActor();
        await customerAccessValidator.EnsureAccessAsync(customerId, actor.IdentityUserId, actor.CanManageAnyPolicy, cancellationToken: cancellationToken);
        return Ok(await getPolicyService.GetByCustomerIdAsync(customerId, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<PolicyResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        if (!GetActor().CanManageAnyPolicy)
        {
            return Forbid();
        }

        return Ok(await getPolicyService.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType<PolicyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CreatePolicyRequest request, CancellationToken cancellationToken)
    {
        var response = await createPolicyService.CreateAsync(request, GetActor(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{policyId:guid}")]
    [ProducesResponseType<PolicyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid policyId, CancellationToken cancellationToken)
    {
        var response = await getPolicyService.GetByIdAsync(policyId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{policyId:guid}")]
    [ProducesResponseType<PolicyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid policyId, UpdatePolicyRequest request, CancellationToken cancellationToken)
    {
        var response = await updatePolicyService.UpdateAsync(policyId, request, GetActor(), cancellationToken);
        return Ok(response);
    }

    [HttpPatch("{policyId:guid}/status")]
    [ProducesResponseType<PolicyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(
        Guid policyId,
        TransitionPolicyStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await underwritePolicyService.TransitionAsync(policyId, request, GetActor(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("types")]
    [ProducesResponseType<IReadOnlyCollection<PolicyTypeResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPolicyTypes(CancellationToken cancellationToken)
    {
        var response = await policyTypeQueryService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("types")]
    [ProducesResponseType<PolicyTypeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePolicyType(CreatePolicyTypeRequest request, CancellationToken cancellationToken)
    {
        if (!User.HasClaim("permission", "Policy.Write.Any")) return Forbid();
        var response = await policyTypeCommandService.CreateAsync(request, cancellationToken);
        return Created($"api/policies/types/{response.Id}", response);
    }

    [HttpPatch("types/{policyTypeId:guid}/availability")]
    [ProducesResponseType<PolicyTypeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPolicyTypeAvailability(
        Guid policyTypeId,
        UpdatePolicyTypeAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.HasClaim("permission", "Policy.Write.Any")) return Forbid();
        var response = await policyTypeCommandService.SetAvailabilityAsync(policyTypeId, request.IsAvailable, cancellationToken);
        return Ok(response);
    }

    private PolicyActor GetActor()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(identityUserId, out var userId))
        {
            throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");
        }

        return new PolicyActor(
            userId,
            User.HasClaim("permission", "Policy.Write.Any"),
            User.HasClaim("permission", "Policy.Approve"));
    }
}