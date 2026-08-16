namespace Multitenant.Web.Client.Constants;

/// <summary>
/// Rutas relativas a ApiSettings:BaseUrl (ej. https://localhost:7299/).
/// </summary>
public static class ApiEndpoints
{
    public static class Auth
    {
        public const string Login = "api/auth/login";
        public const string Refresh = "api/auth/refresh";
        public const string Logout = "api/auth/logout";
    }

    public static class Customers
    {
        public const string Base = "api/customers";
    }

    public static class Users
    {
        public const string Base = "api/users";
    }
}
