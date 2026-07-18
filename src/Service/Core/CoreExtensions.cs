// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.Common.Extensions;
using ChromaConnect.Service.Core.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ChromaConnect.Service.Core;

/// <summary>
/// Core extension methods.
/// </summary>
public static class CoreExtensions
{
    /// <summary>
    /// Adds core configuration to a <see cref="IHostApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to add configuration to.</param>
    /// <returns>The <see cref="IHostApplicationBuilder"/> to continue adding configuration to.</returns>
    public static IHostApplicationBuilder ConfigureCore(this IHostApplicationBuilder builder)
    {
        builder.ConfigureChromaConnect()
            .ConfigureTelemetry()
            .ConfigureGrpc()
            .ConfigureHealth()
            .ConfigureTransport();

        return builder;
    }

    private static IHostApplicationBuilder ConfigureChromaConnect(this IHostApplicationBuilder builder)
    {
        builder.Configuration.AddChromaConnectConfiguration();
        var environmentSingleInstanceName = Environment.GetEnvironmentVariable("CHROMACONNECT_SINGLE_INSTANCE_NAME");
        var configuredSingleInstanceName = builder.Configuration["ChromaConnect:SingleInstanceName"];
        var singleInstanceName = environmentSingleInstanceName ?? configuredSingleInstanceName ?? "ChromaConnect.Service";

        builder.Services.AddChromaConnectServices(singleInstanceName);
        builder.Logging.AddChromaConnectLogging(builder.Configuration);

        return builder;
    }

    private static IHostApplicationBuilder ConfigureTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                });
            });

        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            builder.Services.AddOpenTelemetry()
                .UseOtlpExporter();
        }

        return builder;
    }

    private static IHostApplicationBuilder ConfigureGrpc(this IHostApplicationBuilder builder)
    {
        builder.Services.AddGrpc();

        return builder;
    }

    private static IHostApplicationBuilder ConfigureHealth(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ServiceStartupState>();
        builder.Services.AddHealthChecks()
            .AddCheck<StartupHealthCheck>("startup");

        return builder;
    }

    private static IHostApplicationBuilder ConfigureTransport(this IHostApplicationBuilder builder)
    {
        if (builder is not WebApplicationBuilder webAppBuilder)
        {
            throw new ArgumentException("Named pipes can only be configured on a WebApplicationBuilder.");
        }

        var transportOptions = ChromaConnectExtensions.ResolveTransportOptions(builder.Configuration);

        if (transportOptions.UseNamedPipes)
        {
            ConfigureNamedPipe(webAppBuilder, transportOptions.PipeName, transportOptions.AllowAllAuthenticatedUsers);
        }
        else
        {
            ConfigureTcp(webAppBuilder, transportOptions.Port);
        }

        return builder;
    }

    private static void ConfigureNamedPipe(WebApplicationBuilder webAppBuilder, string pipeName, bool allowAllAuthenticatedUsers)
    {
        webAppBuilder.WebHost.UseNamedPipes(options =>
        {
            options.CurrentUserOnly = !allowAllAuthenticatedUsers;

            if (allowAllAuthenticatedUsers)
            {
                options.PipeSecurity = new();
                options.PipeSecurity.AddAccessRule(new(
                    identity: new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                    rights: PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
                    type: AccessControlType.Allow));
            }
        });

        webAppBuilder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenNamedPipe(pipeName, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });
    }

    private static void ConfigureTcp(WebApplicationBuilder webAppBuilder, int port)
    {
        webAppBuilder.WebHost.ConfigureKestrel(options =>
        {
            if (port <= 0)
            {
                options.ListenLocalhost(0, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });

                return;
            }

            options.ListenLocalhost(port, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });
    }
}
