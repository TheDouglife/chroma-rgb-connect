// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.SDK.Synapse.Extensions;
using ChromaConnect.SDK.Synapse.Sample;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSynapseSDK();

builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
