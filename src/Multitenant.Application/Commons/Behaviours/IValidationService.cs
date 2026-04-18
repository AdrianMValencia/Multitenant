namespace Multitenant.Application.Commons.Behaviours;

public interface IValidationService
{
    Task ValidateAsync<T>(T request, CancellationToken cancellationToken = default);
}
