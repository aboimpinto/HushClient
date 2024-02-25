using HushEcosystem;
using HushEcosystem.Model.Rpc.CommandDeserializeStrategies;
using Microsoft.Extensions.Logging;
using Olimpo.TcpClientManager;

namespace HushClient.TcpClient
{
    public class TcpClientService : ITcpClientService
    {
        private readonly IClient _client;
        private readonly IEnumerable<ICommandDeserializeStrategy> _strategies;
        private readonly ILogger<TcpClientService> _logger;

        public TcpClientService(
            IClient client,
            IEnumerable<ICommandDeserializeStrategy> strategies, 
            ILogger<TcpClientService> logger)
        {
            this._client = client;
            this._strategies = strategies;
            this._logger = logger;
        }

        public void Stop()
        {
            this._client.Stop();
        }

        public Task Start()
        {
            this._logger.LogInformation("Starting TcpClient...");

            this._client.Start("localhost", 4665);

            this._client.DataReceived.Subscribe(x => 
            {
                var decompressedMessage = x.Message.Decompress();

                var commandStrategy = this._strategies.SingleOrDefault(x => x.CanHandle(decompressedMessage));
                if (commandStrategy == null)
                {
                    throw new InvalidOperationException($"There is no strategy for the command: : {decompressedMessage}");
                }

                commandStrategy.Handle(decompressedMessage, string.Empty);
            });

            return Task.CompletedTask;
        }

        public void Send(string commandJson)
        {
            this._client.Channel.Send(commandJson);
        }
    }
}