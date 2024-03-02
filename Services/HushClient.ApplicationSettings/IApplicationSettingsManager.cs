using HushClient.ApplicationSettings.Model;

namespace HushClient.ApplicationSettings;

public interface IApplicationSettingsManager
{
    UserProfile UserProfile { get; }

    BlockchainInfo BlockchainInfo { get; }

    Task LoadSettingsAsync();   

    Task SetLastHeightAsync(int lastHeight);
}
