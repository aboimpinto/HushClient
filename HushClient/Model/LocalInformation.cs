using ReactiveUI;

namespace HushClient.Model;

public class LocalInformation : ReactiveObject
{
    private double _lastHeightSynched;
    private bool _isSynching;
    private double _balance;

    public double LastHeightSynched 
    { 
        get => this._lastHeightSynched; 
        set => this.RaiseAndSetIfChanged(ref this._lastHeightSynched, value); 
    }

    public bool IsSynching 
    {
        get => this._isSynching; 
        set => this.RaiseAndSetIfChanged(ref this._isSynching, value); 
    }

    public double Balance 
    { 
        get => this._balance; 
        set => this.RaiseAndSetIfChanged(ref this._balance, value); 
    }
}
