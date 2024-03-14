using System.Collections.Generic;
using System.Reactive.Subjects;
using HushEcosystem.Model.Blockchain;
using HushEcosystem.Model.Rpc.Feeds;
using ReactiveUI;

namespace HushClient.Model;

public class LocalInformation : ReactiveObject
{
    private double _lastHeightSynched;
    private bool _isSynching;
    private double _balance;

    public Subject<bool> IsSynchingStream { get; set; } = new Subject<bool>();

    public double LastHeightSynched 
    { 
        get => this._lastHeightSynched; 
        set => this.RaiseAndSetIfChanged(ref this._lastHeightSynched, value); 
    }

    public bool IsSynching 
    {
        get => this._isSynching; 
        set 
        {
            this.RaiseAndSetIfChanged(ref this._isSynching, value); 
            this.IsSynchingStream.OnNext(value);
        }
    }

    public double Balance 
    { 
        get => this._balance; 
        set => this.RaiseAndSetIfChanged(ref this._balance, value); 
    }

    public IList<Feed> SubscribedFeeds { get; set; } = [];

    public IList<FeedDefinition> SubscribedFeedsDefinitions { get; set; } = [];

    public Dictionary<string, IList<FeedMessage>> SubscribedFeedMessages { get; set; } = [];
}
