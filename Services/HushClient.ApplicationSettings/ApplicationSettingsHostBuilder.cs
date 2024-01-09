using HushClient.ApplicationSettings;
using Microsoft.Extensions.DependencyInjection;
using Olimpo;

namespace HushClient;

public static class ApplicationSettingsHostBuilder
{
    public static IServiceCollection RegisterApplicationSettings(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IBootstrapper, ApplicationSettingsBootstrapper>();
        serviceCollection.AddSingleton<IApplicationSettingsManager, ApplicationSettingsManager>();

        return serviceCollection;
    }
}
