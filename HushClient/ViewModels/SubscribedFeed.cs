using HushEcosystem.Model.Blockchain;
using ReactiveUI;

namespace HushClient.ViewModels;

public class SubscribedFeed : ReactiveObject
{
    private string _feedId = string.Empty;
    private FeedTypeEnum _feedType;
    private string _publicAddressView = string.Empty;
    private string _publicAddress = string.Empty;
    private string _privateKey = string.Empty;

    public string FeedId 
    { 
        get => this._feedId; 
        set => this.RaiseAndSetIfChanged(ref this._feedId, value); 
    }

    public FeedTypeEnum FeedType 
    {
        get => this._feedType; 
        set => this.RaiseAndSetIfChanged(ref this._feedType, value); 
    }

    public string PublicAddressView
    {
        get => this._publicAddressView;
        set => this.RaiseAndSetIfChanged(ref this._publicAddressView, value);
    }

    public string PublicAddress
    {
        get => this._publicAddress;
        set => this.RaiseAndSetIfChanged(ref this._publicAddress, value);
    }

    public string PrivateKey 
    { 
        get => this._privateKey; 
        set => this.RaiseAndSetIfChanged(ref this._privateKey, value);
    }
}