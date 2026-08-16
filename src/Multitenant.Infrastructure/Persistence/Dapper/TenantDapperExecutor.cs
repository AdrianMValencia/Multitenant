using Multitenant.Application.Abstractions.Persistence;
using Multitenant.Infrastructure.Persistence.Context;
using Dapper;
using Multitenant.Application.Abstractions.Multitenancy;

namespace Multitenant.Infrastructure.Persistence.Dapper;

public class TenantDapperExecutor(ApplicationDbContext context, ITenantContext tenantContext) : ITenantDapperExecutor
{
    private readonly ApplicationDbContext _context = context;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var command = BuildTenantCommand(sql, parameters, cancellationToken);
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<T>(command);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var command = BuildTenantCommand(sql, parameters, cancellationToken);
        using var connection = _context.CreateConnection();
        return await connection.ExecuteAsync(command);
    }

    // Método de seguridad que transforma el SQL original inyectando obligatoriamente el TenantId
    private CommandDefinition BuildTenantCommand(string sql, object? parameters, CancellationToken cancellationToken)
    {
        // Validación: No se permite operar en Dapper si no hay un Tenant identificado
        if (_tenantContext.TenantId is null)
        {
            throw new InvalidOperationException("No hay tenant activo para la operación Dapper.");
        }

        // Validación: Exige que el programador use el marcador /**tenant**/ para confirmar que es consciente del filtro
        if (!sql.Contains("/**tenant**/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La consulta Dapper debe incluir el marcador /**tenant**/ para aplicar el filtro global por tenant.");
        }

        // Definimos el predicado SQL para el filtrado
        var tenantPredicate = "\"TenantId\" = @TenantId"; // Comillas: EF creó la columna PascalCase.
        // Reemplazamos el marcador por el filtro SQL real
        var finalSql = sql.Replace("/**tenant**/", tenantPredicate, StringComparison.OrdinalIgnoreCase);

        // Agregamos el ID del Tenant a los parámetros dinámicos de Dapper
        var dynamicParameters = new DynamicParameters(parameters);
        dynamicParameters.Add("TenantId", _tenantContext.TenantId.Value);

        // Devolvemos la definición del comando lista para ser ejecutada
        return new CommandDefinition(finalSql, dynamicParameters, cancellationToken: cancellationToken);
    }
}
