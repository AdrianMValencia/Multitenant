using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Multitenant.Application.Commons.Bases;
using ValidationException = Multitenant.Application.Commons.Exceptions.ValidationException;

namespace Multitenant.Application.Commons.Behaviours;

public class ValidationService(IServiceProvider serviceProvider) : IValidationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task ValidateAsync<T>(T request, CancellationToken cancellationToken = default)
    {
        var validators = _serviceProvider.GetServices<IValidator<T>>();

        if (!validators.Any()) return;

        var context = new ValidationContext<T>(request);

        var validationResults =
            await Task.WhenAll(validators.Select(x => x.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(x => !x.IsValid)
            .SelectMany(x => x.Errors)
            .Select(err => new BaseError
            {
                PropertyName = err.PropertyName,
                ErrorMessage = err.ErrorMessage
            })
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);
    }
}
