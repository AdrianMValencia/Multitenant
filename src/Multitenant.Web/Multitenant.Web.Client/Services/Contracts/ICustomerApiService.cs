using Multitenant.Web.Client.DTOs.Commons;
using Multitenant.Web.Client.DTOs.Customers;

namespace Multitenant.Web.Client.Services.Contracts;

public interface ICustomerApiService
{
    /// <summary>GET api/customers. El TenantId va en el JWT / header, no en la URL.</summary>
    Task<ApiResponse<IReadOnlyCollection<CustomerItemDto>>> GetCustomersAsync();
}
