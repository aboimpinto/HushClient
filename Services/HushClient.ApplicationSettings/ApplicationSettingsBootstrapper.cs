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

    public Task Startup()
    {
        this.BootstrapFinished.OnNext(true);
        return Task.CompletedTask;
    }
}
