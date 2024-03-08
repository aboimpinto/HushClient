using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using HushClient.Account.Model;
using Olimpo.NavigationManager;

namespace HushClient.Account;

public class AccountService : IAccountService
{
    private string _accountsFilePath;
    private readonly INavigationManager _navigationManager;

    public UserProfile UserProfile { get; private set; }

    public AccountService(
        INavigationManager navigationManager)
    {
        this._navigationManager = navigationManager;

        this._accountsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Accounts.json");
    }

    public async Task LoadAccountsAsync()
    {
        if (File.Exists(this._accountsFilePath))
        {
            using (var readerStream = new FileStream(this._accountsFilePath, FileMode.Open))
            {
                var userProfile = await JsonSerializer.DeserializeAsync<UserProfile>(readerStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (userProfile != null)
                {
                    this.UserProfile = userProfile;
                }
                else
                {
                    await this._navigationManager.NavigateAsync("NewAccountViewModel");
                }
            }

        }
        else
        {
            await this._navigationManager.NavigateAsync("NewAccountViewModel");
        }
    }

    public async Task SaveProfileToJsonAsync(UserProfile userProfile)
    {
        this.UserProfile = userProfile;

        using (var writerStream = new FileStream(this._accountsFilePath, FileMode.CreateNew))
        {
            await JsonSerializer.SerializeAsync(writerStream, userProfile, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public async Task UpdateProfileUserNameAsync(string profileUserName)
    {
        if (this.UserProfile.ProfileName == profileUserName)
        {
            return;
        }

        this.UserProfile.ProfileName = profileUserName;
        await this.SaveProfileToJsonAsync(this.UserProfile);
    }
}
