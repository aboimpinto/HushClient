using Olimpo.NavigationManager;
using Olimpo;
using System.Threading.Tasks;
using System.Collections.Generic;
using HushClient.Model;
using ReactiveUI;
using System;
using HushEcosystem.Model.Rpc.Feeds;
using HushClient.Workflows;

namespace HushClient.ViewModels;

public class FeedViewModel : ViewModelBase, ILoadableViewModel
{
    private string _messageToSend;
    private SubscribedFeed _selectedFeed;
    private readonly IHushClientWorkflow _hushClientWorkflow;

    public BlockchainInformation BlockchainInformation { get; }
    public LocalInformation LocalInformation { get; }

    public string MessageToSend 
    { 
        get => this._messageToSend; 
        set => this.RaiseAndSetIfChanged(ref this._messageToSend, value); 
    }

    public FeedViewModel(
        IHushClientWorkflow hushClientWorkflow,
        BlockchainInformation blockchainInformation, 
        LocalInformation localInformation)
    {
        this._hushClientWorkflow = hushClientWorkflow;
        this.BlockchainInformation = blockchainInformation;
        this.LocalInformation = localInformation;
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
}
