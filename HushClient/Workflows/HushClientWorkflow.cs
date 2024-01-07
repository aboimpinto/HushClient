using System;
using System.Threading.Tasks;
using HushClient.Services.ClientService;
using HushEcosystem.RpcModel;
using HushEcosystem.RpcModel.Handshake;
using Microsoft.Extensions.Logging;
using Olimpo;

namespace HushClient.Workflows;

public class HushClientWorkflow : IHushClientWorkflow
{
    private readonly ITcpClientService _tcpClientService;
    private readonly IBootstrapperManager _bootstrapperManager;
        private readonly ILogger<HushClientWorkflow> _logger;

    public HushClientWorkflow(
        ITcpClientService tcpClientService,
        IBootstrapperManager bootstrapperManager,
        ILogger<HushClientWorkflow> logger)
    {
        this._tcpClientService = tcpClientService;
        this._bootstrapperManager = bootstrapperManager;
        this._logger = logger;
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
            .WithClientType(HushEcosystem.RpcModel.ClientType.ClientNode)
            .WithNodeId("nodeId")
            .WithNodeAddressResonsabile("NodeAddressResonsabile")
            .Build();

        // var jsonObject = handshakeRequest.ToJson();
        // var compressObject = handshakeRequest.Compress(jsonObject);
        // var uncompressObject = handshakeRequest.Decompress(compressObject);

        this._tcpClientService.Send(handshakeRequest.ToJson().Compress());
    }
}
