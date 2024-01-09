using System.Reactive.Subjects;
using Olimpo;

namespace HushClient.TcpClient
{
    public class TcpClientBootstrapper : IBootstrapper
    {
        private readonly ITcpClientService _tcpClientService;

        public Subject<bool> BootstrapFinished { get; }

        public int Priority { get; set; } = 10;

        public TcpClientBootstrapper(ITcpClientService tcpClientService)
        {
            this._tcpClientService = tcpClientService;

            this.BootstrapFinished = new Subject<bool>();
        }

        public void Shutdown()
        {
            this._tcpClientService.Stop();
        }

        public async Task Startup()
        {
            await this._tcpClientService.Start();
            
            this.BootstrapFinished.OnNext(true);
        }
    }
}