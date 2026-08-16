using Multitenant.Web.Client.Auth;
using Multitenant.Web.Client.Constants;
using Multitenant.Web.Client.DTOs.Commons;
using Multitenant.Web.Client.DTOs.Customers;
using Multitenant.Web.Client.Services.Base;
using Multitenant.Web.Client.Services.Contracts;

namespace Multitenant.Web.Client.Services;

/// <summary>GET de clientes. Headers Bearer + X-Tenant-Id los pone ApiClientBase.</summary>
public class CustomerApiService(HttpClient httpClient, ITokenStorage tokenStorage)
    : ApiClientBase(httpClient, tokenStorage), ICustomerApiService
{
    public Task<ApiResponse<IReadOnlyCollection<CustomerItemDto>>> GetCustomersAsync()
        => GetAsync<IReadOnlyCollection<CustomerItemDto>>(ApiEndpoints.Customers.Base);
}
