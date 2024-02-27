namespace HushClient.GlobalEvents;

public class RefreshFeedMessagesEvent
{
    public string FeedId { get; } = string.Empty;

    public RefreshFeedMessagesEvent(string feedId)
    {
        this.FeedId = feedId;
    }
}
