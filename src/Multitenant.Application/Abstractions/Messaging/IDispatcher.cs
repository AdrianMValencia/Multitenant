using Multitenant.Application.Commons.Bases;

namespace Multitenant.Application.Abstractions.Messaging;

public interface IDispatcher
{
    Task<BaseResponse<TResponse>> Dispatch<TRequest, TResponse>(
        TRequest request, 
        CancellationToken cancellationToken) where TRequest : IRequest<TResponse>;
}
