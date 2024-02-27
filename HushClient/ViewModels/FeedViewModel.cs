using Olimpo.NavigationManager;
using Olimpo;
using System.Threading.Tasks;
using System.Collections.Generic;
using HushClient.Model;
using ReactiveUI;
using System;
using HushClient.Workflows;
using HushClient.GlobalEvents;
using System.Collections.ObjectModel;
using HushEcosystem.Model.Blockchain;

namespace HushClient.ViewModels;

public class FeedViewModel : 
    ViewModelBase, 
    ILoadableViewModel,
    IHandle<RefreshFeedMessagesEvent>
{
    private string _messageToSend;
    private SubscribedFeed _selectedFeed;
    private readonly IHushClientWorkflow _hushClientWorkflow;

    public BlockchainInformation BlockchainInformation { get; }
    public LocalInformation LocalInformation { get; }

    public ObservableCollection<FeedMessage> FeedMessages { get; }

    public string MessageToSend 
    { 
        get => this._messageToSend; 
        set => this.RaiseAndSetIfChanged(ref this._messageToSend, value); 
    }

    public FeedViewModel(
        IHushClientWorkflow hushClientWorkflow,
        BlockchainInformation blockchainInformation, 
        LocalInformation localInformation,
        IEventAggregator eventAggregator)
    {
        this._hushClientWorkflow = hushClientWorkflow;
        this.BlockchainInformation = blockchainInformation;
        this.LocalInformation = localInformation;

        eventAggregator.Subscribe(this);

        this.FeedMessages = new ObservableCollection<FeedMessage>();
    }

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        if (parameters != null && parameters.ContainsKey("SelectedSubscribedFeed"))
        {
            var selectedFeed = parameters["SelectedSubscribedFeed"] as SubscribedFeed;

            if (selectedFeed == null)
            {
                throw new InvalidOperationException("SelectedSubscribedFeed is not a SubscribedFeed");
            }

            this._selectedFeed = selectedFeed;
        }

        return Task.CompletedTask;
    }

    public void SendMessageCommand()
    {
        if (string.IsNullOrWhiteSpace(this.MessageToSend))
        {
            return;
        }

        this._hushClientWorkflow.SendMessage(this._selectedFeed.FeedId, this.MessageToSend);
    }

    public void Handle(RefreshFeedMessagesEvent message)
    {
        this.FeedMessages.Clear();

        this.LocalInformation.SubscribedFeedMessages[message.FeedId].ForEach(x => this.FeedMessages.Add(x));
    }
}