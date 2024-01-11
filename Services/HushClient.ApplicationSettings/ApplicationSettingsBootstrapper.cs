using System.Reactive.Subjects;
using Olimpo;

namespace HushClient.ApplicationSettings;

public class ApplicationSettingsBootstrapper : IBootstrapper
{
    private readonly IApplicationSettingsManager _applicationSettingsManager;

    public Subject<bool> BootstrapFinished { get; }

    public int Priority { get; set; } = 5;

    public ApplicationSettingsBootstrapper(IApplicationSettingsManager applicationSettingsManager)
    {
        this._applicationSettingsManager = applicationSettingsManager;

        this.BootstrapFinished = new Subject<bool>();
    }

    public void Shutdown()
    {
    }

    public async Task Startup()
    {
        await this._applicationSettingsManager.LoadSettingsAsync();

        this.BootstrapFinished.OnNext(true);
    }
}
