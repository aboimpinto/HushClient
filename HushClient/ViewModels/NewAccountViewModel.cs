using HushClient.Account.Model;
using HushClient.ApplicationSettings;
using HushClient.ApplicationSettings.Model;
using HushClient.GlobalEvents;
using HushClient.Workflows;
using Olimpo;
using Olimpo.NavigationManager;
using ReactiveUI;

namespace HushClient.ViewModels;

public class NewAccountViewModel : ViewModelBase
{
    private string _profileName = string.Empty;
    private string _signingPublicKey = string.Empty;
    private string _signingPrivateKey = string.Empty;
    private string _encryptPublicKey = string.Empty;
    private string _encryptPrivateKey = string.Empty;
    private bool _areKeysGenerated;
    private bool _isProfileNameValid = false;
    private readonly IApplicationSettingsManager _applicationSettingsManager;
    private readonly IProfileWorkflow _profileWorkflow;
    private readonly IEventAggregator _eventAggregator;

    public string ProfileName 
    { 
        get => this._profileName; 
        set => this.RaiseAndSetIfChanged(ref this._profileName, value); 
    }

    public string SigningPublicKey 
    { 
        get => this._signingPublicKey; 
        set => this.RaiseAndSetIfChanged(ref this._signingPublicKey, value); 
    }

    public string SigningPrivateKey 
    { 
        get => this._signingPrivateKey; 
        set => this.RaiseAndSetIfChanged(ref this._signingPrivateKey, value); 
    }

    public string EncryptPublicKey 
    { 
        get => this._encryptPublicKey; 
        set => this.RaiseAndSetIfChanged(ref this._encryptPublicKey, value); 
    }

    public string EncryptPrivateKey 
    { 
        get => this._encryptPrivateKey; 
        set => this.RaiseAndSetIfChanged(ref this._encryptPrivateKey, value); 
    }

    public bool AreKeysGenerated 
    { 
        get => this._areKeysGenerated; 
        set => this.RaiseAndSetIfChanged(ref this._areKeysGenerated, value); 
    }

    public NewAccountViewModel(
        IApplicationSettingsManager applicationSettingsManager,
        IProfileWorkflow profileWorkflow,
        IEventAggregator eventAggregator)
    {
        this._applicationSettingsManager = applicationSettingsManager;
        this._profileWorkflow = profileWorkflow;
        this._eventAggregator = eventAggregator;
    }

    public void GenerateKeysCommand()
    {
        var signingKeys = new SigningKeys();
        this.SigningPublicKey = signingKeys.PublicAddress;
        this.SigningPrivateKey = signingKeys.PrivateKey;

        var encryptKeys = new EncryptKeys();
        this.EncryptPublicKey = encryptKeys.PublicKey;
        this.EncryptPrivateKey = encryptKeys.PrivateKey;

        this.AreKeysGenerated = true;
    }

    public void CreateProfileCommand()
    {
        if (string.IsNullOrEmpty(this.ProfileName) || !this._isProfileNameValid)
        {
            return;
        }

        if (!this.AreKeysGenerated)
        {
            return;
        }

        var userProfile = new UserProfile
        {
            ProfileName = this.ProfileName,
            PublicSigningAddress = this.SigningPublicKey,
            PrivateSigningKey = this.SigningPrivateKey,
            PublicEncryptAddress = this.EncryptPublicKey,
            PrivateEncryptKey = this.EncryptPrivateKey,
            IsPublic = true
        };

        // Save the keys into the account file

        // Send the profile to the Blockchain
        this._profileWorkflow.SetNewProfileAsync(userProfile);
    }

    public void ProfileNameChangedCommand()
    {
        if (this.ProfileName.Length > 3)
        {
            this._isProfileNameValid = true;
        }
        else
        {
            this._isProfileNameValid = false;
        }
    }
}
