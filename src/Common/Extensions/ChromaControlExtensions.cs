// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.Common.Services;
using ChromaControl.Keys;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;
using System.Security.Principal;

namespace ChromaControl.Common.Extensions;

/// <summary>
/// Chroma Control extension methods.
/// </summary>
public static class ChromaControlExtensions
{
    private const string DefaultPipeName = "ChromaControl";
    private const int DefaultTcpPort = 50071;

    /// <summary>
    /// Adds Chroma Control services to an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> to continue adding services to.</returns>
    public static IServiceCollection AddChromaControlServices(this IServiceCollection services)
    {
        var applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "ChromaControl";

        return AddChromaControlServices(services, applicationName);
    }

    /// <summary>
    /// Adds Chroma Control services to an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="applicationName">The single-instance application name.</param>
    /// <returns>The <see cref="IServiceCollection"/> to continue adding services to.</returns>
    public static IServiceCollection AddChromaControlServices(this IServiceCollection services, string applicationName)
    {
        services.AddSingleton(AcquireSingleInstanceMutex(applicationName));

        services.AddHostedService<StartupService>();

        return services;
    }

    internal static Mutex AcquireSingleInstanceMutex(string applicationName)
    {
        var normalizedApplicationName = string.IsNullOrWhiteSpace(applicationName)
            ? "ChromaControl"
            : applicationName.Trim();

        var mutex = new Mutex(true, normalizedApplicationName, out var success);

        if (success)
        {
            return mutex;
        }

        mutex.Dispose();
        throw new InvalidOperationException($"Another instance of {normalizedApplicationName} is already running.");
    }

    /// <summary>
    /// Adds Chroma Control logging to an <see cref="ILoggingBuilder"/>.
    /// </summary>
    /// <param name="logging">The <see cref="ILoggingBuilder"/> to add logging to.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> to continue adding logging to.</returns>
    public static ILoggingBuilder AddChromaControlLogging(this ILoggingBuilder logging)
    {
        var paths = ResolvePaths(new ConfigurationManager(), AppContext.BaseDirectory, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

        return AddChromaControlLogging(logging, paths);
    }

    /// <summary>
    /// Adds Chroma Control logging to an <see cref="ILoggingBuilder"/>.
    /// </summary>
    /// <param name="logging">The <see cref="ILoggingBuilder"/> to add logging to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing Chroma Control path settings.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> to continue adding logging to.</returns>
    public static ILoggingBuilder AddChromaControlLogging(this ILoggingBuilder logging, IConfiguration configuration)
    {
        var paths = ResolvePaths(configuration);

        return AddChromaControlLogging(logging, paths);
    }

    private static ILoggingBuilder AddChromaControlLogging(ILoggingBuilder logging, ChromaControlPaths paths)
    {
        var applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "ChromaControl";
        var logFilePath = Path.Combine(paths.LogsPath, $"{applicationName}.log");

        logging.AddFile(logFilePath, config =>
        {
            config.Append = true;
            config.FileSizeLimitBytes = 1024 * 1024 * 64;
            config.MaxRollingFiles = 2;
        });

        return logging;
    }

    /// <summary>
    /// Adds Chroma Control configuration to an <see cref="IConfigurationManager"/>.
    /// </summary>
    /// <param name="config">The <see cref="IConfigurationManager"/> to add configuration to.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> to continue adding configuration to.</returns>
    public static IConfigurationManager AddChromaControlConfiguration(this IConfigurationManager config)
    {
        var resolvedPaths = ResolvePaths(config);

        EnsureBootstrapDirectoriesExist(resolvedPaths);

        var chromaControl = config.GetSection("ChromaControl");
        var pathSection = chromaControl.GetSection("Path");
        var connectionStrings = config.GetSection("ConnectionStrings");

        var appExecutable = $"{AppDomain.CurrentDomain.FriendlyName}.exe";
        var appPath = Path.Combine(resolvedPaths.AppPath, appExecutable);
        chromaControl["VERSION"] = ResolveProductVersion(appPath);

        pathSection["APP"] = resolvedPaths.AppPath;
        pathSection["DATA"] = resolvedPaths.DataPath;
        pathSection["LOGS"] = resolvedPaths.LogsPath;
        pathSection["CONFIG"] = resolvedPaths.ConfigPath;

        connectionStrings["Database"] = resolvedPaths.DatabaseConnectionString;

        config.AddChromaControlKeys();

        return config;
    }

    internal static void EnsureBootstrapDirectoriesExist(ChromaControlPaths paths)
    {
        EnsureBootstrapDirectoriesExist(paths, Directory.CreateDirectory);
    }

    internal static void EnsureBootstrapDirectoriesExist(ChromaControlPaths paths, Func<string, DirectoryInfo> createDirectory)
    {
        ArgumentNullException.ThrowIfNull(createDirectory);

        EnsureDirectoryExists(paths.DataPath, "data", createDirectory);
        EnsureDirectoryExists(paths.LogsPath, "logs", createDirectory);
        EnsureDirectoryExists(paths.ConfigPath, "config", createDirectory);
    }

    internal static ChromaControlPaths ResolvePaths(IConfiguration configuration)
    {
        return ResolvePaths(configuration, AppContext.BaseDirectory, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
    }

    internal static ChromaControlPaths ResolvePaths(IConfiguration configuration, string appBasePath, string localAppDataPath)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var pathSection = configuration.GetSection("ChromaControl").GetSection("Path");
        var appPath = GetConfiguredValue(pathSection["APP"]) ?? appBasePath;
        var dataPath = GetConfiguredValue(pathSection["DATA"]) ?? Path.Combine(localAppDataPath, "ChromaControl");
        var logsPath = GetConfiguredValue(pathSection["LOGS"]) ?? Path.Combine(dataPath, "logs");
        var configPath = GetConfiguredValue(pathSection["CONFIG"]) ?? Path.Combine(dataPath, "config");
        var databaseConnectionString = GetConfiguredValue(configuration.GetConnectionString("Database")) ?? $"Data Source={Path.Combine(dataPath, "ChromaControl.db")}";

        return new ChromaControlPaths(appPath, dataPath, logsPath, configPath, databaseConnectionString);
    }

    internal static string ResolveProductVersion(string appPath)
    {
        return ResolveProductVersion(appPath, Assembly.GetEntryAssembly());
    }

    internal static string ResolveProductVersion(string appPath, Assembly? entryAssembly)
    {
        if (TryGetProductVersion(appPath, out var productVersion))
        {
            return productVersion;
        }

        if (entryAssembly != null)
        {
            if (TryGetProductVersion(entryAssembly.Location, out productVersion))
            {
                return productVersion;
            }

            var informationalVersion = entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            if (!string.IsNullOrWhiteSpace(informationalVersion))
            {
                return informationalVersion;
            }

            var assemblyVersion = entryAssembly.GetName().Version?.ToString();

            if (!string.IsNullOrWhiteSpace(assemblyVersion))
            {
                return assemblyVersion;
            }
        }

        return "Unknown";
    }

    private static bool TryGetProductVersion(string appPath, out string productVersion)
    {
        productVersion = string.Empty;

        if (string.IsNullOrWhiteSpace(appPath) || !File.Exists(appPath))
        {
            return false;
        }

        try
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(appPath);

            if (string.IsNullOrWhiteSpace(versionInfo.ProductVersion))
            {
                return false;
            }

            productVersion = versionInfo.ProductVersion;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void EnsureDirectoryExists(string path, string purpose, Func<string, DirectoryInfo> createDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);

        if (Directory.Exists(path))
        {
            return;
        }

        try
        {
            createDirectory(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw new InvalidOperationException($"Unable to initialize the Chroma Control {purpose} directory at '{path}'.", ex);
        }
    }

    private static string? GetConfiguredValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Gets a Chroma Control path from the configuration.
    /// </summary>
    /// <param name="config">The <see cref="IConfigurationManager"/> to get the path from.</param>
    /// <param name="pathName">The name of the path requested.</param>
    /// <returns>The requested path.</returns>
    public static string GetChromaControlPath(this IConfiguration config, string pathName)
    {
        var result = config.GetSection("ChromaControl").GetSection("Path")[pathName.ToUpper()]
            ?? throw new InvalidOperationException($"The requested path '{pathName}` could not be found.");

        return result;
    }

    /// <summary>
    /// Adds a Chroma Control Grpc client.
    /// </summary>
    /// <typeparam name="TClient">The client type to add.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddChromaControlGrpcClient<TClient>(this IServiceCollection services) where TClient : ClientBase
    {
        return AddChromaControlGrpcClient<TClient>(services, new ConfigurationManager());
    }

    /// <summary>
    /// Adds a Chroma Control Grpc client.
    /// </summary>
    /// <typeparam name="TClient">The client type to add.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing transport settings.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddChromaControlGrpcClient<TClient>(this IServiceCollection services, IConfiguration configuration) where TClient : ClientBase
    {
        var transportOptions = ResolveTransportOptions(configuration);
        services.AddGrpcClient<TClient>(options =>
        {
            options.Address = ResolveGrpcClientAddress(transportOptions);
            options.ChannelOptionsActions.Add(channel =>
            {
                if (!transportOptions.UseNamedPipes)
                {
                    return;
                }

                channel.HttpHandler = new SocketsHttpHandler
                {
                    ConnectCallback = (_, cancellationToken) => new ValueTask<Stream>(ConnectNamedPipeAsync(transportOptions.PipeName, cancellationToken))
                };
            });
        });

        return services;
    }

    internal static ChromaControlTransportOptions ResolveTransportOptions(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var useNamedPipes = configuration.GetValue("Service:Transport:UseNamedPipes", true);
        var allowAllAuthenticatedUsers = configuration.GetValue("Service:Transport:AllowAllAuthenticatedUsers", true);
        var configuredPipeName = GetConfiguredValue(configuration["Service:Transport:PipeName"]);
        var pipeName = configuredPipeName ?? DefaultPipeName;
        var port = configuration.GetValue<int?>("Service:Transport:Port") ?? DefaultTcpPort;

        return new ChromaControlTransportOptions(useNamedPipes, pipeName, port, allowAllAuthenticatedUsers);
    }

    internal static Uri ResolveGrpcClientAddress(ChromaControlTransportOptions transportOptions)
    {
        if (transportOptions.UseNamedPipes)
        {
            return new Uri("http://localhost");
        }

        if (transportOptions.Port <= 0)
        {
            throw new InvalidOperationException("Chroma Control TCP client transport requires a positive Service:Transport:Port value.");
        }

        return new Uri($"http://127.0.0.1:{transportOptions.Port}");
    }

    internal static string GetTransportModeLabel(ChromaControlTransportOptions transportOptions)
    {
        return transportOptions.UseNamedPipes ? "named-pipe" : "tcp";
    }

    internal static string GetTransportEndpointLabel(ChromaControlTransportOptions transportOptions, bool forServer)
    {
        if (transportOptions.UseNamedPipes)
        {
            return $"pipe:{transportOptions.PipeName}";
        }

        if (transportOptions.Port <= 0)
        {
            return forServer ? "tcp:127.0.0.1:auto" : "tcp:invalid-port";
        }

        return $"tcp:127.0.0.1:{transportOptions.Port}";
    }

    internal static IReadOnlyList<string> GetTransportConfigurationWarnings(IConfiguration configuration, ChromaControlTransportOptions transportOptions, bool forServer)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var warnings = new List<string>();
        var hasExplicitPort = GetConfiguredValue(configuration["Service:Transport:Port"]) != null;
        var hasExplicitPipeName = GetConfiguredValue(configuration["Service:Transport:PipeName"]) != null;
        var hasExplicitPipeAclMode = GetConfiguredValue(configuration["Service:Transport:AllowAllAuthenticatedUsers"]) != null;

        if (transportOptions.UseNamedPipes)
        {
            if (hasExplicitPort)
            {
                warnings.Add("Service:Transport:Port is ignored when Service:Transport:UseNamedPipes=true.");
            }

            return warnings;
        }

        if (hasExplicitPipeName)
        {
            warnings.Add("Service:Transport:PipeName is ignored when Service:Transport:UseNamedPipes=false.");
        }

        if (hasExplicitPipeAclMode)
        {
            warnings.Add("Service:Transport:AllowAllAuthenticatedUsers is ignored when Service:Transport:UseNamedPipes=false.");
        }

        if (transportOptions.Port <= 0)
        {
            warnings.Add(forServer
                ? "Service:Transport:Port<=0 enables dynamic server TCP port selection, which cannot be auto-discovered by TCP clients."
                : "Service:Transport:Port must be a positive value for TCP client transport.");
        }

        return warnings;
    }

    internal static Task<Stream> ConnectNamedPipeAsync(string pipeName, CancellationToken cancellationToken)
    {
        return ConnectNamedPipeAsync(
            pipeName,
            pipeNameValue => new NamedPipeClientStream(
                serverName: ".",
                pipeName: pipeNameValue,
                direction: PipeDirection.InOut,
                options: PipeOptions.WriteThrough | PipeOptions.Asynchronous,
                impersonationLevel: TokenImpersonationLevel.Anonymous),
            static (clientStream, token) => clientStream.ConnectAsync(token),
            cancellationToken);
    }

    internal static async Task<Stream> ConnectNamedPipeAsync(
        string pipeName,
        Func<string, NamedPipeClientStream> createClientStream,
        Func<NamedPipeClientStream, CancellationToken, Task> connectAsync,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pipeName);
        ArgumentNullException.ThrowIfNull(createClientStream);
        ArgumentNullException.ThrowIfNull(connectAsync);

        var normalizedPipeName = pipeName.Trim();
        var clientStream = createClientStream(normalizedPipeName);

        try
        {
            await connectAsync(clientStream, cancellationToken).ConfigureAwait(false);
            return clientStream;
        }
        catch (OperationCanceledException)
        {
            clientStream.Dispose();
            throw;
        }
        catch (Exception ex) when (ex is System.IO.IOException or TimeoutException or UnauthorizedAccessException)
        {
            clientStream.Dispose();
            throw new InvalidOperationException($"Unable to connect to the Chroma Control service named pipe '{normalizedPipeName}'.", ex);
        }
        catch
        {
            clientStream.Dispose();
            throw;
        }
    }
}

internal readonly record struct ChromaControlPaths(
    string AppPath,
    string DataPath,
    string LogsPath,
    string ConfigPath,
    string DatabaseConnectionString);

internal readonly record struct ChromaControlTransportOptions(
    bool UseNamedPipes,
    string PipeName,
    int Port,
    bool AllowAllAuthenticatedUsers);
