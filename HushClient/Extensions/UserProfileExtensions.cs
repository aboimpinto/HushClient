using HushClient.Account.Model;

namespace HushClient;

public static class UserProfileExtensions
{
    public static UserProfile ToLocalUserProfile(this HushEcosystem.Model.Blockchain.UserProfile userProfile)
    {
        return new UserProfile
        {
            PublicEncryptAddress = userProfile.UserPublicEncryptAddress,
            PublicSigningAddress = userProfile.UserPublicSigningAddress,
            ProfileName = userProfile.UserName,
            IsPublic = userProfile.IsPublic
        };
    }
}
