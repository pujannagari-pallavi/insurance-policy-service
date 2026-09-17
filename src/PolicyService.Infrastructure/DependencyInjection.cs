using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PolicyService.Application.Abstractions.Customers;
using PolicyService.Application.Abstractions.Validation;
using PolicyService.Application.Contracts.Policies;
using PolicyService.Application.Services;
using PolicyService.Application.Validation.Policies;
using PolicyService.Domain.Repositories;
using PolicyService.Infrastructure.Customers;
using PolicyService.Infrastructure.Persistence;
using PolicyService.Infrastructure.Persistence.Repositories;

namespace PolicyService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PolicyDbContext>(options =>
            options.UseNpgsql(NormalizePostgresConnectionString(configuration.GetConnectionString("PolicyDatabase"))));

        var customerServiceBaseUrl = configuration["Services:Customer:BaseUrl"]
            ?? throw new InvalidOperationException("Customer Service base URL is missing.");

        services.AddHttpClient<ICustomerAccessValidator, CustomerAccessValidator>(client =>
        {
            client.BaseAddress = new Uri(customerServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddScoped<IUnitOfWork, PolicyUnitOfWork>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IPolicyTypeRepository, PolicyTypeRepository>();
        services.AddScoped<IValidator<CreatePolicyRequest>, CreatePolicyRequestValidator>();
        services.AddScoped<IValidator<UpdatePolicyRequest>, UpdatePolicyRequestValidator>();
        services.AddScoped<PolicyResponseFactory>();
        services.AddScoped<IPremiumRatingService, PremiumRatingService>();
        services.AddScoped<ICreatePolicyService, CreatePolicyService>();
        services.AddScoped<IGetPolicyService, GetPolicyService>();
        services.AddScoped<IUpdatePolicyService, UpdatePolicyService>();
        services.AddScoped<IUnderwritePolicyService, UnderwritePolicyService>();
        services.AddScoped<IPolicyTypeQueryService, PolicyTypeQueryService>();
        services.AddScoped<IPolicyTypeCommandService, PolicyTypeCommandService>();

        return services;
    }

    private static string NormalizePostgresConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'PolicyDatabase' was not found.");
        }

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var connectionUri)
            || (connectionUri.Scheme is not "postgres" and not "postgresql"))
        {
            return connectionString;
        }

        var credentials = connectionUri.UserInfo.Split(':', 2);
        if (credentials.Length != 2 || string.IsNullOrWhiteSpace(connectionUri.AbsolutePath.Trim('/')))
        {
            throw new InvalidOperationException("The PostgreSQL connection URL must include a username, password, and database name.");
        }

        return new NpgsqlConnectionStringBuilder
        {
            Host = connectionUri.Host,
            Port = connectionUri.IsDefaultPort ? 5432 : connectionUri.Port,
            Database = Uri.UnescapeDataString(connectionUri.AbsolutePath.Trim('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = Uri.UnescapeDataString(credentials[1]),
            SslMode = SslMode.Require
        }.ConnectionString;
    }
}