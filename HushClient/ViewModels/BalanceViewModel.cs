using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using HushClient.Account;
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
    private readonly IAccountService _accountService;
    private readonly INavigationManager _navigationManager;

    public BlockchainInformation BlockchainInformation { get; }
    public LocalInformation LocalInformation { get; }
    public ObservableCollection<SubscribedFeed> SubscribedFeeds { get; }
    public ReactiveCommand<SubscribedFeed, Unit> FeedSelectCommand { get; }

    public BalanceViewModel(
        BlockchainInformation blockchainInformation, 
        IAccountService accountService,
        LocalInformation localInformation,
        INavigationManager navigationManager,
        IEventAggregator eventAggregator)
    {
        this.BlockchainInformation = blockchainInformation;
        this._accountService = accountService;
        this.LocalInformation = localInformation;
        this._navigationManager = navigationManager;

        eventAggregator.Subscribe(this);

        this.SubscribedFeeds = new ObservableCollection<SubscribedFeed>();
        this.FeedSelectCommand = ReactiveCommand.Create<SubscribedFeed>(this.OnFeedSelect);
    }

    public async Task NewFeedCommand()
    {
        await this._navigationManager.NavigateAsync("SearchAccountsViewModel");
    }

    private void OnFeedSelect(SubscribedFeed subscribedFeed)
    {
        var navigateParameters = new Dictionary<string, object>
        {
            { "SelectedSubscribedFeed", subscribedFeed }
        };

        this._navigationManager.NavigateAsync("FeedViewModel", navigateParameters);
    }

    public void Handle(RefreshFeedsEvent message)
    {
        Console.WriteLine("Refreshing feeds...");

        this.LocalInformation.SubscribedFeedsDefinitions.ForEach(x => 
        {
            var personalFeed = new SubscribedFeed
            {
                FeedId = x.FeedId,
                FeedType = x.FeedType,
                PublicAddressView = $"{this._accountService.UserProfile.ProfileName} (You)",
                PublicAddress = x.FeedParticipant
            };

            this.SubscribedFeeds.Add(personalFeed);
        });

        // this.LocalInformation.SubscribedFeeds.ForEach(x => 
        // {
        //     if (this.SubscribedFeeds.Any(f => f.FeedId == x.FeedId))
        //     {
        //         var existingFeed = this.SubscribedFeeds.Single(x => x.FeedId == x.FeedId);
        //         existingFeed.PublicAddress = x.FeedPublicEncriptAddress;
        //         existingFeed.PrivateKey = x.FeedPrivateEncriptAddress;
        //     }
        //     else
        //     {
        //         // can only have one personal feed
        //         if (this.SubscribedFeeds.IsPersonalFeedUnique())
        //         {
        //             if (x.FeedType == FeedTypeEnum.Chat)
        //             {
        //                 var chatFeed = new SubscribedFeed
        //                 {
        //                     FeedId = x.FeedId,
        //                     FeedType = x.FeedType,
        //                     PublicAddressView = x.FeedParticipantPublicAddress.Truncate(10),
        //                     PublicAddress = x.FeedPublicEncriptAddress,
        //                     PrivateKey = x.FeedPrivateEncriptAddress
        //                 };

        //                 this.SubscribedFeeds.Add(chatFeed);
        //             }

        //             return;
        //         }

        //         var personalFeed = new SubscribedFeed
        //         {
        //             FeedId = x.FeedId,
        //             FeedType = x.FeedType,
        //             PublicAddressView = $"{this._accountService.UserProfile.ProfileName} (You)",
        //             PublicAddress = x.FeedPublicEncriptAddress,
        //             PrivateKey = x.FeedPrivateEncriptAddress
        //         };

        //         this.SubscribedFeeds.Add(personalFeed);
        //     }
        // });
    }
}