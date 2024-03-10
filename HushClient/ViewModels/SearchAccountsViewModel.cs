using System.Threading.Tasks;
using HushClient.Model;
using HushClient.Workflows;
using HushEcosystem.Model.Rpc;
using HushEcosystem.Model.Rpc.Profiles;
using Microsoft.CodeAnalysis.CSharp;
using Olimpo.NavigationManager;
using ReactiveUI;

namespace HushClient.ViewModels;

public class SearchAccountsViewModel : ViewModelBase
{
    private readonly IProfileWorkflow _profileWorkflow;
    private readonly INavigationManager _navigationManager;
    private string _profileName;
    private string _userProfileKey;

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

    public SearchAccountsViewModel(
        IProfileWorkflow profileWorkflow,
        BlockchainInformation blockchainInformation,
        LocalInformation localInformation,
        INavigationManager navigationManager)
    {
        this._profileWorkflow = profileWorkflow;
        this.BlockchainInformation = blockchainInformation;
        this.LocalInformation = localInformation;

        this._navigationManager = navigationManager;
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
}
