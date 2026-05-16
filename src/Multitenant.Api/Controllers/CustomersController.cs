using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multitenant.Api.Security;
using Multitenant.Application.Abstractions.Messaging;
using Multitenant.Application.UseCases.Customers.Queries;

namespace Multitenant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController(IDispatcher dispatcher) : ControllerBase
    {
        [HttpGet]
        [HasPermission("customers.read")]
        public async Task<IActionResult> GetCustomers(CancellationToken cancellationToken)
        {
            var result = await dispatcher.Dispatch<GetCustomerQuery, IReadOnlyCollection<CustomerItemResponse>>
                (new GetCustomerQuery(), cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
