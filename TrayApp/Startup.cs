using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using TrayApp.BackgroundServices;

namespace TrayApp;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<Settings>();
        services.AddSingleton<ApplicationService>();
        services.AddScoped<MediaService>();
        services.AddScoped<VolumeService>();

        services.AddHostedService<VolumeBackgroundService>();

        services.AddControllers();
        services.AddSignalR();
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}