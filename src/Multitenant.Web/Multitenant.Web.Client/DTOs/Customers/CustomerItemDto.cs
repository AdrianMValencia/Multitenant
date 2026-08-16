namespace Multitenant.Web.Client.DTOs.Customers;

/// <summary>Una fila del listado de clientes.</summary>
public sealed record CustomerItemDto(Guid Id, string? Name, string? Email, string? Status);
