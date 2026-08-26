using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PolicyService.Application.Services;

namespace PolicyService.API.Tests;

public sealed class PolicyApiFactory(
    ICreatePolicyService createPolicyService,
    IGetPolicyService getPolicyService,
    IUpdatePolicyService updatePolicyService,
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
            services.RemoveAll<IPolicyTypeQueryService>();

            services.AddSingleton(createPolicyService);
            services.AddSingleton(getPolicyService);
            services.AddSingleton(updatePolicyService);
            services.AddSingleton(policyTypeQueryService);
        });
    }
}