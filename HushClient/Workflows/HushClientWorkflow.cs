using System;
using System.Threading.Tasks;
using HushClient.ApplicationSettings;
using HushClient.TcpClient;
using Microsoft.Extensions.Logging;
using Olimpo;
using HushEcosystem.Model.Rpc.Handshake;
using HushEcosystem.Model.Rpc;
using HushEcosystem.Model.Rpc.Blockchain;
using HushEcosystem.Model.Rpc.Transactions;
using HushClient.Model;
using Olimpo.NavigationManager;
using HushEcosystem.Model;
using HushEcosystem.Model.Builders;
using HushEcosystem.Model.Blockchain;
using HushEcosystem.Model.Rpc.Feeds;
using HushEcosystem.Model.GlobalEvents;
using System.Collections.Generic;
using HushEcosystem.Model.Blockchain.TransactionHandlerStrategies;
using System.Linq;
using HushClient.GlobalEvents;
using HushEcosystem.Model.Rpc.Profiles;
using System.Text.Json;
using HushClient.Account;
using System.Threading;

namespace HushClient.Workflows;

public class HushClientWorkflow : 
    IHushClientWorkflow,
    IHandleAsync<HandshakeRespondedEvent>,
    IHandleAsync<ProfileUserLoadedEvent>,
    IHandleAsync<BlockchainHeightRespondedEvent>,
    IHandleAsync<TransactionsWithAddressRespondedEvent>,
    IHandleAsync<BalanceByAddressRespondedEvent>,
    IHandleAsync<FeedTransactionHandledEvent>,
    IHandleAsync<FeedMessageTransactionHandledEvent>,
    IHandleAsync<FeedsForAddressRespondedEvent>
{
    private readonly IProfileWorkflow _profileWorkflow;
    private readonly IApplicationSettingsManager _applicationSettingsManager;
    private readonly IAccountService _accountService;
    private readonly ITcpClientService _tcpClientService;
    private readonly IBootstrapperManager _bootstrapperManager;
    private readonly BlockchainInformation _blockchainInformation;
    private readonly LocalInformation _localInformation;
    private readonly INavigationManager _navigationManager;
    private readonly IEventAggregator _eventAggregator;
    private readonly TransactionBaseConverter _transactionBaseConverter;
    private readonly IEnumerable<ITransactionHandlerStrategy> _transactionHandlerStrategies;
    private readonly ILogger<HushClientWorkflow> _logger;

    private bool _ownFeedFound = false;

    private bool _isConnected = false;

    private bool _isProfileLoaded = false;

    public HushClientWorkflow(
        IProfileWorkflow profileWorkflow,
        IApplicationSettingsManager applicationSettingsManager,
        IAccountService accountService,
        ITcpClientService tcpClientService,
        IBootstrapperManager bootstrapperManager,
        BlockchainInformation blockchainInformation,
        LocalInformation localInformation,
        INavigationManager navigationManager,
        IEventAggregator eventAggregator,
        TransactionBaseConverter transactionBaseConverter,
        IEnumerable<ITransactionHandlerStrategy> transactionHandlerStrategies,
        ILogger<HushClientWorkflow> logger)
    {
        this._profileWorkflow = profileWorkflow;
        this._applicationSettingsManager = applicationSettingsManager;
        this._accountService = accountService;
        this._tcpClientService = tcpClientService;
        this._bootstrapperManager = bootstrapperManager;
        this._blockchainInformation = blockchainInformation;
        this._localInformation = localInformation;
        this._navigationManager = navigationManager;
        this._eventAggregator = eventAggregator;
        this._transactionBaseConverter = transactionBaseConverter;
        this._transactionHandlerStrategies = transactionHandlerStrategies;
        this._logger = logger;

        this._eventAggregator.Subscribe(this);

        this._localInformation.IsSynchingStream.Subscribe(async x => this.OnBlockChainSynchedAsync(x));
    }

    public async Task Start()
    {
        this._logger.LogInformation("Starting HushClientWorkflow...");

        this._bootstrapperManager.AllModulesBootstrapped.Subscribe(async x => 
        {
            await this.InitiateHandShake();
        });

        await this._bootstrapperManager.Start();

        await this._navigationManager.NavigateAsync("BalanceViewModel");
    }

    public async Task<FeedMessage?> SendMessage(string feedId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var feedMessage = new FeedMessageBuilder()
            .WithFeedId(feedId)
            .WithMessageIssuer(this._accountService.UserProfile.PublicSigningAddress)
            .WithMessage(EncryptKeys.Encrypt(message, _accountService.UserProfile.PublicEncryptAddress))
            .Build();

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

        feedMessage.HashObject(hashTransactionJsonOptions);
        feedMessage.Sign(this._accountService.UserProfile.PublicSigningAddress, signTransactionJsonOptions);

        var sendMessageRequest = new SendFeedMessageRequest
        {
            FeedMessage = feedMessage
        };

        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        await this._tcpClientService.Send(sendMessageRequest.ToJson(sendTransactionJsonOptions));
        return feedMessage;
    }

    private async Task InitiateHandShake()
    {
        var handshakeRequest = new HandshakeRequestBuilder()
            .WithClientType(ClientType.ClientNode)
            .WithNodeId("nodeId")
            .WithNodeAddressResonsabile("NodeAddressResonsabile")
            .Build();

        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        await this._tcpClientService
            .Send(handshakeRequest.ToJson(sendTransactionJsonOptions));
    }

    public async Task HandleAsync(HandshakeRespondedEvent message)
    {
        if (message.HandshakeResponse.Result)
        {
            // Handshake accepted
            await this._profileWorkflow.LoadAccountsAsync();
            this._isConnected = true;
        }
        else
        {
            // Handshake not accepted
            // TODO [AboimPinto] Need to implement in case the Handshake is not implemented.
            this._isConnected = false;
        }
    }

    public async Task HandleAsync(ProfileUserLoadedEvent message)
    {
        var sendBlockchainHeightRequestTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        var blockchainHeight = new BlockchainHeightRequest();
        await this._tcpClientService.Send(blockchainHeight.ToJson(sendBlockchainHeightRequestTransactionJsonOptions));

        if (!this._isProfileLoaded)
        {
            this._isProfileLoaded = true;

            // await this._navigationManager.NavigateAsync("BalanceViewModel");

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

            // Create UserProfile
            await this.CreateUserProfileOnBlockchainAsync(hashTransactionJsonOptions, signTransactionJsonOptions, sendTransactionJsonOptions);

            // Create Personal Feed
            await this.CreatePersonalFeedAsync(hashTransactionJsonOptions, signTransactionJsonOptions, sendTransactionJsonOptions);

            // HACK [AboimPinto] 2024.03.04 Create a chat feed with the other user
            // await this.CreateFeedWithAboimPintoAsync(hashTransactionJsonOptions, signTransactionJsonOptions, sendTransactionJsonOptions);
        }
    }

    public async Task HandleAsync(BlockchainHeightRespondedEvent message)
    {
        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        this._blockchainInformation.BlockchainHeight = message.BlockchainHeightResponse.Height;

        // Request all transactions since last sync for the address
        var lastTransactionsRequest = new TransationsWithAddressRequestBuilder()
            .WithAddress(this._accountService.UserProfile.PublicSigningAddress)
            .WithLastHeightSynched(this._applicationSettingsManager.BlockchainInfo.LastHeightSynched)
            .Build();

        await this._tcpClientService.Send(lastTransactionsRequest.ToJson(sendTransactionJsonOptions));
    }

    public async Task HandleAsync(TransactionsWithAddressRespondedEvent message)
    {
        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        // HACK: [AboimPinto] For debug purposes
        // Console.WriteLine($"--> {DateTime.UtcNow}: {message.TransactionsWithAddressResponse.Transactions.Count} until {message.TransactionsWithAddressResponse.BlockHeightSyncPoint}");

        if (message.TransactionsWithAddressResponse != null && message.TransactionsWithAddressResponse.Transactions != null)
        {
            message.TransactionsWithAddressResponse.Transactions.ForEach(x => 
            {
                // CHECK [AboimPinto] 2024.02.13 Is LocalInformation and ApplicationSettingsManager and BlockchainInformation need to have the same information about LastHeightSynched?
                this._localInformation.LastHeightSynched = x.BlockIndex;

                var strategy = this._transactionHandlerStrategies.FirstOrDefault(s => s.CanHandle(x.SpecificTransaction));
                strategy?.Handle(x.SpecificTransaction);
            });
        }

        this._applicationSettingsManager.BlockchainInfo.LastHeightSynched = message.TransactionsWithAddressResponse.BlockHeightSyncPoint;

        var BalanceByAddressRequest = new BalanceByAddressRequestBuilder()
            .WithAddress(this._accountService.UserProfile.PublicSigningAddress)
            .Build();

        await this._tcpClientService.Send(BalanceByAddressRequest.ToJson(sendTransactionJsonOptions));
    }

    public async Task HandleAsync(BalanceByAddressRespondedEvent message)
    {
        this._localInformation.Balance = message.BalanceByAddressResponse.Balance;

        await Task.Delay(3000);

        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        var blockchainHeight = new BlockchainHeightRequest();
        await this._tcpClientService.Send(blockchainHeight.ToJson(sendTransactionJsonOptions));

        var feedsForAddressRequest = new FeedsForAddressRequest
        {
            Address = this._accountService.UserProfile.PublicSigningAddress,
            SinceBlockIndex = this._localInformation.LastHeightSynched
        };
        await this._tcpClientService.Send(feedsForAddressRequest.ToJson(sendTransactionJsonOptions));
    }

    public async Task HandleAsync(FeedTransactionHandledEvent message)
    {
        Console.WriteLine("Handling new feed...");

        if (this._localInformation.SubscribedFeeds.Any(x => x.FeedId == message.Feed.FeedId))
        {
            Console.WriteLine("Feed already subscribed");

            // Feed already exists
            // TODO [AboimPinto] Need to implement in case the Feed already exists. 
            // Maybe this should be Blockchain validation. Should now allow to create feeds that already exists.
            return;
        }

        Console.WriteLine("Feed subscribed and RefreshFeedsEvent published");
        this._localInformation.SubscribedFeeds.Add(message.Feed);
        await this._eventAggregator.PublishAsync(new RefreshFeedsEvent());
    }

    public async Task HandleAsync(FeedMessageTransactionHandledEvent message)
    {
        if(this._localInformation.SubscribedFeedMessages.ContainsKey(message.FeedMessage.FeedId))
        {
            this._localInformation.SubscribedFeedMessages[message.FeedMessage.FeedId].Add(message.FeedMessage);
        }
        else
        {
            this._localInformation.SubscribedFeedMessages.Add(message.FeedMessage.FeedId, new List<FeedMessage> { message.FeedMessage });
        }

        await this._eventAggregator.PublishAsync(new RefreshFeedMessagesEvent(message.FeedMessage.FeedId));
    }

    public async Task HandleAsync(FeedsForAddressRespondedEvent message)
    {
        foreach(var item in message.FeedsForAddressResponse.FeedDefinitions)
        {
            if (this._localInformation.SubscribedFeedsDefinitions.Any(x => x.FeedId == item.FeedId))
            {
                // the feed is already added. Just update the name
                var feed = this._localInformation.SubscribedFeedsDefinitions.Single(x => x.FeedId == item.FeedId);
                feed.FeedTitle = item.FeedTitle;
            }
            else
            {
                this._localInformation.SubscribedFeedsDefinitions.Add(item);
            }
        }

        await this._eventAggregator.PublishAsync(new RefreshFeedsEvent());
    }

    private async Task OnBlockChainSynchedAsync(bool synched)
    {
        if (synched && !this._ownFeedFound)
        {
            this._logger.LogInformation("Blockain synched...");

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

            // Create UserProfile
            // await this.CreateUserProfileOnBlockchainAsync(hashTransactionJsonOptions, signTransactionJsonOptions, sendTransactionJsonOptions);

            // Create Personal Feed
            // await this.CreatePersonalFeedAsync(hashTransactionJsonOptions, signTransactionJsonOptions, sendTransactionJsonOptions);

            // HACK [AboimPinto] 2024.03.04 Create a chat feed with the other user
            // await this.CreateFeedWithAboimPintoAsync(hashTransactionJsonOptions, signTransactionJsonOptions, sendTransactionJsonOptions);
        }
    }

    private async Task CreatePersonalFeedAsync(
        JsonSerializerOptions hashTransactionJsonOptions, 
        JsonSerializerOptions signTransactionJsonOptions, 
        JsonSerializerOptions sendTransactionJsonOptions)
    {
        var feedEncryptionKeys = new EncryptKeys();

        // For the Personal Feed, the Owner and the Participant are the same
        var personalFeed = new FeedBuilder()
            .WithFeedOwner(this._accountService.UserProfile.PublicSigningAddress)
            .WithFeedParticipantPublicAddress(this._accountService.UserProfile.PublicSigningAddress)
            .WithFeedType(FeedTypeEnum.Personal)
            .WithPublicEncriptAddress(feedEncryptionKeys.PublicKey)
            .WithPrivateEncriptAddress(feedEncryptionKeys.PrivateKey)
            .Build();

        personalFeed.HashObject(hashTransactionJsonOptions);
        personalFeed.Sign(this._accountService.UserProfile.PublicSigningAddress, signTransactionJsonOptions);

        var newFeedRequest = new NewFeedRequest
        {
            Feed = personalFeed
        };
        
        await this._tcpClientService.Send(newFeedRequest.ToJson(sendTransactionJsonOptions));
        this._ownFeedFound = true;
    }

    private async Task CreateUserProfileOnBlockchainAsync(
        JsonSerializerOptions hashTransactionJsonOptions, 
        JsonSerializerOptions signTransactionJsonOptions, 
        JsonSerializerOptions sendTransactionJsonOptions)
    {
        var userProfile = new UserProfileBuilder()
            .WithPublicSigningAddress(this._accountService.UserProfile.PublicSigningAddress)
            .WithPublicEncryptAddress(this._accountService.UserProfile.PublicEncryptAddress)
            .WithUserName(this._accountService.UserProfile.ProfileName)
            .WithIsPublicFlag(this._accountService.UserProfile.IsPublic)
            .Build();

        userProfile.HashObject(hashTransactionJsonOptions);
        userProfile.Sign(this._accountService.UserProfile.PublicSigningAddress, signTransactionJsonOptions);

        var userProfileRequest = new UserProfileRequest
        {
            UserProfile = userProfile
        };

        await this._tcpClientService.Send(userProfileRequest.ToJson(sendTransactionJsonOptions));
    }

    // private async Task CreateFeedWithAboimPintoAsync(
    //     JsonSerializerOptions hashTransactionJsonOptions, 
    //     JsonSerializerOptions signTransactionJsonOptions, 
    //     JsonSerializerOptions sendTransactionJsonOptions)
    // {
    //     var feedEncryptionKeys = new EncryptKeys();

    //     // Add user to ChatFeed
    //     var participantMeToFeed = new FeedBuilder()
    //         .WithFeedOwner(this._accountService.UserProfile.PublicSigningAddress)
    //         .WithFeedParticipantPublicAddress(this._accountService.UserProfile.PublicSigningAddress)
    //         .WithFeedType(FeedTypeEnum.Chat)
    //         .WithPublicEncriptAddress(feedEncryptionKeys.PublicKey)
    //         .WithPrivateEncriptAddress(feedEncryptionKeys.PrivateKey)
    //         .Build();

    //     participantMeToFeed.HashObject(hashTransactionJsonOptions);
    //     participantMeToFeed.Sign(this._accountService.UserProfile.PublicSigningAddress, signTransactionJsonOptions);

    //     var meFeedRequest = new NewFeedRequest
    //     {
    //         Feed = participantMeToFeed
    //     };

    //     await this._tcpClientService.Send(meFeedRequest.ToJson(sendTransactionJsonOptions));

    //     // Add user to ChatFeed
    //     var participantOtherToFeed = new FeedBuilder()
    //         .WithFeedOwner(this._accountService.UserProfile.PublicSigningAddress)
    //         .WithFeedParticipantPublicAddress("04f505dab724c28db882376ca25d470cbabdc188f15a20a2083f49756bc00dd12ac44679afc9f4d658df07310f68191a633e31cd80ccf407a6a065d5b18874433f")
    //         .WithFeedType(FeedTypeEnum.Chat)
    //         .WithPublicEncriptAddress(feedEncryptionKeys.PublicKey)
    //         .WithPrivateEncriptAddress(feedEncryptionKeys.PrivateKey)
    //         .Build();

    //     participantOtherToFeed.HashObject(hashTransactionJsonOptions);
    //     participantOtherToFeed.Sign(this._accountService.UserProfile.PublicSigningAddress, signTransactionJsonOptions);

    //     var otherFeedRequest = new NewFeedRequest
    //     {
    //         Feed = participantOtherToFeed
    //     };

    //     await this._tcpClientService.Send(otherFeedRequest.ToJson(sendTransactionJsonOptions));
    // }
}

