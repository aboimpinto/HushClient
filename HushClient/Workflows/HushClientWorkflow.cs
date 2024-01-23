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

namespace HushClient.Workflows;

public class HushClientWorkflow : 
    IHushClientWorkflow,
    IHandleAsync<HandshakeResponseEvent>,
    IHandle<BlockchainHeightResponseEvent>,
    IHandle<TransactionsWithAddressResponseEvent>
{
    private readonly IApplicationSettingsManager _applicationSettingsManager;
    private readonly ITcpClientService _tcpClientService;
    private readonly IBootstrapperManager _bootstrapperManager;
    private readonly BlockchainInformation _blockchainInformation;
    private readonly LocalInformation _localInformation;
    private readonly INavigationManager _navigationManager;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<HushClientWorkflow> _logger;

    // private double _blockchainHeight;
    // private double _lastBlockHeightProcessed;
    // private bool _isSyncing;

    public HushClientWorkflow(
        IApplicationSettingsManager applicationSettingsManager,
        ITcpClientService tcpClientService,
        IBootstrapperManager bootstrapperManager,
        BlockchainInformation blockchainInformation,
        LocalInformation localInformation,
        INavigationManager navigationManager,
        IEventAggregator eventAggregator,
        ILogger<HushClientWorkflow> logger)
    {
        this._applicationSettingsManager = applicationSettingsManager;
        this._tcpClientService = tcpClientService;
        this._bootstrapperManager = bootstrapperManager;
        this._blockchainInformation = blockchainInformation;
        this._localInformation = localInformation;
        this._navigationManager = navigationManager;
        this._eventAggregator = eventAggregator;
        this._logger = logger;

        this._eventAggregator.Subscribe(this);
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

        this._tcpClientService.Send(handshakeRequest.ToJson().Compress());
    }

    public async Task HandleAsync(HandshakeResponseEvent message)
    {
        if (message.HandshakeResponse.Result)
        {
            // Handshake accepted
            await this._navigationManager.NavigateAsync("BalanceViewModel");

            this._localInformation.IsSynching = true;
            // Request the height of the blockchain
            var blockchainHeight = new BlockchainHeightRequest();
            this._tcpClientService.Send(blockchainHeight.ToJson().Compress());
        }
        else
        {
            // Handshake not accepted
            // TODO [AboimPinto] Need to implement in case the Handshake is not implemented.
        }
    }

    public void Handle(BlockchainHeightResponseEvent message)
    {
        if (this._blockchainInformation.BlockchainHeight == message.BlockchainHeightResponse.Height)
        {
            this._localInformation.IsSynching = false;
            // TODO [AboimPinto] Wait 3s and request sync again

            Task.Delay(3000);
            var blockchainHeight = new BlockchainHeightRequest();
            this._tcpClientService.Send(blockchainHeight.ToJson().Compress());
        }
        else
        {
            this._localInformation.IsSynching = true;
            this._blockchainInformation.BlockchainHeight = message.BlockchainHeightResponse.Height;

            // Request all transactions since last sync for the address
            var lastTransactionsCommand = new TransationsWithAddressRequestBuilder()
                .WithAddress(this._applicationSettingsManager.UserInfo.PublicSigningAddress)
                .WithLastHeightSynched(this._applicationSettingsManager.BlockchainInfo.LastHeightSynched)
                .Build();

            this._tcpClientService.Send(lastTransactionsCommand.ToJson().Compress());
        }

    }

    public void Handle(TransactionsWithAddressResponseEvent message)
    {
        foreach (var item in message.TransactionsWithAddressResponse.Transactions)
        {
            this._localInformation.LastHeightSynched = item.BlockHeight;
        }

        var blockchainHeight = new BlockchainHeightRequest();
        this._tcpClientService.Send(blockchainHeight.ToJson().Compress());
    }
}
