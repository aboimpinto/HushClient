using System.Threading.Tasks;
using HushClient.Account;
using HushClient.Account.Model;
using HushClient.GlobalEvents;
using HushClient.TcpClient;
using HushEcosystem.Model;
using HushEcosystem.Model.Builders;
using HushEcosystem.Model.GlobalEvents;
using HushEcosystem.Model.Rpc.Profiles;
using Olimpo;
using Org.BouncyCastle.Asn1.Utilities;

namespace HushClient.Workflows;

public class ProfileWorkflow : 
    WorkflowBase, 
    IProfileWorkflow,
    IHandleAsync<UserProfileTransactionHandledEvent>
{
    private readonly IAccountService _accountService;
    private readonly IEventAggregator _eventAggregator;
    private readonly ITcpClientService _tcpClientService;

    public ProfileWorkflow(
        IAccountService accountService, 
        TransactionBaseConverter transactionBaseConverter,
        IEventAggregator eventAggregator,
        ITcpClientService tcpClientService) : base(transactionBaseConverter)
    {
        this._accountService = accountService;
        this._eventAggregator = eventAggregator;
        this._tcpClientService = tcpClientService;
    }

    public async Task SetNewProfileAsync(UserProfile userProfile)
    {
        var profile = new UserProfileBuilder()
            .WithPublicSigningAddress(userProfile.PublicSigningAddress)
            .WithPublicEncryptAddress(userProfile.PublicEncryptAddress)
            .WithUserName(userProfile.ProfileName)
            .WithIsPublicFlag(userProfile.IsPublic)
            .Build();

        profile.HashObject(this.HashTransactionJsonOptions);
        profile.Sign(userProfile.PublicSigningAddress, this.SignTransactionJsonOptions);

        var userProfileRequest = new UserProfileRequest
        {
            UserProfile = profile
        };

        await this._tcpClientService.Send(userProfileRequest.ToJson(this.SendTransactionJsonOptions));
        await this._accountService.SaveProfileToJsonAsync(userProfile);
    }

    public async Task LoadAccountsAsync()
    {
        await this._accountService.LoadAccountsAsync();

        await this._eventAggregator.PublishAsync(new ProfileUserLoadedEvent());
    }

    public async Task HandleAsync(UserProfileTransactionHandledEvent message)
    {
        await this._accountService.UpdateProfileUserNameAsync(message.UserProfile.UserName);

        await this._eventAggregator.PublishAsync(new ProfileUserLoadedEvent());
    }

    public async Task SearchAccountByPublicKeyAsync(string publicKey)
    {
        var request = new SearchAccountByPublicKeyRequest
        {
            UserPublicKey = publicKey
        };

        await this._tcpClientService.Send(request.ToJson(this.SendTransactionJsonOptions));
    }
}
