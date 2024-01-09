﻿using HushClient.ApplicationSettings.Model;

namespace HushClient.ApplicationSettings;

public class ApplicationSettingsManager : IApplicationSettingsManager
{
    public UserInfo UserInfo { get; private set; }

    public BlockchainInfo BlockchainInfo { get; private set; }

    public Task LoadSettingsAsync()
    {
        this.UserInfo = new UserInfo
        {
            PublicSigningAddress = "04f505dab724c28db882376ca25d470cbabdc188f15a20a2083f49756bc00dd12ac44679afc9f4d658df07310f68191a633e31cd80ccf407a6a065d5b18874433f",
            PrivateSigningKey = "f875393235cde1b7b13d19ae8380a378f3631de81e350023bdcb9b71dd9a7970",
            PublicEncryptAddress = "PFJTQUtleVZhbHVlPjxNb2R1bHVzPndhdCtndzVBM1RTdGlTaHl0UDlqMjM2VVIrb0hBNXQwV3kzVGZmZ05tUmNKdkpkbGhraTd3QjFPRlF4MXFpVFNMdXJEZnRzd0UveWdzc3RWdUZ0M2VJNFpNZUVDYjBkYWxHeU5uYjlscTJsTE5hc1JCMmtoU3VUUE9MNTl3bGFwMi9aNUJVamZsRkQ4aG1Fc1RaNk9NWW93M3AyZ25ManQzaXpLYy8ybnN2OD08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjwvUlNBS2V5VmFsdWU+",
            PrivateEncryptKey = "PFJTQUtleVZhbHVlPjxNb2R1bHVzPndhdCtndzVBM1RTdGlTaHl0UDlqMjM2VVIrb0hBNXQwV3kzVGZmZ05tUmNKdkpkbGhraTd3QjFPRlF4MXFpVFNMdXJEZnRzd0UveWdzc3RWdUZ0M2VJNFpNZUVDYjBkYWxHeU5uYjlscTJsTE5hc1JCMmtoU3VUUE9MNTl3bGFwMi9aNUJVamZsRkQ4aG1Fc1RaNk9NWW93M3AyZ25ManQzaXpLYy8ybnN2OD08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjxQPjRCVjd5c0tneEdMSStwOTZvcW5VRzY0OHArQTAwNzZUeXh0WElqNS9VdXZpMUFtL0t3VEVrL0xiMzdOSVcyc21oY2tqZ2pieXpHK0VScnVvd1A4Z3Z3PT08L1A+PFE+M1VFUEVZSlIycEgxMkZwSHhJUEdRM2U3VjJpeEo5VURXZHZ4VkdpTlZKVGZ6RlUzUE5WaS9YNlhTVHRHQkJpdy9VOWpCcGJoODhoQ0xIVi9NNTI5d1E9PTwvUT48RFA+SlVzT0JpbXNEZU1PNWI4Qzd1MXFzb3lsNVo2SHpER3NjU0lFdDF0RlgyeUluSmRlckc4bnRDTzMraHZoVCsyZVJLZTc5Q0RtK2FVWms1Z0p0c3ovQ3c9PTwvRFA+PERRPmZoOE41ZDh4cGJRNlBlQUl0UDZneitpNmcvTUx1VGIxdUExbUhjV1Rlcmw2Y1ZIS01RVTZibUh3L3krb2s4RTNjczRFRnNkL2VhV1lBeHZmTEo4b2dRPT08L0RRPjxJbnZlcnNlUT50Mk1BdFJaRG5LVW5IZ1RwSHFxWDRDWUk3K0s2SW1CYmtXQnJaVEhlYkZQdE5HWHVCczYxa0s2djJyTlRWS3FzTTZpckgrbmZBTEM0MXl0aW1MZFdydz09PC9JbnZlcnNlUT48RD5oZ3RjbFBqWWpFSlptZ1VVMzVHa1c5ZFFhalhnaE82anFqZ2RtYUtHUnQ5YkxkemJ1QVcvYlVqYk1sWmRXbFlCNDljNVdDUDlNdGczb1loNjNBN3lzSm50SUF4MzJqL3p1OU9BTXVEc2JWN0lSOWNOdlpWRjdwTnBzalVHdHRsMWFNUGZaVXo1cERzdTF5Nm1EaWhqMGN2M1hYK01KRHhRVER0NWp0YnpXQUU9PC9EPjwvUlNBS2V5VmFsdWU+"
        };

        this.BlockchainInfo = new BlockchainInfo
        {
            LastHeightSynched = 0
        };

        return Task.CompletedTask;
    }

    public Task SetLastHeightAsync(int lastHeight)
    {
        this.BlockchainInfo.LastHeightSynched = lastHeight;
        return Task.CompletedTask;
    }
}
