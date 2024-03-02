using System.Threading.Tasks;
using HushEcosystem.Model.Blockchain;

namespace HushClient.Workflows;

public interface IHushClientWorkflow
{
    Task Start();

    FeedMessage? SendMessage(string feedId, string message);
}
