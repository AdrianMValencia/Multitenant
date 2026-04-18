using Microsoft.Extensions.Logging;
using Multitenant.Application.Commons.Bases;
using Multitenant.Application.Commons.Behaviours;
using Multitenant.Application.Commons.Exceptions;

namespace Multitenant.Application.Abstractions.Messaging;

public class HandlerExecutor(IValidationService validationService, ILogger<HandlerExecutor> logger)
{
    private readonly IValidationService _validationService = validationService;
    private readonly ILogger<HandlerExecutor> _logger = logger;

    public async Task<BaseResponse<T>> ExecuteAsync<TRequest, T>(
        TRequest request,
        Func<Task<BaseResponse<T>>> action,
        CancellationToken cancellationToken)
    {
        try
        {
            await _validationService.ValidateAsync(request, cancellationToken);

            return await action();
        }
        catch(ValidationException ex)
        {
            _logger.LogWarning(
                "Fallo de validación para la petición {@Request}. Detalles: {@Errors}", 
                request, 
                ex.Errors);

            return new BaseResponse<T>
            {
                IsSuccess = false,
                Message = "La información enviada no cumple con las reglas del sistema.",
                Errors = ex.Errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Excepción no controlada, procesando el handler para {@Request}", request);

            return new BaseResponse<T>
            {
                IsSuccess = false,
                Message = "Ocurrió un error inesperado en el servidor.",
                Errors =
                [
                    new() { PropertyName = "Server", ErrorMessage = ex.Message }
                ]
            };
        }
    }
}
