using System.Threading.Tasks;

namespace HushClient.Workflows;

public interface IHushClientWorkflow
{
    Task Start();

    void SendMessage(string feedId, string message);
}
