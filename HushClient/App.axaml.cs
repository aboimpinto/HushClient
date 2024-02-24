using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HushClient.Views;
using Microsoft.Extensions.DependencyInjection;
using Olimpo;
using Olimpo.NavigationManager;
using HushEcosystem;
using HushClient.ApplicationSettings.Model;
using HushClient.Model;
using HushClient.ViewModels;

namespace HushClient;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var viewModel = ServiceCollectionManager.ServiceProvider.GetService<ViewModelBase>("MainViewModel");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = viewModel
            };
        }

        if (viewModel is ILoadableViewModel loadableViewModel)
        {
            loadableViewModel.LoadAsync().Wait();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public override void RegisterServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddLogging()
            .RegisterEventAggregator()
            .RegisterBootstrapper()
            .RegisterNavigationManager()
            .RegisterTcpClientManager()
            .RegisterTcpClientService()
            .RegisterHushClientServices()
            .RegisterRpcModel()
            .RegisterApplicationSettings();

        serviceCollection.AddSingleton<BlockchainInformation>();
        serviceCollection.AddSingleton<LocalInformation>();

        serviceCollection.AddScoped<ViewModelBase, BalanceViewModel>("BalanceViewModel");
        serviceCollection.AddScoped<ViewModelBase, FeedViewModel>("FeedViewModel");

        ServiceCollectionManager.SetServiceProvider(serviceCollection);

        base.RegisterServices();
    }
}