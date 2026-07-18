// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.SDK.OpenRGB;
using ChromaControl.SDK.Synapse;
using ChromaControl.SDK.Synapse.Enums;
using ChromaControl.Service.Data;
using ChromaControl.Service.Data.Extensions;
using ChromaControl.Service.Lighting.Services;
using System.Drawing;
using System.Threading.Channels;

namespace ChromaControl.Service.Lighting.Workers;

/// <summary>
/// The lighting worker.
/// </summary>
public partial class LightingWorker : IHostedService
{
    private const int ChannelCapacity = 8;

    private readonly ILogger<LightingWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly EventDispatcher _eventDispatcher;
    private readonly IOpenRGBService _openRGBService;
    private readonly ISynapseService _synapseService;
    private readonly object _devicesLock = new();
    private readonly Channel<Color[]> _colorUpdates = Channel.CreateBounded<Color[]>(new BoundedChannelOptions(ChannelCapacity)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.DropOldest
    });

    private CancellationTokenSource? _processingTokenSource;
    private Task? _processingTask;
    private bool _isRunning;
    private IReadOnlyList<SDK.OpenRGB.Structs.OpenRGBDevice> _devices = [];

    [LoggerMessage(0, LogLevel.Information, "Connected to Synapse...", EventName = "SynapseConnected")]
    private static partial void LogSynapseConnected(ILogger logger);

    [LoggerMessage(1, LogLevel.Information, "Disconnected from Synapse...", EventName = "SynapseDisconnected")]
    private static partial void LogSynapseDisconnected(ILogger logger);

    [LoggerMessage(2, LogLevel.Error, "Applying received color update failed.", EventName = "LightingColorUpdateFailed")]
    private static partial void LogLightingColorUpdateFailed(ILogger logger, Exception ex);

    [LoggerMessage(3, LogLevel.Warning, "Skipped color update because the worker queue is closed.", EventName = "LightingColorUpdateSkipped")]
    private static partial void LogLightingColorUpdateSkipped(ILogger logger);

    /// <summary>
    /// Creates a <see cref="LightingWorker"/> instance.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    /// <param name="eventDispatcher">The <see cref="EventDispatcher"/>.</param>
    /// <param name="openRGBService">The <see cref="IOpenRGBService"/>.</param>
    /// <param name="synapseService">The <see cref="ISynapseService"/>.</param>
    public LightingWorker(ILogger<LightingWorker> logger, IServiceProvider serviceProvider, EventDispatcher eventDispatcher, IOpenRGBService openRGBService, ISynapseService synapseService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _eventDispatcher = eventDispatcher;
        _openRGBService = openRGBService;
        _synapseService = synapseService;

        _openRGBService.DeviceListUpdated += DeviceListUpdated;
        _synapseService.StatusChanged += OnStatusChanged;
        _synapseService.ColorsReceived += OnColorsReceived;
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _processingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _processingTask = Task.Run(() => ProcessColorUpdatesAsync(_processingTokenSource.Token), _processingTokenSource.Token);
        _isRunning = true;

        await WriteConfiguration();
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _isRunning = false;

        _openRGBService.DeviceListUpdated -= DeviceListUpdated;
        _synapseService.StatusChanged -= OnStatusChanged;
        _synapseService.ColorsReceived -= OnColorsReceived;

        _colorUpdates.Writer.TryComplete();

        _processingTokenSource?.Cancel();

        if (_processingTask != null)
        {
            try
            {
                await _processingTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        _processingTokenSource?.Dispose();
        _processingTokenSource = null;
    }

    private void DeviceListUpdated(object? sender, IReadOnlyList<SDK.OpenRGB.Structs.OpenRGBDevice> e)
    {
        lock (_devicesLock)
        {
            _devices = e;
        }

        _eventDispatcher.RaiseDevicesUpdated();
    }

    private async Task WriteConfiguration()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var config = await context.GenerateConfig();

        _openRGBService.UpdateConfiguration(config);
    }

    private void OnStatusChanged(object? sender, SynapseStatus e)
    {
        if (e == SynapseStatus.Live)
        {
            LogSynapseConnected(_logger);
        }
        else if (e == SynapseStatus.NotLive)
        {
            LogSynapseDisconnected(_logger);
        }
    }

    private void OnColorsReceived(object? sender, Color[] e)
    {
        if (!_isRunning)
        {
            return;
        }

        if (!_colorUpdates.Writer.TryWrite((Color[])e.Clone()))
        {
            LogLightingColorUpdateSkipped(_logger);
        }
    }

    private async Task ProcessColorUpdatesAsync(CancellationToken cancellationToken)
    {
        await foreach (var colors in _colorUpdates.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                var latestColors = colors;

                while (_colorUpdates.Reader.TryRead(out var newerColors))
                {
                    latestColors = newerColors;
                }

                await ApplyColorsAsync(latestColors, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                LogLightingColorUpdateFailed(_logger, ex);
            }
        }
    }

    private async Task ApplyColorsAsync(Color[] colors, CancellationToken cancellationToken)
    {
        IReadOnlyList<SDK.OpenRGB.Structs.OpenRGBDevice> devices;

        lock (_devicesLock)
        {
            devices = _devices;
        }

        foreach (var device in devices)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var buffer = device.CreateColorBuffer();
            var currentColor = 0;

            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = currentColor < colors.Length
                    ? colors[currentColor]
                    : Color.Black;

                if (currentColor == 4)
                {
                    currentColor = 0;
                }
                else
                {
                    currentColor++;
                }
            }

            await _openRGBService.UpdateLedsAsync(device, buffer, cancellationToken);
        }
    }
}
