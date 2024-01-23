using HushClient.Model;
using Olimpo.NavigationManager;

namespace HushClient.ViewModels;

public class BalanceViewModel : ViewModelBase
{
    public BlockchainInformation BlockchainInformation { get; }
    public LocalInformation LocalInformation { get; }

    public BalanceViewModel(
        BlockchainInformation blockchainInformation, 
        LocalInformation localInformation)
    {
        BlockchainInformation = blockchainInformation;
        LocalInformation = localInformation;
    }
}
