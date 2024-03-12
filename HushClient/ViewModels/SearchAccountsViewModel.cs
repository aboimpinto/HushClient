using System.Threading.Tasks;
using HushClient.Account;
using HushClient.Model;
using HushClient.TcpClient;
using HushClient.Workflows;
using HushEcosystem.Model;
using HushEcosystem.Model.Blockchain;
using HushEcosystem.Model.Builders;
using HushEcosystem.Model.GlobalEvents;
using HushEcosystem.Model.Rpc.Feeds;
using Olimpo;
using Olimpo.NavigationManager;
using ReactiveUI;

namespace HushClient.ViewModels;

public class SearchAccountsViewModel : 
    ViewModelBase,
    IHandleAsync<SearchAccountByPublicKeyRespondedEvent>
{
    private readonly IProfileWorkflow _profileWorkflow;
    private readonly IAccountService _accountService;
    private readonly TransactionBaseConverter _transactionBaseConverter;
    private readonly INavigationManager _navigationManager;
    private readonly IEventAggregator _eventAggregator;
    private readonly ITcpClientService _tcpClientService;
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
        TransactionBaseConverter transactionBaseConverter,
        INavigationManager navigationManager,
        IEventAggregator eventAggregator,
        ITcpClientService tcpClientService)
    {
        this._profileWorkflow = profileWorkflow;
        this.BlockchainInformation = blockchainInformation;
        this.LocalInformation = localInformation;
        this._accountService = accountService;
        this._transactionBaseConverter = transactionBaseConverter;
        this._navigationManager = navigationManager;
        this._eventAggregator = eventAggregator;
        this._tcpClientService = tcpClientService;
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

    public async Task HandleAsync(SearchAccountByPublicKeyRespondedEvent message)
    {
        if (!message.SearchAccountByPublicKeyResponse.Result)
        {
            this.ErrorMessage = message.SearchAccountByPublicKeyResponse.FailureReason;
            return;
        }
        else
        {
            if (message.SearchAccountByPublicKeyResponse.UserProfile.UserPublicSigningAddress == this._accountService.UserProfile.PublicSigningAddress)
            {
                this.ErrorMessage = "Cannot create feed for yourself.";
                return;
            }
        }

        // Create new feed of type chat for this user
        var feedEncryptionKeys = new EncryptKeys();

        var hashTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .WithModifierExcludeSignature()
            .WithModifierExcludeBlockIndex()
            .WithModifierExcludeHash()
            .Build();

        var signTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .WithModifierExcludeSignature()
            .WithModifierExcludeBlockIndex()
            .Build();

        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .WithModifierExcludeBlockIndex()
            .Build();

        var feedOwn = new FeedBuilder()
            .WithFeedOwner(this._accountService.UserProfile.PublicSigningAddress)
            .WithFeedType(FeedTypeEnum.Chat)
            .WithFeedParticipantPublicAddress(this._accountService.UserProfile.PublicSigningAddress)
            .WithPublicEncriptAddress(feedEncryptionKeys.PublicKey)
            .WithPrivateEncriptAddress(feedEncryptionKeys.PrivateKey)
            .Build();

        feedOwn.HashObject(hashTransactionJsonOptions);
        feedOwn.Sign(this._accountService.UserProfile.PublicSigningAddress, signTransactionJsonOptions);

        var meFeedRequest = new NewFeedRequest
        {
            Feed = feedOwn
        };
        await this._tcpClientService.Send(meFeedRequest.ToJson(sendTransactionJsonOptions));

        var feedOther = new FeedBuilder()
            .WithFeedOwner(this._accountService.UserProfile.PublicSigningAddress)
            .WithFeedType(FeedTypeEnum.Chat)
            .WithFeedParticipantPublicAddress(message.SearchAccountByPublicKeyResponse.UserProfile.UserPublicSigningAddress)
            .WithPublicEncriptAddress(feedEncryptionKeys.PublicKey)
            .WithPrivateEncriptAddress(feedEncryptionKeys.PrivateKey)
            .Build();

        feedOther.HashObject(hashTransactionJsonOptions);
        feedOther.Sign(this._accountService.UserProfile.PublicSigningAddress, signTransactionJsonOptions);

        var otherFeedRequest = new NewFeedRequest
        {
            Feed = feedOther
        };
        await this._tcpClientService.Send(otherFeedRequest.ToJson(sendTransactionJsonOptions));
    }
}
