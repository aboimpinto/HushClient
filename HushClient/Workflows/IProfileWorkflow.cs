using System.Threading.Tasks;
using HushClient.Account.Model;

namespace HushClient.Workflows;

public interface IProfileWorkflow
{
    Task SetNewProfileAsync(UserProfile userProfile);

    Task LoadAccountsAsync();

    Task SearchAccountByPublicKeyAsync(string publicKey);
}