using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Olimpo.NavigationManager;
using Olimpo;
using HushClient.Model;
using HushClient.GlobalEvents;
using HushClient.Workflows;
using HushClient.Account;

namespace HushClient.ViewModels;

public class FeedViewModel : 
    ViewModelBase, 
    ILoadableViewModel,
    IHandle<RefreshFeedMessagesEvent>
{
    private string _messageToSend;
    private SubscribedFeed _selectedFeed;
    private bool _messageToSentFocus;
    private bool _scrollMessageToEnd;
    private string _feedName;
    private readonly IHushClientWorkflow _hushClientWorkflow;
    private readonly IAccountService _accountService;
    private readonly INavigationManager _navigationManager;

    public BlockchainInformation BlockchainInformation { get; }
    public LocalInformation LocalInformation { get; }

    public string FeedName 
    { 
        get => this._feedName; 
        set => this.RaiseAndSetIfChanged(ref this._feedName, value); 
    }

    public bool MessageToSentFocus 
    { 
        get => this._messageToSentFocus; 
        set => this.RaiseAndSetIfChanged(ref this._messageToSentFocus, value); 
    }

    public bool ScrollMessageToEnd 
    {
        get => this._scrollMessageToEnd; 
        set => this.RaiseAndSetIfChanged(ref this._scrollMessageToEnd, value); 
    }

    public ObservableCollection<FeedMessageUI> FeedMessages { get; private set; }

    public string MessageToSend 
    { 
        get => this._messageToSend; 
        set => this.RaiseAndSetIfChanged(ref this._messageToSend, value); 
    }

    public FeedViewModel(
        IHushClientWorkflow hushClientWorkflow,
        BlockchainInformation blockchainInformation, 
        LocalInformation localInformation,
        IAccountService accountService,
        INavigationManager navigationManager,
        IEventAggregator eventAggregator)
    {
        this._hushClientWorkflow = hushClientWorkflow;
        this.BlockchainInformation = blockchainInformation;
        this.LocalInformation = localInformation;
        this._accountService = accountService;
        this._navigationManager = navigationManager;
        eventAggregator.Subscribe(this);
    }

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        this.FeedMessages = new ObservableCollection<FeedMessageUI>();

        if (parameters != null && parameters.ContainsKey("SelectedSubscribedFeed"))
        {
            var selectedFeed = parameters["SelectedSubscribedFeed"] as SubscribedFeed;

            if (selectedFeed == null)
            {
                throw new InvalidOperationException("SelectedSubscribedFeed is not a SubscribedFeed");
            }

            this._selectedFeed = selectedFeed;

            this.FeedName = this.CalculateFeedName(selectedFeed);

            if (this.LocalInformation.SubscribedFeedMessages.ContainsKey(selectedFeed.FeedId))
            {
                this.LocalInformation.SubscribedFeedMessages[selectedFeed.FeedId]
                    .TakeLast(100)                              //  Only take the last 100 messages
                    .ForEach(x => 
                    {
                        this.FeedMessages.Add(
                            x
                                .ToFeedMessageUI(true)
                                .SetOwnMessage(this._accountService.UserProfile.PublicSigningAddress)
                                .DecryptFeedMessage(this._accountService.UserProfile.PrivateEncryptKey));
                    });

                this.ScrollMessageToEnd = true;
            }
        }

        return Task.CompletedTask;
    }

    public async Task SendMessageCommand()
    {
        if (string.IsNullOrWhiteSpace(this.MessageToSend))
        {
            return;
        }

        var sentMessage = await this._hushClientWorkflow.SendMessage(this._selectedFeed.FeedId, this.MessageToSend);
        if (sentMessage != null)
        {
            this.FeedMessages.Add(
                sentMessage
                    .ToFeedMessageUI()
                    .SetOwnMessage(this._accountService.UserProfile.PublicSigningAddress)
                    .DecryptFeedMessage(this._accountService.UserProfile.PrivateEncryptKey));
        }

        this.MessageToSend = string.Empty;
        this.MessageToSentFocus = true;
        this.ScrollMessageToEnd = true;
    }

    public void BackCommand()
    {
        this._navigationManager.NavigateAsync("BalanceViewModel");
    }

    public void Handle(RefreshFeedMessagesEvent message)
    {
    }

    // public void Handle(FeedMessageTransactionHandledEvent message)
    // {
    //     if (message.FeedMessage.FeedId != this._selectedFeed.FeedId)
    //     {
    //         return;
    //     }

    //     // Confirm the message that should be already in the list
    //     var messageToConfirm = this.FeedMessages.FirstOrDefault(x => x.FeedMessageId == message.FeedMessage.FeedMessageId);
    //     if (messageToConfirm != null)
    //     {
    //         messageToConfirm.IsFeedMessageConfirmed = true;    
    //     }
    //     else
    //     {
    //         // TODO [AboimPinto] if the total of messages ir greater than 100, remove the first one
    //         this.FeedMessages.Add(
    //             message.FeedMessage
    //                 .ToFeedMessageUI(false)
    //                 .SetOwnMessage(this._accountService.UserProfile.PublicSigningAddress)
    //                 .DecryptFeedMessage(this._accountService.UserProfile.PrivateEncryptKey));
    //     }

    //     this.ScrollMessageToEnd = true;
    // }

    private string CalculateFeedName(SubscribedFeed selectedFeed)
    {
        if (selectedFeed.FeedType == HushEcosystem.Model.Blockchain.FeedTypeEnum.Personal)
        {
            return $"{this._accountService.UserProfile.ProfileName} (You)";
        }

        return "Unknown Feed Name";
    }
}
