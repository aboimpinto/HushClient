using System.Threading.Tasks;

namespace HushClient.Services.ClientService
{
    public interface ITcpClientService
    {
        void Stop();

        Task Start();

        void Send(string commandJson);
    }
}