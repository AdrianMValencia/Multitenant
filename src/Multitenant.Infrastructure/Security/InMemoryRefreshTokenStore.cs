namespace Multitenant.Infrastructure.Security;

public class InMemoryRefreshTokenStore
{
    // Diccionario en memoria:
    // Key   = RefreshToken
    // Value = Datos asociados al token
    private readonly Dictionary<string, (Guid UserId, Guid TenantId, DateTime ExpiresAtUtc)> _tokens = [];

    // Objeto usado para sincronizar acceso concurrente (thread-safe)
    private readonly Lock _lock = new();

    // Guarda o actualiza un refresh token
    public void Save(string refreshToken, Guid userId, Guid tenantId, DateTime expiresAtUtc)
    {
        // Solo un hilo puede entrar aquí a la vez
        lock (_lock)
        {
            // Guarda el token junto con su información
            _tokens[refreshToken] = (userId, tenantId, expiresAtUtc);
        }
    }

    // Intenta obtener la información de un refresh token
    public bool TryGet(string refreshToken, out (Guid UserId, Guid TenantId, DateTime ExpiresAtUtc) value)
    {
        // Bloqueo para evitar acceso simultáneo al Dictionary
        lock (_lock)
        {
            if (_tokens.TryGetValue(refreshToken, out var tokenData))
            {
                // Si existe, retorna los datos
                value = tokenData;
                return true;
            }
        }

        // Si no existe, retorna valores por defecto
        value = default;
        return false;
    }

    // Elimina un refresh token
    public void Remove(string refreshToken)
    {
        // Bloqueo para acceso seguro
        lock (_lock)
        {
            // Remueve el token del diccionario
            _tokens.Remove(refreshToken);
        }
    }
}