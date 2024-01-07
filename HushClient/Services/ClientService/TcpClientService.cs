using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Olimpo.TcpClientManager;

namespace HushClient.Services.ClientService
{
    public class TcpClientService : ITcpClientService
    {
        private readonly IClient _client;
        private readonly ILogger<TcpClientService> _logger;

        public TcpClientService(
            IClient client,
            ILogger<TcpClientService> logger)
        {
            this._client = client;
            this._logger = logger;
        }

        public void Stop()
        {
        }

        public Task Start()
        {
            this._logger.LogInformation("Starting TcpClient...");

            this._client.Start("localhost", 4665);

            return Task.CompletedTask;
        }

        public void Send(string commandJson)
        {
            this._client.Channel.Send(commandJson);
        }
    }
}