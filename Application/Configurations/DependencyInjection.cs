using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<VolumeService>();
        services.AddTransient<MediaService>();
        services.AddSingleton<ApplicationService>();

        return services;
    }
}