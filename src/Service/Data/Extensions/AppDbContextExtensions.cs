// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;

namespace ChromaConnect.Service.Data.Extensions;

/// <summary>
/// Extension methods for <see cref="AppDbContext"/>.
/// </summary>
public static class AppDbContextExtensions
{
    /// <summary>
    /// Generates a config from a <see cref="AppDbContext"/>.
    /// </summary>
    /// <param name="context">The <see cref="AppDbContext"/>.</param>
    /// <returns>A <see cref="JsonNode"/> config.</returns>
    public static async Task<JsonNode> GenerateConfig(this AppDbContext context)
    {
        var detectorConfig = new JsonObject();

        var config = new JsonObject
        {
            ["Detectors"] = new JsonObject
            {
                ["detectors"] = detectorConfig
            },

            ["QMKOpenRGBDevices"] = new JsonObject
            {
                ["devices"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["name"] = "Keychron V6 Max",
                        ["usb_pid"] = "0960",
                        ["usb_vid"] = "3434"
                    }
                }
            }
        };

        var detectors = await context.DeviceDetectors
            .Select(d => new
            {
                d.Name,
                d.Vendor.Enabled
            })
            .ToListAsync();

        foreach (var detector in detectors)
        {
            detectorConfig[detector.Name] = detector.Enabled;
        }

        return config;
    }
}
