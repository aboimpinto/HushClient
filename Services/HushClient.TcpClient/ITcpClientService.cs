using System.Threading.Tasks;

namespace HushClient.TcpClient
{
    public interface ITcpClientService
    {
        void Stop();

        Task Start();

        void Send(string commandJson);
    }
}