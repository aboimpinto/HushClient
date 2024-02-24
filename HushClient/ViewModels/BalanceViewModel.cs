using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using HushClient.GlobalEvents;
using HushClient.Model;
using HushEcosystem.Model.Blockchain;
using Olimpo;
using Olimpo.NavigationManager;
using ReactiveUI;

namespace HushClient.ViewModels;

public class BalanceViewModel : 
    ViewModelBase,
    IHandle<RefreshFeedsEvent>
{
    private readonly INavigationManager _navigationManager;

    public BlockchainInformation BlockchainInformation { get; }
    public LocalInformation LocalInformation { get; }

    public ObservableCollection<SubscribedFeed> SubscribedFeeds { get; }
    public ReactiveCommand<SubscribedFeed, Unit> FeedSelectCommand { get; }


    public BalanceViewModel(
        BlockchainInformation blockchainInformation, 
        LocalInformation localInformation,
        INavigationManager navigationManager,
        IEventAggregator eventAggregator)
    {
        this.BlockchainInformation = blockchainInformation;
        this.LocalInformation = localInformation;
        this._navigationManager = navigationManager;
        eventAggregator.Subscribe(this);

        this.SubscribedFeeds = new ObservableCollection<SubscribedFeed>();
        this.FeedSelectCommand = ReactiveCommand.Create<SubscribedFeed>(this.OnFeedSelect);
    }

    private void OnFeedSelect(SubscribedFeed subscribedFeed)
    {
        var navigateParameters = new Dictionary<string, object>
        {
            { "SelectedSubcibedFeed", subscribedFeed }
        };

        this._navigationManager.NavigateAsync("FeedViewModel", navigateParameters);
    }

    public void Handle(RefreshFeedsEvent message)
    {
        this.LocalInformation.SubscribedFeeds.ForEach(x => 
        {
            if (this.SubscribedFeeds.Any(f => f.FeedId == x.FeedId))
            {
                var existingFeed = this.SubscribedFeeds.Single(x => x.FeedId == x.FeedId);
                existingFeed.PublicAddress = x.FeedPublicEncriptAddress;
                existingFeed.PrivateKey = x.FeedPrivateEncriptAddress;
            }
            else
            {
                // can only have one personal feed
                if (this.SubscribedFeeds.IsPersonalFeedUnique())
                {
                    return;
                }

                var feed = new SubscribedFeed
                {
                    FeedId = x.FeedId,
                    FeedType = x.FeedType,
                    PublicAddressView = x.FeedPublicEncriptAddress.Truncate(10),
                    PublicAddress = x.FeedPublicEncriptAddress,
                    PrivateKey = x.FeedPrivateEncriptAddress
                };

                this.SubscribedFeeds.Add(feed);
            }
        });
    }
}

public static class SubscribedFeedsExtensions
{
    public static bool IsPersonalFeedUnique(this IEnumerable<SubscribedFeed> source)
    {
        var uniqueFeed = source.SingleOrDefault(x => x.FeedType == FeedTypeEnum.Personal);
        if (uniqueFeed == null)
        {
            return false;
        }

        return true;
    }
}

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

public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) 
        {
            return value;
        }

        return value.Length <= maxLength ? value : value[..maxLength]; 
    }
}