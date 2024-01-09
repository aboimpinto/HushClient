using Microsoft.Extensions.DependencyInjection;
using HushClient.TcpClient;
using Olimpo;

namespace HushClient;

public static class TcpClientHostBuilder
{
    public static IServiceCollection RegisterTcpClientService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IBootstrapper, TcpClientBootstrapper>();
        serviceCollection.AddSingleton<ITcpClientService, TcpClientService>();

        return serviceCollection;
    }
}
