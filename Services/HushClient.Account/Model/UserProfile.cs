namespace HushClient.Account.Model;

public class UserProfile
{
    public string ProfileName { get; set; } = string.Empty;

    public string PublicSigningAddress { get; set; } = string.Empty;

    public string PrivateSigningKey { get; set; } = string.Empty;
    
    public string PublicEncryptAddress { get; set; } = string.Empty;
    
    public string PrivateEncryptKey { get; set; } = string.Empty;

    public bool IsPublic { get; set; }
}
