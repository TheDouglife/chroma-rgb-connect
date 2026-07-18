// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

var builder = DistributedApplication.CreateBuilder(args);

var service = builder.AddProject<Projects.ChromaConnect_Service>("service", "http");

builder.AddProject<Projects.ChromaConnect_App>("app")
    .WithReference(service);

builder.Build().Run();
