﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Timingz;

namespace WebApiSample.Services
{
    public class SampleServerTimingCallback : IServerTimingCallback, IAsyncDisposable
    {
        public async Task OnServerTiming(ServerTimingEvent serverTimingEvent)
        {
            var displayUrl = serverTimingEvent.HttpContext.Request.GetDisplayUrl();
            var metrics = serverTimingEvent.Metrics;
            await Console.Out.WriteLineAsync($"Server-Timing for {displayUrl} has {metrics.Count} metrics:");

            foreach (var metric in metrics)
                await Console.Out.WriteLineAsync(
                    $"- Name: {metric.Name}, Description: {metric.Description}, Duration: {metric.Duration}");
        }

        public async ValueTask DisposeAsync() => await Console.Out.WriteLineAsync("Disposing callback");
    }
}
