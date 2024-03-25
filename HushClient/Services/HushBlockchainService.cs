using System.Threading.Tasks;
using Grpc.Net.Client;

namespace HushClient.Services;

public class HushBlockchainService : IHushBlockchainService
{
    public HushBlockchainService()
    {
    }

    public async Task<double> GetBlockchainHeightAsync()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5000");
        var client = new HushBlockchain.HushBlockchainClient(channel);
        var reply = await client.GetBlockchainHeightAsync(new GetBlockchainHeightRequest());

        return reply.Index;
    }
}

public interface IHushBlockchainService
{
    Task<double> GetBlockchainHeightAsync();
}