namespace HushClient.ApplicationSettings.Model;

public class UserInfo
{
    public string PublicSigningAddress { get; set; } = string.Empty;

    public string PrivateSigningKey { get; set; } = string.Empty;
    
    public string PublicEncryptAddress { get; set; } = string.Empty;
    
    public string PrivateEncryptKey { get; set; } = string.Empty;
}
