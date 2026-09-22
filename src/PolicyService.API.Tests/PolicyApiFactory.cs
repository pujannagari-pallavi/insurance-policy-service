using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Services;

namespace PolicyService.API.Tests;

public sealed class PolicyApiFactory(
    ICreatePolicyService createPolicyService,
    IGetPolicyService getPolicyService,
    IUpdatePolicyService updatePolicyService,
    IUnderwritePolicyService underwritePolicyService,
    IPolicyTypeQueryService policyTypeQueryService) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "PolicyService.API")));
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });

            services.RemoveAll<ICreatePolicyService>();
            services.RemoveAll<IGetPolicyService>();
            services.RemoveAll<IUpdatePolicyService>();
            services.RemoveAll<IUnderwritePolicyService>();
            services.RemoveAll<IPolicyTypeQueryService>();
            services.RemoveAll<IPolicyTypeCommandService>();

            services.AddSingleton(createPolicyService);
            services.AddSingleton(getPolicyService);
            services.AddSingleton(updatePolicyService);
            services.AddSingleton(underwritePolicyService);
            services.AddSingleton(policyTypeQueryService);
            services.AddSingleton<IPolicyTypeCommandService, StubPolicyTypeCommandService>();
        });
    }

    private sealed class StubPolicyTypeCommandService : IPolicyTypeCommandService
    {
        public Task<PolicyTypeResponse> CreateAsync(CreatePolicyTypeRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PolicyTypeResponse(Guid.NewGuid(), request.Code, request.Name, request.Description, request.BasePremium));
        }
    }
}