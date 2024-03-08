using System.Threading.Tasks;
using HushClient.Account.Model;

namespace HushClient.Account;

public interface IAccountService
{
    UserProfile UserProfile { get; }

    Task LoadAccountsAsync();

    Task SaveProfileToJsonAsync(UserProfile userProfile);

    Task UpdateProfileUserNameAsync(string profileUserName);
}
