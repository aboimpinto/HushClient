using HushClient.ApplicationSettings.Model;

namespace HushClient.ApplicationSettings;

public interface IApplicationSettingsManager
{
    UserInfo UserInfo { get; }

    BlockchainInfo BlockchainInfo { get; }

    Task LoadSettingsAsync();   

    Task SetLastHeightAsync(int lastHeight);
}
