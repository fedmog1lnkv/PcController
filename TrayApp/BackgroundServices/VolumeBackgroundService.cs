using Application.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using TrayApp.Hubs;

namespace TrayApp.BackgroundServices;

public class VolumeBackgroundService(VolumeService service, IHubContext<MainHub> hubContext) : BackgroundService
{
    private readonly VolumeService _service = service;
    private readonly IHubContext<MainHub> _hubContext = hubContext;
    private int _currentVolume;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _currentVolume = _service.GetCurrentVolume();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformBackgroundTaskAsync();

                await Task.Delay(500, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in background task: {ex.Message}");
            }
        }
    }

    private async Task PerformBackgroundTaskAsync()
    {
        var currentVolume = _service.GetCurrentVolume();
        if (currentVolume != _currentVolume)
        {
            _currentVolume = currentVolume;
            await _hubContext.Clients.All.SendAsync("ReceiveVolume", currentVolume);
        }
        await Task.CompletedTask;
    }
}