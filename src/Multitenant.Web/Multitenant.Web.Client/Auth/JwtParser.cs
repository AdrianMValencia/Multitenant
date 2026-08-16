using System.Security.Claims;
using System.Text.Json;

namespace Multitenant.Web.Client.Auth;

public class JwtParser
{
    /// <summary>Parte 2 del JWT (payload) → claims. Sin validar firma (eso lo hace la API).</summary>
    public static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3) return [];

        var payload = parts[1];
        var padded = (payload.Length % 4) switch
        {
            2 => payload + "==",
            3 => payload + "=",
            _ => payload
        };

        var bytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
        var json = System.Text.Encoding.UTF8.GetString(bytes);

        using var doc = JsonDocument.Parse(json);
        var claims = new List<Claim>();

        foreach (var element in doc.RootElement.EnumerateObject())
        {
            if (element.Value.ValueKind == JsonValueKind.Array)
            {
                claims.AddRange(element.Value.EnumerateArray()
                    .Select(v => new Claim(element.Name, v.GetString() ?? string.Empty)));
            }
            else
            {
                claims.Add(new Claim(element.Name, element.Value.ToString()));
            }
        }

        return claims;
    }
}
