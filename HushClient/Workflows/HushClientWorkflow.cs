using System;
using System.Threading.Tasks;
using HushClient.ApplicationSettings;
using HushClient.TcpClient;
using HushEcosystem;
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

namespace HushClient.Workflows;

public class HushClientWorkflow : 
    IHushClientWorkflow,
    IHandleAsync<HandshakeRespondedEvent>,
    IHandle<BlockchainHeightRespondedEvent>,
    IHandle<TransactionsWithAddressRespondedEvent>,
    IHandleAsync<BalanceByAddressRespondedEvent>,
    IHandleAsync<FeedTransactionHandledEvent>,
    IHandleAsync<FeedMessageTransactionHandledEvent>
{
    private readonly IApplicationSettingsManager _applicationSettingsManager;
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

    public HushClientWorkflow(
        IApplicationSettingsManager applicationSettingsManager,
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
        this._applicationSettingsManager = applicationSettingsManager;
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

        this._localInformation.IsSynchingStream.Subscribe(x => this.OnBlockChainSynched(x));
    }

    public async Task Start()
    {
        this._logger.LogInformation("Starting HushClientWorkflow...");


        this._bootstrapperManager.AllModulesBootstrapped.Subscribe(x => 
        {
            this.InitiateHandShake();
        });

        await this._bootstrapperManager.Start();
    }

    public FeedMessage? SendMessage(string feedId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var feedMessage = new FeedMessageBuilder()
            .WithFeedId(feedId)
            .WithMessageIssuer(this._applicationSettingsManager.UserProfile.PublicSigningAddress)
            .WithMessage(EncryptKeys.Encrypt(message, _applicationSettingsManager.UserProfile.PublicEncryptAddress))
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
        feedMessage.Sign(this._applicationSettingsManager.UserProfile.PublicSigningAddress, signTransactionJsonOptions);

        var sendMessageRequest = new SendFeedMessageRequest
        {
            FeedMessage = feedMessage
        };

        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        this._tcpClientService.Send(sendMessageRequest.ToJson(sendTransactionJsonOptions).Compress());
        return feedMessage;
    }

    private void InitiateHandShake()
    {
        var handshakeRequest = new HandshakeRequestBuilder()
            .WithClientType(ClientType.ClientNode)
            .WithNodeId("nodeId")
            .WithNodeAddressResonsabile("NodeAddressResonsabile")
            .Build();

        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        this._tcpClientService.Send(handshakeRequest.ToJson(sendTransactionJsonOptions).Compress());
    }

    public async Task HandleAsync(HandshakeRespondedEvent message)
    {
        if (message.HandshakeResponse.Result)
        {
            // Handshake accepted
            await this._navigationManager.NavigateAsync("BalanceViewModel");

            this._localInformation.IsSynching = true;

            var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
                .WithTransactionBaseConverter(this._transactionBaseConverter)
                .Build();

            // Request the height of the blockchain
            var blockchainHeightRequest = new BlockchainHeightRequest();
            this._tcpClientService.Send(blockchainHeightRequest.ToJson(sendTransactionJsonOptions).Compress());
        }
        else
        {
            // Handshake not accepted
            // TODO [AboimPinto] Need to implement in case the Handshake is not implemented.
        }
    }

    public void Handle(BlockchainHeightRespondedEvent message)
    {
        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        this._blockchainInformation.BlockchainHeight = message.BlockchainHeightResponse.Height;

        // Request all transactions since last sync for the address
        var lastTransactionsRequest = new TransationsWithAddressRequestBuilder()
            .WithAddress(this._applicationSettingsManager.UserProfile.PublicSigningAddress)
            .WithLastHeightSynched(this._applicationSettingsManager.BlockchainInfo.LastHeightSynched)
            .Build();

        this._tcpClientService.Send(lastTransactionsRequest.ToJson(sendTransactionJsonOptions).Compress());
    }

    public void Handle(TransactionsWithAddressRespondedEvent message)
    {
        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        if (message.TransactionsWithAddressResponse != null && message.TransactionsWithAddressResponse.Transactions != null)
        {
            message.TransactionsWithAddressResponse.Transactions.ForEach(x => 
            {
                // CHECK [AboimPinto] 2024.02.13 Is LocalInformation and ApplicationSettingsManager and BlockchainInformation need to have the same information about LastHeightSynched?
                this._localInformation.LastHeightSynched = x.BlockIndex;
                this._applicationSettingsManager.BlockchainInfo.LastHeightSynched = x.BlockIndex;

                var strategy = this._transactionHandlerStrategies.FirstOrDefault(s => s.CanHandle(x.SpecificTransaction));
                strategy?.Handle(x.SpecificTransaction);
            });
        }

        var BalanceByAddressRequest = new BalanceByAddressRequestBuilder()
            .WithAddress(this._applicationSettingsManager.UserProfile.PublicSigningAddress)
            .Build();

        this._tcpClientService.Send(BalanceByAddressRequest.ToJson(sendTransactionJsonOptions).Compress());
    }

    public async Task HandleAsync(BalanceByAddressRespondedEvent message)
    {
        this._localInformation.Balance = message.BalanceByAddressResponse.Balance;

        await Task.Delay(3000);

        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        var blockchainHeight = new BlockchainHeightRequest();
        this._tcpClientService.Send(blockchainHeight.ToJson(sendTransactionJsonOptions).Compress());
    }

    public async Task HandleAsync(FeedTransactionHandledEvent message)
    {
        if (this._localInformation.SubscribedFeeds.Any(x => x.FeedId == message.Feed.FeedId))
        {
            // Feed already exists
            // TODO [AboimPinto] Need to implement in case the Feed already exists. 
            // Maybe this should be Blockchain validation. Should now allow to create feeds that already exists.
            return;
        }

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

    private void OnBlockChainSynched(bool synched)
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
            this.CreateUserProfileOnBlockchain(hashTransactionJsonOptions, signTransactionJsonOptions, sendTransactionJsonOptions);

            // Create Personal Feed
            this.CreatePersonalFeed(hashTransactionJsonOptions, signTransactionJsonOptions, sendTransactionJsonOptions);
        }
    }

    private void CreatePersonalFeed(
        JsonSerializerOptions hashTransactionJsonOptions, 
        JsonSerializerOptions signTransactionJsonOptions, 
        JsonSerializerOptions sendTransactionJsonOptions)
    {
        var feedEncryptionKeys = new EncryptKeys();

        // For the Personal Feed, the Owner and the Participant are the same
        var personalFeed = new FeedBuilder()
            .WithFeedOwner(this._applicationSettingsManager.UserProfile.PublicSigningAddress)
            .WithFeedParticipantPublicAddress(this._applicationSettingsManager.UserProfile.PublicSigningAddress)
            .WithFeedType(FeedTypeEnum.Personal)
            .WithPublicEncriptAddress(feedEncryptionKeys.PublicKey)
            .WithPrivateEncriptAddress(feedEncryptionKeys.PrivateKey)
            .Build();

        personalFeed.HashObject(hashTransactionJsonOptions);
        personalFeed.Sign(this._applicationSettingsManager.UserProfile.PublicSigningAddress, signTransactionJsonOptions);

        var newFeedRequest = new NewFeedRequest
        {
            Feed = personalFeed
        };
        this._tcpClientService.Send(newFeedRequest.ToJson(sendTransactionJsonOptions).Compress());
        this._ownFeedFound = true;
    }

    private void CreateUserProfileOnBlockchain(
        JsonSerializerOptions hashTransactionJsonOptions, 
        JsonSerializerOptions signTransactionJsonOptions, 
        JsonSerializerOptions sendTransactionJsonOptions)
    {
        var userProfile = new UserProfileBuilder()
            .WithPublicSigningAddress(this._applicationSettingsManager.UserProfile.PublicSigningAddress)
            .WithPublicEncryptAddress(this._applicationSettingsManager.UserProfile.PublicEncryptAddress)
            .WithUserName(this._applicationSettingsManager.UserProfile.ProfileName)
            .Build();

        userProfile.HashObject(hashTransactionJsonOptions);
        userProfile.Sign(this._applicationSettingsManager.UserProfile.PublicSigningAddress, signTransactionJsonOptions);

        var userProfileRequest = new UserProfileRequest
        {
            UserProfile = userProfile
        };
        this._tcpClientService.Send(userProfileRequest.ToJson(sendTransactionJsonOptions).Compress());
    }
}
