using System.Collections.Generic;
using System.Linq;
using HushClient.ViewModels;
using HushEcosystem.Model.Blockchain;

namespace HushClient;

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
