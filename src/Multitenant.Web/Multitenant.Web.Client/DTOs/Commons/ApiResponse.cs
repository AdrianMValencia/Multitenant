namespace Multitenant.Web.Client.DTOs.Commons;

/// <summary>Sobre JSON que devuelve la API (BaseResponse en el servidor).</summary>
public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public IEnumerable<ApiError>? Errors { get; set; }
}

public sealed class ApiError
{
    public string? PropertyName { get; set; }
    public string? ErrorMessage { get; set; }
}
