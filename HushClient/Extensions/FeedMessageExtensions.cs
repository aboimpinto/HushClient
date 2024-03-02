using HushClient.ViewModels;
using HushEcosystem.Model.Blockchain;

namespace HushClient.Extensions;

public static class FeedMessageExtensions
{
    public static FeedMessageUI ToFeedMessageUI(this FeedMessage feedMessage, bool isMessageConfirmed = false)
    {
        return new FeedMessageUI
        {
            FeedMessageId = feedMessage.FeedMessageId,
            FeedId = feedMessage.FeedId,
            IssuerPublicKey = feedMessage.Issuer.Truncate(10),
            FeedMessageEncrypted = feedMessage.Message,
            IsFeedMessageConfirmed = isMessageConfirmed,
            MessageTime = feedMessage.TimeStamp.ToString("HH:mm")    
        };
    }
}
