using Microsoft.Extensions.DependencyInjection;
using Olimpo;

namespace HushClient.Services.ClientService;

public static class TcpClientHostBuilder
{
    public static IServiceCollection RegisterTcpClientService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IBootstrapper, TcpClientBootstrapper>();
        serviceCollection.AddSingleton<ITcpClientService, TcpClientService>();

        return serviceCollection;
    }
}
