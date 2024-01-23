using ReactiveUI;

namespace HushClient.Model;

public class BlockchainInformation : ReactiveObject
{
    private double _blockchainHeight;

    public double BlockchainHeight 
    { 
        get => this._blockchainHeight; 
        set => this.RaiseAndSetIfChanged(ref this._blockchainHeight, value); 
    }
}
