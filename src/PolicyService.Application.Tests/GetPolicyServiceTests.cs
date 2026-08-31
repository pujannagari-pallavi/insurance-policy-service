using PolicyService.Application.Exceptions;
using PolicyService.Application.Services;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Application.Tests;

public sealed class GetPolicyServiceTests
{
    [Fact]
    public async Task GetByIdAsync_WhenPolicyExists_ReturnsMappedResponse()
    {
        var policy = TestPolicyFactory.CreatePolicy();
        var repository = new FakePolicyRepository(policy);
        var service = new GetPolicyService(repository, new PolicyResponseFactory());

        var response = await service.GetByIdAsync(policy.Id);

        Assert.Equal(policy.Id, response.Id);
        Assert.Equal(policy.PolicyNumber, response.PolicyNumber);
        Assert.Single(response.Coverages);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPolicyDoesNotExist_ThrowsNotFoundException()
    {
        var repository = new FakePolicyRepository(null);
        var service = new GetPolicyService(repository, new PolicyResponseFactory());

        var action = () => service.GetByIdAsync(Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal("Policy was not found.", exception.Message);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedPolicies()
    {
        var policy = TestPolicyFactory.CreatePolicy();
        var service = new GetPolicyService(new FakePolicyRepository(policy), new PolicyResponseFactory());

        var response = await service.GetAllAsync();

        var returnedPolicy = Assert.Single(response);
        Assert.Equal(policy.Id, returnedPolicy.Id);
        Assert.Equal(policy.PolicyNumber, returnedPolicy.PolicyNumber);
    }

    private sealed class FakePolicyRepository(Policy? policy) : IPolicyRepository
    {
        public Task<Policy?> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(policy);
        }

        public Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Policy?>(null);
        }

        public Task<IReadOnlyCollection<Policy>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Policy>>(policy is null ? [] : [policy]);
        }

        public Task<IReadOnlyCollection<Policy>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Policy>>(policy is null ? [] : [policy]);
        }

        public Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}