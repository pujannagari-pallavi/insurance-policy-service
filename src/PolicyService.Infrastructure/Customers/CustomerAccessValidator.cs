using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using PolicyService.Application.Abstractions.Customers;
using PolicyService.Application.Exceptions;

namespace PolicyService.Infrastructure.Customers;

public sealed class CustomerAccessValidator(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor) : ICustomerAccessValidator
{
    public async Task EnsureAccessAsync(
        Guid customerId,
        Guid identityUserId,
        bool canManageAnyPolicy,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/customers/{customerId}");
        var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization))
        {
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization);
        }

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException("Customer was not found.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ServiceUnavailableException("Customer validation is temporarily unavailable.");
            }

            var customer = await response.Content.ReadFromJsonAsync<CustomerLookupResponse>(cancellationToken: cancellationToken)
                ?? throw new ServiceUnavailableException("Customer validation returned an invalid response.");

            if (!customer.IsActive)
            {
                throw new ValidationException("Policies cannot be managed for an inactive customer.");
            }

            if (!canManageAnyPolicy && customer.IdentityUserId != identityUserId)
            {
                throw new ForbiddenException("You are not allowed to manage policies for this customer.");
            }
        }
        catch (HttpRequestException exception)
        {
            throw new ServiceUnavailableException("Customer validation is temporarily unavailable.", exception);
        }
    }

    private sealed record CustomerLookupResponse(Guid Id, Guid? IdentityUserId, bool IsActive);
}