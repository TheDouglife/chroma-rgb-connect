// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.App.Lighting.Commands;
using ChromaConnect.App.Lighting.Queries;
using ChromaConnect.App.Shell.Services;
using ChromaConnect.Common.Protos.Lighting;
using Google.Protobuf.Collections;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace ChromaConnect.App.Lighting.Components;

/// <summary>
/// The device zones tab.
/// </summary>
public partial class DeviceZonesTab
{
    /// <summary>
    /// The index of the device.
    /// </summary>
    [CascadingParameter(Name = "DeviceIndex")]
    public required int DeviceIndex { get; set; }

    /// <summary>
    /// The <see cref="IMediator"/>.
    /// </summary>
    [Inject]
    public required IMediator Mediator { get; set; }

    /// <summary>
    /// The <see cref="DialogService"/>.
    /// </summary>
    [Inject]
    public required DialogService DialogService { get; set; }

    private RepeatedField<DeviceZone> _zones = [];
    private readonly Dictionary<int, string> _zoneStates = [];

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        await UpdateDeviceZones();
    }

    private async Task UpdateDeviceZones()
    {
        var response = await Mediator.Send(new GetDeviceZones.Query(DeviceIndex));

        if (response.IsSuccess(out var zones))
        {
            _zones = zones;
            _zoneStates.Clear();
        }
        else if (response.IsFailure(out var error))
        {
            _zones = [];

            DialogService.ShowError(error);
        }
    }

    private async Task OnZoneChanged(DeviceZone zone)
    {
        _zoneStates[zone.Index] = "Applying";

        var result = await Mediator.Send(new ResizeDeviceZone.Command(DeviceIndex, zone.Index, zone.LedCount));

        if (result.IsSuccess(out _))
        {
            _zoneStates[zone.Index] = "Applied";
        }
        else if (result.IsFailure(out var error))
        {
            _zoneStates[zone.Index] = "Failed";
            DialogService.ShowError(error);
        }

        await InvokeAsync(StateHasChanged);
    }
}
