using Multitenant.Application.Abstractions.Persistence;
using Multitenant.Infrastructure.Persistence.Context;

namespace Multitenant.Infrastructure.Persistence;

public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
