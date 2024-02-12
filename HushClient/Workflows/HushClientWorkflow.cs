using System;
using System.Threading.Tasks;
using HushClient.ApplicationSettings;
using HushClient.TcpClient;
using HushEcosystem.Model.Rpc.GlobalEvents;
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

namespace HushClient.Workflows;

public class HushClientWorkflow : 
    IHushClientWorkflow,
    IHandleAsync<HandshakeRespondedEvent>,
    IHandle<BlockchainHeightRespondedEvent>,
    IHandle<TransactionsWithAddressRespondedEvent>,
    IHandle<BalanceByAddressRespondedEvent>
{
    private readonly IApplicationSettingsManager _applicationSettingsManager;
    private readonly ITcpClientService _tcpClientService;
    private readonly IBootstrapperManager _bootstrapperManager;
    private readonly BlockchainInformation _blockchainInformation;
    private readonly LocalInformation _localInformation;
    private readonly INavigationManager _navigationManager;
    private readonly IEventAggregator _eventAggregator;
    private readonly TransactionBaseConverter _transactionBaseConverter;
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
        this._logger = logger;

        this._eventAggregator.Subscribe(this);

        this._localInformation.IsSynchingStream.Subscribe(x => 
        {
            if (x && !this._ownFeedFound)
            {
                this._logger.LogInformation("Blockain synched...");

                var feedEncryptionKeys = new EncryptKeys();

                var personalFeed =  new FeedBuilder()
                    .WithFeedOwner(this._applicationSettingsManager.UserInfo.PublicSigningAddress)
                    .WithFeedType(FeedTypeEnum.Personal)
                    .WithPublicEncriptAddress(feedEncryptionKeys.PublicKey)
                    .WithPrivateEncriptAddress(feedEncryptionKeys.PrivateKey)
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

                personalFeed.HashObject(hashTransactionJsonOptions);
                personalFeed.Sign(this._applicationSettingsManager.UserInfo.PublicSigningAddress, signTransactionJsonOptions);

                var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
                    .WithTransactionBaseConverter(this._transactionBaseConverter)
                    .WithModifierExcludeBlockIndex()
                    .Build();

                var newFeedRequest = new NewFeedRequest
                {
                    Feed = personalFeed
                };
                this._tcpClientService.Send(newFeedRequest.ToJson(sendTransactionJsonOptions).Compress());
                this._ownFeedFound = true;
            }
        });
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

        if (this._blockchainInformation.BlockchainHeight == message.BlockchainHeightResponse.Height)
        {
            this._localInformation.IsSynching = false;
            // TODO [AboimPinto] Wait 3s and request sync again

            Task.Delay(3000);

            var blockchainHeight = new BlockchainHeightRequest();
            this._tcpClientService.Send(blockchainHeight.ToJson(sendTransactionJsonOptions).Compress());
        }
        else
        {
            this._localInformation.IsSynching = true;
            this._blockchainInformation.BlockchainHeight = message.BlockchainHeightResponse.Height;

            // Request all transactions since last sync for the address
            var lastTransactionsRequest = new TransationsWithAddressRequestBuilder()
                .WithAddress(this._applicationSettingsManager.UserInfo.PublicSigningAddress)
                .WithLastHeightSynched(this._applicationSettingsManager.BlockchainInfo.LastHeightSynched)
                .Build();

            this._tcpClientService.Send(lastTransactionsRequest.ToJson(sendTransactionJsonOptions).Compress());
        }
    }

    public void Handle(TransactionsWithAddressRespondedEvent message)
    {
        if (message.TransactionsWithAddressResponse != null && message.TransactionsWithAddressResponse.Transactions != null)
        {
            foreach (var item in message.TransactionsWithAddressResponse.Transactions)
            {
                this._localInformation.LastHeightSynched = item.BlockIndex;

                if (item.SpecificTransaction.TransactionId == Feed.TypeCode)
                {
                    // TODO [AboimPinto] Need to implement the logic to handle the subscription to the feed
                }
            }
        }

        var sendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this._transactionBaseConverter)
            .Build();

        var blockchainHeightRequest = new BlockchainHeightRequest();
        this._tcpClientService.Send(blockchainHeightRequest.ToJson(sendTransactionJsonOptions).Compress());

        var BalanceByAddressRequest = new BalanceByAddressRequestBuilder()
            .WithAddress(this._applicationSettingsManager.UserInfo.PublicSigningAddress)
            .Build();

        this._tcpClientService.Send(BalanceByAddressRequest.ToJson(sendTransactionJsonOptions).Compress());
    }

    public void Handle(BalanceByAddressRespondedEvent message)
    {
        this._localInformation.Balance = message.BalanceByAddressResponse.Balance;
    }
}
