// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MediatR;

namespace ChromaConnect.App.Core.Mediator;

/// <summary>
/// Marker interface to represent a command with a void response.
/// </summary>
public interface ICommand : IRequest<Result<int, string>>;

/// <summary>
/// Marker interface to represent a command with a response.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse, string>>;
