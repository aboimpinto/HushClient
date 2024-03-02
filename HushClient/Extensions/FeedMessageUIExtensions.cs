using HushClient.ViewModels;
using Olimpo;

namespace HushClient.Extensions;

public static class FeedMessageUIExtensions
{
    public static FeedMessageUI DecryptFeedMessage(this FeedMessageUI feedMessageUI, string privateEncryptKey)
    {
        feedMessageUI.FeedMessageDecrypted = EncryptKeys.Decrypt(feedMessageUI.FeedMessageEncrypted, privateEncryptKey);
        return feedMessageUI;
    }

    public static FeedMessageUI SetOwnMessage(this FeedMessageUI feedMessageUI, string publicSigningAddress)
    {
        if (feedMessageUI.IssuerPublicKey == publicSigningAddress)
        {
            feedMessageUI.IsOwnMessage = true;
        }
        else
        {
            feedMessageUI.IsOwnMessage = false;
        }

        return feedMessageUI;
    }
}
