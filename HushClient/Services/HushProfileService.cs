using System;
using System.Threading.Tasks;
using Grpc.Net.Client;

namespace HushClient.Services;

public class HushProfileService : IHushProfileService
{
    // private HushProfile.HushProfileClient _client;

    public HushProfileService()
    {
        // using var channel = GrpcChannel.ForAddress("http://localhost:5000");
        // this._client = new HushProfile.HushProfileClient(channel);
    }

    public async Task SetProfileAsync()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5000");
        var client = new HushProfile.HushProfileClient(channel);
        var reply = await client.SetProfileAsync(new SetProfileRequest { Name = "xxxx" });

        Console.WriteLine(reply.Message);
    }

    public async Task<string> LoadProfileAsync(string userPublicAddress)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5000");
        var client = new HushProfile.HushProfileClient(channel);
        var reply = await client.LoadProfileAsync(new LoadProfileRequest { ProfilePublicKey = userPublicAddress });

        return reply.Profile.UserName;
    }
}

public interface IHushProfileService
{
    Task SetProfileAsync();
    Task<string> LoadProfileAsync(string userPublicAddress);
}