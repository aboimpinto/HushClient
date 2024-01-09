using System;
using System.Threading.Tasks;
using HushClient.ApplicationSettings;
using HushClient.TcpClient;
using HushEcosystem.RpcModel;
using HushEcosystem.RpcModel.GlobalEvents;
using HushEcosystem.RpcModel.Handshake;
using HushEcosystem.RpcModel.Transactions;
using Microsoft.Extensions.Logging;
using Olimpo;

namespace HushClient.Workflows;

public class HushClientWorkflow : 
    IHushClientWorkflow,
    IHandle<HandshakeResponseEvent>
{
    private readonly IApplicationSettingsManager _applicationSettingsManager;
    private readonly ITcpClientService _tcpClientService;
    private readonly IBootstrapperManager _bootstrapperManager;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<HushClientWorkflow> _logger;

    public HushClientWorkflow(
        IApplicationSettingsManager applicationSettingsManager,
        ITcpClientService tcpClientService,
        IBootstrapperManager bootstrapperManager,
        IEventAggregator eventAggregator,
        ILogger<HushClientWorkflow> logger)
    {
        this._applicationSettingsManager = applicationSettingsManager;
        this._tcpClientService = tcpClientService;
        this._bootstrapperManager = bootstrapperManager;
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

    public void Handle(HandshakeResponseEvent message)
    {
        if (message.HandshakeResponse.Result)
        {
            // Handshake accepted

            // Request all transactions since last sync for the address
            var lastTransactionsCommand = new TransationsWithAddressRequestBuilder()
                .WithAddress(this._applicationSettingsManager.UserInfo.PublicSigningAddress)
                .WithLastHeightSynched(this._applicationSettingsManager.BlockchainInfo.LastHeightSynched)
                .Build();

            this._tcpClientService.Send(lastTransactionsCommand.ToJson().Compress());
        }
        else
        {
            // Handshake not accepted
            // TODO [AboimPinto] Need to implement in case the Handshake is not implemented.
        }
    }
}
