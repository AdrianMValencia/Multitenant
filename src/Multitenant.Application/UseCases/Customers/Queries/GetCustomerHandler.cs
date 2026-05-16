using Multitenant.Application.Abstractions.Messaging;
using Multitenant.Application.Abstractions.Persistence;
using Multitenant.Application.Commons.Bases;

namespace Multitenant.Application.UseCases.Customers.Queries;

public class GetCustomerHandler(ITenantDapperExecutor dapperExecutor)
    : IQueryHandler<GetCustomerQuery, IReadOnlyCollection<CustomerItemResponse>>
{
    public async Task<BaseResponse<IReadOnlyCollection<CustomerItemResponse>>> Handle(
        GetCustomerQuery query, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT "CustomerId" AS Id,
                                  "Name" AS Name,
                                  "Email" AS Email,
                                  "Status" AS Status
                           FROM public."Customers"
                           WHERE /**tenant**/
                           ORDER BY "CreatedAt" DESC NULLS LAST
                           LIMIT @Take;
                           """;

        var rows = await dapperExecutor.QueryAsync<CustomerItemResponse>(sql, new { query.Take }, cancellationToken);
        var data = rows.ToArray();

        return new BaseResponse<IReadOnlyCollection<CustomerItemResponse>>
        {
            IsSuccess = true,
            Message = "Customers retrieved successfully.",
            Data = data
        };
    }
}
