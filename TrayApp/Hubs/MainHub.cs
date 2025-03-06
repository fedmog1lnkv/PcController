using Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace TrayApp.Hubs;

public class MainHub(VolumeService volumeService, IHubContext<MainHub> hubContext)
    : Hub
{
    public async Task SendVolume()
    {
            var currentVolume = volumeService.GetCurrentVolume();
            await hubContext.Clients.All.SendAsync("ReceiveVolume", currentVolume);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}