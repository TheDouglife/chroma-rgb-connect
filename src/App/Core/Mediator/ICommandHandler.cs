// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MediatR;

namespace ChromaConnect.App.Core.Mediator;

/// <summary>
/// Defines a handler for a command with a void response.
/// </summary>
/// <typeparam name="TCommand">The type of the command being handled.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result<int, string>> where TCommand : ICommand;

/// <summary>
/// Defines a handler for a command with a response.
/// </summary>
/// <typeparam name="TCommand">The type of the command being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response from the handler.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse, string>> where TCommand : ICommand<TResponse>;
