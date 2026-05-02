using Microsoft.EntityFrameworkCore;
using Multitenant.Application.Abstractions.Persistence;
using Multitenant.Infrastructure.Persistence.Context;
using System.Linq.Expressions;

namespace Multitenant.Infrastructure.Persistence.Repositories;

public class Repository<TEntity>(ApplicationDbContext dbContext) : IRepository<TEntity> 
    where TEntity : class
{
    protected readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, 
        CancellationToken cancellationToken = default)
    {
       var query = _dbContext.Set<TEntity>().AsQueryable();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<TEntity>().FindAsync([id], cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

    public void Update(TEntity entity)
        => _dbContext.Set<TEntity>().Update(entity);

    public void Remove(TEntity entity)
        => _dbContext.Set<TEntity>().Remove(entity);
}
