using System.Reactive.Subjects;
using System.Threading.Tasks;
using Olimpo;

namespace HushClient.Account;

public class AccountBootstrapper : IBootstrapper
{
    private IAccountService _accountService;

    public Subject<bool> BootstrapFinished { get; }

    public int Priority { get; set; } = 10;

    public AccountBootstrapper(IAccountService accountService)
    {
        this._accountService = accountService;
        
        this.BootstrapFinished = new Subject<bool>();
    }

    public void Shutdown()
    {
    }

    public async Task Startup()
    {
        // await this._accountService.LoadAccountsAsync();

        this.BootstrapFinished.OnNext(true);
    }
}
