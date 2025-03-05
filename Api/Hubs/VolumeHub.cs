using Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class VolumeHub(VolumeService volumeService, IHubContext<VolumeHub> hubContext)
    : Hub
{
    public async Task StartSendingVolume()
    {
        while (true)
        {
            // TODO : not for all clients
            var currentVolume = volumeService.GetCurrentVolume();
            await hubContext.Clients.All.SendAsync("ReceiveVolume", currentVolume);
            await Task.Delay(10000);
        }
    }

    public override async Task OnConnectedAsync()
    {
        await StartSendingVolume();
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}