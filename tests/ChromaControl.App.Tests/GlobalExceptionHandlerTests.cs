// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommonTypeUnions.Unions;
using Grpc.Core;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging.Abstractions;
using static CommonTypeUnions.Extensions.ResultExtensions;

using ChromaControl.App.Core.Mediator;

namespace ChromaControl.App.Tests;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task HandleReturnsFeatureUnsupportedFailureForUnimplementedRpc()
    {
        var handler = CreateHandler<TestQuery, Result<int, string>, RpcException>();
        var state = new RequestExceptionHandlerState<Result<int, string>>();

        await handler.Handle(new TestQuery(), new RpcException(new Status(StatusCode.Unimplemented, "missing")), state, CancellationToken.None);

        Assert.True(state.Handled);
        Assert.True(state.Response.IsFailure(out var error));
        Assert.Equal("The lighting service does not support this feature.", error);
    }

    [Fact]
    public async Task HandleReturnsCanceledFailureForCancelledRpc()
    {
        var handler = CreateHandler<TestQuery, Result<int, string>, RpcException>();
        var state = new RequestExceptionHandlerState<Result<int, string>>();

        await handler.Handle(new TestQuery(), new RpcException(new Status(StatusCode.Cancelled, "cancelled")), state, CancellationToken.None);

        Assert.True(state.Handled);
        Assert.True(state.Response.IsFailure(out var error));
        Assert.Equal("The request was canceled.", error);
    }

    [Fact]
    public async Task HandleReturnsUnexpectedFailureForGenericException()
    {
        var handler = CreateHandler<TestQuery, Result<int, string>, Exception>();
        var state = new RequestExceptionHandlerState<Result<int, string>>();

        await handler.Handle(new TestQuery(), new InvalidOperationException("boom"), state, CancellationToken.None);

        Assert.True(state.Handled);
        Assert.True(state.Response.IsFailure(out var error));
        Assert.Equal("An unexpected error occurred.", error);
    }

    [Fact]
    public async Task HandleThrowsClearErrorForUnsupportedResponseContract()
    {
        var handler = CreateHandler<InvalidResponseRequest, int, InvalidOperationException>();
        var state = new RequestExceptionHandlerState<int>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new InvalidResponseRequest(), new InvalidOperationException("boom"), state, CancellationToken.None));

        Assert.Contains("requires a response type shaped like Result<TValue, string>", exception.Message, StringComparison.Ordinal);
    }

    private static GlobalExceptionHandler<TRequest, TResponse, TException> CreateHandler<TRequest, TResponse, TException>()
        where TRequest : IBaseRequest
        where TResponse : struct
        where TException : Exception
    {
        return new GlobalExceptionHandler<TRequest, TResponse, TException>(NullLogger<GlobalExceptionHandler<TRequest, TResponse, TException>>.Instance);
    }

    private readonly record struct TestQuery() : IQuery<int>;

    private readonly record struct InvalidResponseRequest() : IRequest<int>;
}