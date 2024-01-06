using Microsoft.Extensions.DependencyInjection;
using HushClient.ViewModels;
using Olimpo;
using Olimpo.NavigationManager;
using HushClient.Workflows;


namespace HushClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterHushClientServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ViewModelBase, MainViewModel>("MainViewModel");

        serviceCollection.AddSingleton<IHushClientWorkflow, HushClientWorkflow>();

        return serviceCollection;
    }
}
