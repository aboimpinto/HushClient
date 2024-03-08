using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HushClient.Views;
using Microsoft.Extensions.DependencyInjection;
using Olimpo;
using Olimpo.NavigationManager;
using HushEcosystem;
using HushClient.Model;
using HushClient.ViewModels;
using System.Linq;

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

            desktop.ShutdownRequested += this.OnShutdownRequested;
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

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        ServiceCollectionManager.ServiceProvider
            .GetServices<IBootstrapper>()
            .ToList()
            .ForEach(x => x.Shutdown());
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
            .RegisterApplicationSettings()
            .RegisterAccountService();

        ServiceCollectionManager.SetServiceProvider(serviceCollection);

        base.RegisterServices();
    }
}