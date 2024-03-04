using System.Threading.Tasks;

namespace HushClient.TcpClient
{
    public interface ITcpClientService
    {
        void Stop();

        Task Start();

        Task Send(string commandJson);
    }
}