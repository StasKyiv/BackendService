using BackendService.Repository.Implementation;
using BackendService.Repository.Interfaces;
using BackendService.ServiceBus.Implementation;
using BackendService.ServiceBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BackendService.Configuration;

public static class InitializeServicesExtension
{
    public static void InitializeServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddSingleton<IServiceBusHandler, ServiceBusHandler>();
    }
}