using Microsoft.Extensions.DependencyInjection;
using HushClient.ViewModels;
using Olimpo;
using Olimpo.NavigationManager;
using HushClient.Workflows;
using HushClient.Model;


namespace HushClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterHushClientServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<BlockchainInformation>();
        serviceCollection.AddSingleton<LocalInformation>();


        serviceCollection.AddSingleton<IHushClientWorkflow, HushClientWorkflow>();
        serviceCollection.AddSingleton<IProfileWorkflow, ProfileWorkflow>();

        serviceCollection.AddScoped<ViewModelBase, MainViewModel>("MainViewModel");
        serviceCollection.AddScoped<ViewModelBase, BalanceViewModel>("BalanceViewModel");
        serviceCollection.AddScoped<ViewModelBase, FeedViewModel>("FeedViewModel");
        serviceCollection.AddScoped<ViewModelBase, NewAccountViewModel>("NewAccountViewModel");
        serviceCollection.AddScoped<ViewModelBase, SearchAccountsViewModel>("SearchAccountsViewModel");

        return serviceCollection;
    }
}
