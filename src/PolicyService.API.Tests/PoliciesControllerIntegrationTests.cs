using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;
using PolicyService.Application.Services;
using PolicyService.Domain.Entities;

namespace PolicyService.API.Tests;

public sealed class PoliciesControllerIntegrationTests
{
    [Fact]
    public async Task Create_WhenServiceThrowsValidationException_ReturnsBadRequestProblemDetails()
    {
        using var factory = new PolicyApiFactory(
            new ThrowingCreatePolicyService(new ValidationException("Policy number is required.")),
            new StubGetPolicyService(),
            new StubUpdatePolicyService(),
            new StubUnderwritePolicyService(),
            new StubPolicyTypeQueryService());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/policies", CreateRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem!.Status);
        Assert.Equal("Validation failed.", problem.Title);
        Assert.Equal("Policy number is required.", problem.Detail);
    }

    [Fact]
    public async Task GetById_WhenServiceThrowsNotFoundException_ReturnsNotFoundProblemDetails()
    {
        using var factory = new PolicyApiFactory(
            new StubCreatePolicyService(),
            new ThrowingGetPolicyService(new NotFoundException("Policy was not found.")),
            new StubUpdatePolicyService(),
            new StubUnderwritePolicyService(),
            new StubPolicyTypeQueryService());
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/policies/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem!.Status);
        Assert.Equal("Resource not found.", problem.Title);
        Assert.Equal("Policy was not found.", problem.Detail);
    }

    [Fact]
    public async Task Health_ReturnsHealthyResponse()
    {
        using var factory = new PolicyApiFactory(
            new StubCreatePolicyService(),
            new StubGetPolicyService(),
            new StubUpdatePolicyService(),
            new StubUnderwritePolicyService(),
            new StubPolicyTypeQueryService());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class ThrowingCreatePolicyService(Exception exception) : ICreatePolicyService
    {
        public Task<PolicyResponse> CreateAsync(
            CreatePolicyRequest request,
            PolicyActor actor,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<PolicyResponse>(exception);
        }
    }

    private sealed class ThrowingGetPolicyService(Exception exception) : IGetPolicyService
    {
        public Task<PolicyResponse> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
        {
            return Task.FromException<PolicyResponse>(exception);
        }

        public Task<IReadOnlyCollection<PolicyResponse>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IReadOnlyCollection<PolicyResponse>>(exception);
        }

        public Task<IReadOnlyCollection<PolicyResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromException<IReadOnlyCollection<PolicyResponse>>(exception);
        }
    }

    private sealed class StubCreatePolicyService : ICreatePolicyService
    {
        public Task<PolicyResponse> CreateAsync(
            CreatePolicyRequest request,
            PolicyActor actor,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResponse());
        }
    }

    private sealed class StubGetPolicyService : IGetPolicyService
    {
        public Task<PolicyResponse> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResponse(policyId));
        }

        public Task<IReadOnlyCollection<PolicyResponse>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<PolicyResponse>>([CreateResponse()]);
        }

        public Task<IReadOnlyCollection<PolicyResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<PolicyResponse>>([CreateResponse()]);
        }
    }

    private sealed class StubUpdatePolicyService : IUpdatePolicyService
    {
        public Task<PolicyResponse> UpdateAsync(
            Guid policyId,
            UpdatePolicyRequest request,
            PolicyActor actor,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResponse(policyId));
        }
    }

    private sealed class StubUnderwritePolicyService : IUnderwritePolicyService
    {
        public Task<PolicyResponse> TransitionAsync(
            Guid policyId,
            TransitionPolicyStatusRequest request,
            PolicyActor actor,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateResponse(policyId));
        }
    }

    private sealed class StubPolicyTypeQueryService : IPolicyTypeQueryService
    {
        public Task<IReadOnlyCollection<PolicyTypeResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<PolicyTypeResponse>>([
                new PolicyTypeResponse(Guid.NewGuid(), "HEALTH_STANDARD", "Health Standard", "Standard individual health policy.", 12500m)
            ]);
        }
    }

    private static PolicyResponse CreateResponse(Guid? policyId = null)
    {
        return new PolicyResponse(
            policyId ?? Guid.NewGuid(),
            "POL-2026-0001",
            Guid.NewGuid(),
            new PolicyTypeResponse(Guid.NewGuid(), "HEALTH_STANDARD", "Health Standard", "Standard individual health policy.", 12500m),
            new DateOnly(2026, 8, 1),
            new DateOnly(2027, 7, 31),
            14000m,
            PolicyStatus.Draft,
            DateTime.UtcNow,
            null,
            [new CoverageResponse(Guid.NewGuid(), "Hospitalization", "In-patient hospitalization coverage.", 500000m, 10000m)],
            [new PolicyHistoryResponse(Guid.NewGuid(), "Created", PolicyStatus.Draft, "Initial policy creation.", DateTime.UtcNow)]);
    }

    private static CreatePolicyRequest CreateRequest()
    {
        return new CreatePolicyRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 8, 1),
            new DateOnly(2027, 7, 31),
            [new CoverageRequest("Hospitalization", "In-patient hospitalization coverage.", 500000m, 10000m)],
            "Initial policy creation.");
    }
}