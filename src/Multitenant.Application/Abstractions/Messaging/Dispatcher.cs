using Microsoft.Extensions.DependencyInjection;
using Multitenant.Application.Commons.Bases;

namespace Multitenant.Application.Abstractions.Messaging;

public class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<BaseResponse<TResponse>> Dispatch<TRequest, TResponse>(
        TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        try
        {
            if (request is ICommand<TResponse>)
            {
                var handleType = typeof(ICommandHandler<,>)
                    .MakeGenericType(request.GetType(), typeof(TResponse));

                dynamic handler = _serviceProvider.GetRequiredService(handleType);

                var executor = _serviceProvider.GetRequiredService<HandlerExecutor>();

                return await executor.ExecuteAsync<TRequest, TResponse>(
                    request,
                    () => handler.Handle((dynamic)request, cancellationToken),
                    cancellationToken);
            }

            if (request is IQuery<TResponse>)
            {
                var handleType = typeof(IQueryHandler<,>)
                    .MakeGenericType(request.GetType(), typeof(TResponse));

                dynamic handler = _serviceProvider.GetRequiredService(handleType);

                return await handler.Handle((dynamic)request, cancellationToken);
            }

            throw new
                InvalidOperationException(
                "El tipo de solicitud no es compatible con el Dispatcher actual.");
        }
        catch (Exception ex)
        {
            return new BaseResponse<TResponse>
            {
                IsSuccess = false,
                Message = "Ocurrió un error al despachar la solicitud",
                Errors =
                [
                    new() { PropertyName = "Dispatcher", ErrorMessage = ex.Message }
                ]
            };
        }
    }
}
