namespace Multitenant.Application.Abstractions.Persistence;

public interface ITenantDapperExecutor
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);
}
