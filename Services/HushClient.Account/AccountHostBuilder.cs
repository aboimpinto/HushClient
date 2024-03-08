using Microsoft.Extensions.DependencyInjection;
using Olimpo;
using HushClient.Account;

namespace HushClient;

public static class AccountHostBuilder
{
    public static IServiceCollection RegisterAccountService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IBootstrapper, AccountBootstrapper>();
        serviceCollection.AddSingleton<IAccountService, AccountService>();

        return serviceCollection;
    }
}
