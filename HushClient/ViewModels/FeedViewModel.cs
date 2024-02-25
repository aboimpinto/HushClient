using Olimpo.NavigationManager;
using Olimpo;
using System.Threading.Tasks;
using System.Collections.Generic;
using HushClient.Model;

namespace HushClient.ViewModels;

public class FeedViewModel : ViewModelBase, ILoadableViewModel
{
    public BlockchainInformation BlockchainInformation { get; }
    public LocalInformation LocalInformation { get; }

    public FeedViewModel(
        BlockchainInformation blockchainInformation, 
        LocalInformation localInformation)
    {
        this.BlockchainInformation = blockchainInformation;
        this.LocalInformation = localInformation;
    }

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        if (parameters != null && parameters.ContainsKey("SelectedSubcibedFeed"))
        {
            var selectedFeed = parameters["SelectedSubcibedFeed"] as SubscribedFeed;
            // Do something with the selected feed
        }

        return Task.CompletedTask;
    }
}
