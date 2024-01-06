using Microsoft.Extensions.DependencyInjection;
using Olimpo.TcpClientManager;

namespace Olimpo;

public static class TcpClientManagerHostBuilder
{
    public static IServiceCollection RegisterTcpClientManager(this IServiceCollection serviceCollection)
    {        
        serviceCollection.AddSingleton<IClient, Client>();

        return serviceCollection;
    }
}
