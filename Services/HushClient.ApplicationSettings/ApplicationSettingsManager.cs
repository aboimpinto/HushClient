using System.Reactive.Subjects;
using System.Threading.Tasks;
using HushClient.ApplicationSettings.Model;
using Microsoft.Extensions.Logging;
using Olimpo;

namespace HushClient.ApplicationSettings;

public class ApplicationSettingsManager : 
    IApplicationSettingsManager, 
    IBootstrapper
{
    private readonly IEventAggregator eventAggregator;

    private ILogger<ApplicationSettingsManager> _logger;

    public BlockchainInfo BlockchainInfo { get; private set; }

    public Subject<bool> BootstrapFinished { get; }

    public int Priority { get; set; } = 0;        

    public ApplicationSettingsManager(
        IEventAggregator _eventAggregator,
        ILogger<ApplicationSettingsManager> logger)
    {
        this.eventAggregator = _eventAggregator;
        this._logger = logger;

        this.BootstrapFinished = new Subject<bool>();
    }

    public Task LoadSettingsAsync()
    {
        // TODO [AboimPinto] this information should be get from the local database
        this.BlockchainInfo = new BlockchainInfo
        {
            LastHeightSynched = 0
        };

        return Task.CompletedTask;
    }

    public Task SetLastHeightAsync(int lastHeight)
    {
        this.BlockchainInfo.LastHeightSynched = lastHeight;
        return Task.CompletedTask;
    }

    public async Task Startup()
    {
        await this.LoadSettingsAsync();

        this.BootstrapFinished.OnNext(true);
    }

    public void Shutdown()
    {
    }
}