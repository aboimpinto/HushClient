using System.Collections.Generic;
using HushEcosystem.Model.Blockchain;
using HushEcosystem.Model.Rpc.Feeds;
using ReactiveUI;

namespace HushClient.Model;

public class LocalInformation : ReactiveObject
{
    private double _lastHeightSynched;
    private double _balance;
    private double _lastFeedHeightSynched = 0;
    private double _lastFeedMessageHeightSynched;

    public double LastHeightSynched 
    { 
        get => this._lastHeightSynched; 
        set => this.RaiseAndSetIfChanged(ref this._lastHeightSynched, value); 
    }

    public double LastFeedHeightSynched 
    { 
        get => this._lastFeedHeightSynched; 
        set => this.RaiseAndSetIfChanged(ref this._lastFeedHeightSynched, value); 
    }

    public double LastFeedMessageHeightSynched 
    {
        get => this._lastFeedMessageHeightSynched; 
        set => this.RaiseAndSetIfChanged(ref this._lastFeedMessageHeightSynched, value); 
    }

    public double Balance 
    { 
        get => this._balance; 
        set => this.RaiseAndSetIfChanged(ref this._balance, value); 
    }

    public IList<IFeedDefinition> SubscribedFeedsDefinitions { get; set; } = [];

    public Dictionary<string, IList<FeedMessage>> SubscribedFeedMessages { get; set; } = [];
}
