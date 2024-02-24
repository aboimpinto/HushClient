using Olimpo.NavigationManager;
using Olimpo;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HushClient.ViewModels;

public class FeedViewModel : ViewModelBase, ILoadableViewModel
{
    public FeedViewModel()
    {
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
