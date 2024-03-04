using System.Threading.Tasks;
using HushEcosystem.Model.Blockchain;

namespace HushClient.Workflows;

public interface IHushClientWorkflow
{
    Task Start();

    Task<FeedMessage?> SendMessage(string feedId, string message);
}
