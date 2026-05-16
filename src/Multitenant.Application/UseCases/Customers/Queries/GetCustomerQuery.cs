using Multitenant.Application.Abstractions.Messaging;

namespace Multitenant.Application.UseCases.Customers.Queries;

public sealed record GetCustomerQuery(int Take = 100) : IQuery<IReadOnlyCollection<CustomerItemResponse>>;

public sealed record CustomerItemResponse(Guid Id, string? Name, string? Email, string? Status);
