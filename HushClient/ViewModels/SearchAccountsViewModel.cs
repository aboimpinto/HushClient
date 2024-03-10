using System.Threading.Tasks;
using HushClient.Account;
using HushClient.Model;
using HushClient.Workflows;
using HushEcosystem.Model.GlobalEvents;
using HushEcosystem.Model.Rpc;
using HushEcosystem.Model.Rpc.Profiles;
using Microsoft.CodeAnalysis.CSharp;
using Olimpo;
using Olimpo.NavigationManager;
using ReactiveUI;

namespace HushClient.ViewModels;

public class SearchAccountsViewModel : 
    ViewModelBase,
    IHandle<SearchAccountByPublicKeyRespondedEvent>
{
    private readonly IProfileWorkflow _profileWorkflow;
    private readonly IAccountService _accountService;
    private readonly INavigationManager _navigationManager;
    private readonly IEventAggregator _eventAggregator;
    private string _profileName;
    private string _userProfileKey;
    private string _errorMessage = string.Empty;

    public BlockchainInformation BlockchainInformation { get; }
    public LocalInformation LocalInformation { get; }

    public string ProfileName 
    { 
        get => this._profileName; 
        set => this.RaiseAndSetIfChanged(ref this._profileName, value); 
    }

    public string UserProfileKey 
    { 
        get => this._userProfileKey; 
        set => this.RaiseAndSetIfChanged(ref this._userProfileKey, value); 
    }

    public string ErrorMessage 
    { 
        get => this._errorMessage; 
        set => this.RaiseAndSetIfChanged(ref this._errorMessage, value); 
    }

    public SearchAccountsViewModel(
        IProfileWorkflow profileWorkflow,
        BlockchainInformation blockchainInformation,
        LocalInformation localInformation,
        IAccountService accountService,
        INavigationManager navigationManager,
        IEventAggregator eventAggregator)
    {
        this._profileWorkflow = profileWorkflow;
        this.BlockchainInformation = blockchainInformation;
        this.LocalInformation = localInformation;
        this._accountService = accountService;
        this._navigationManager = navigationManager;
        this._eventAggregator = eventAggregator;

        this._eventAggregator.Subscribe(this);
    }

    public async Task SearchProfileCommand()
    {
        if (!string.IsNullOrEmpty(this.UserProfileKey))
        {
            // search by user profile key 
            await this._profileWorkflow.SearchAccountByPublicKeyAsync(this.UserProfileKey);
            
        }
        else if(!string.IsNullOrEmpty(this.ProfileName))
        {
            // search by profile name
        }
    }

    public void Handle(SearchAccountByPublicKeyRespondedEvent message)
    {
        if (!message.SearchAccountByPublicKeyResponse.Result)
        {
            this.ErrorMessage = message.SearchAccountByPublicKeyResponse.FailureReason;
        }
        else
        {
            if (message.SearchAccountByPublicKeyResponse.UserProfile.UserPublicSigningAddress == this._accountService.UserProfile.PublicSigningAddress)
            this.ErrorMessage = "Cannot create feed for yourself.";
        }
    }
}
