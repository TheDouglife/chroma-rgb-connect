// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Grpc.Core;
using MediatR;
using MediatR.Pipeline;

namespace ChromaControl.App.Core.Mediator;

/// <summary>
/// The global mediator exception handler.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <typeparam name="TException">The exception type.</typeparam>
public partial class GlobalExceptionHandler<TRequest, TResponse, TException> : IRequestExceptionHandler<TRequest, TResponse, TException>
    where TRequest : IBaseRequest
    where TResponse : struct
    where TException : Exception
{
    private static readonly Func<string, TResponse> s_failureResponseFactory = CreateFailureResponseFactory();

    private readonly ILogger _logger;

    [LoggerMessage(0, LogLevel.Warning, "Mediator exception handled for {RequestType}: {Message}", EventName = "ExceptionHandled")]
    private static partial void LogExceptionHandledMessage(ILogger logger, Exception exception, string requestType, string message);

    /// <summary>
    /// Creates a <see cref="GlobalExceptionHandler{TRequest, TResponse, TException}"/> instance.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler<TRequest, TResponse, TException>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles an incoming exception.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="state">The <see cref="RequestExceptionHandlerState{TResponse}"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken)
    {
        state.SetHandled(CreateResponse(exception, GetUserMessage(exception)));

        return Task.CompletedTask;
    }

    private TResponse CreateResponse(Exception exception, string message)
    {
        var response = s_failureResponseFactory(message);

        LogExceptionHandledMessage(_logger, exception, typeof(TRequest).Name, message);

        return response;
    }

    private static string GetUserMessage(Exception exception)
    {
        return exception switch
        {
            OperationCanceledException => "The request was canceled.",
            RpcException { StatusCode: StatusCode.Cancelled } => "The request was canceled.",
            RpcException { StatusCode: StatusCode.DeadlineExceeded } => "The lighting service did not respond in time.",
            RpcException { StatusCode: StatusCode.Unimplemented } => "The lighting service does not support this feature.",
            RpcException => "Unable to communicate with the lighting service.",
            _ => "An unexpected error occurred."
        };
    }

    private static Func<string, TResponse> CreateFailureResponseFactory()
    {
        var responseType = typeof(TResponse);

        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<,>))
        {
            return message => throw CreateUnsupportedResponseException(responseType, message);
        }

        var responseTypeArguments = responseType.GetGenericArguments();

        if (responseTypeArguments[1] != typeof(string))
        {
            return message => throw CreateUnsupportedResponseException(responseType, message);
        }

        var failureType = Failure(string.Empty).GetType();
        var failureConstructor = responseType.GetConstructor([failureType]);

        if (failureConstructor == null)
        {
            return message => throw CreateUnsupportedResponseException(responseType, message);
        }

        return message =>
        {
            var response = failureConstructor.Invoke([Failure(message)]);

            if (response is TResponse typedResponse)
            {
                return typedResponse;
            }

            throw CreateUnsupportedResponseException(responseType, message);
        };
    }

    private static InvalidOperationException CreateUnsupportedResponseException(Type responseType, string message)
    {
        return new InvalidOperationException($"{nameof(GlobalExceptionHandler<TRequest, TResponse, TException>)} requires a response type shaped like Result<TValue, string>. Received '{responseType}'. Original error message: '{message}'.");
    }
}
