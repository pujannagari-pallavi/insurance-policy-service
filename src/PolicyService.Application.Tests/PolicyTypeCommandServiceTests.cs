using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Exceptions;
using PolicyService.Application.Services;
using PolicyService.Domain.Entities;
using PolicyService.Domain.Repositories;

namespace PolicyService.Application.Tests;

public sealed class PolicyTypeCommandServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenRequestIsInvalid_ThrowsValidationException()
    {
        var repository = new FakePolicyTypeRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new PolicyTypeCommandService(repository, new PolicyResponseFactory(), unitOfWork);

        var action = () => service.CreateAsync(new CreatePolicyTypeRequest("", "", "", 0));

        var exception = await Assert.ThrowsAsync<ValidationException>(action);
        Assert.Equal("Code, name, description, and a positive base premium are required.", exception.Message);
        Assert.Null(repository.AddedPolicyType);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateAsync_WhenCodeExists_ThrowsValidationException()
    {
        var repository = new FakePolicyTypeRepository(codeExists: true);
        var unitOfWork = new FakeUnitOfWork();
        var service = new PolicyTypeCommandService(repository, new PolicyResponseFactory(), unitOfWork);

        var action = () => service.CreateAsync(new CreatePolicyTypeRequest("health_standard", "Health", "Health cover", 1000));

        var exception = await Assert.ThrowsAsync<ValidationException>(action);
        Assert.Equal("A policy type with this code already exists.", exception.Message);
        Assert.Null(repository.AddedPolicyType);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SetAvailabilityAsync_WhenPolicyTypeExists_UpdatesAvailability(bool isAvailable)
    {
        var policyType = new PolicyType(Guid.NewGuid(), "TRAVEL", "Travel", "Travel cover", 1000);
        var repository = new FakePolicyTypeRepository(policyType: policyType);
        var unitOfWork = new FakeUnitOfWork();
        var service = new PolicyTypeCommandService(repository, new PolicyResponseFactory(), unitOfWork);

        var response = await service.SetAvailabilityAsync(policyType.Id, isAvailable);

        Assert.Equal(isAvailable, policyType.IsAvailable);
        Assert.Equal(isAvailable, response.IsAvailable);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task SetAvailabilityAsync_WhenPolicyTypeDoesNotExist_ThrowsNotFoundException()
    {
        var service = new PolicyTypeCommandService(new FakePolicyTypeRepository(), new PolicyResponseFactory(), new FakeUnitOfWork());

        var action = () => service.SetAvailabilityAsync(Guid.NewGuid(), false);

        await Assert.ThrowsAsync<NotFoundException>(action);
    }

    private sealed class FakePolicyTypeRepository(bool codeExists = false, PolicyType? policyType = null) : IPolicyTypeRepository
    {
        public PolicyType? AddedPolicyType { get; private set; }

        public Task AddAsync(PolicyType policyType, CancellationToken cancellationToken = default)
        {
            AddedPolicyType = policyType;
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(codeExists);
        }

        public Task<PolicyType?> GetByIdAsync(Guid policyTypeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(policyType?.Id == policyTypeId ? policyType : null);
        }

        public Task<IReadOnlyCollection<PolicyType>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<PolicyType>>([]);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }
}