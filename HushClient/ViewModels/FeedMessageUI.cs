using ReactiveUI;

namespace HushClient.ViewModels;

public class FeedMessageUI : ReactiveObject
{
    private string _feedMessageId = string.Empty;
    private string _feedId = string.Empty;
    private string _issuerPublicKey = string.Empty;
    private string _feedMessageEncrypted = string.Empty;
    private string _feedMessageDecrypted = string.Empty;
    private bool _isFeedMessageConfirmed;
    private bool _isOwnMessage;
    private string _messageTime;

    public string FeedMessageId 
    { 
        get => this._feedMessageId; 
        set => this.RaiseAndSetIfChanged(ref this._feedMessageId, value); 
    }

    public string FeedId 
    { 
        get => this._feedId; 
        set => this.RaiseAndSetIfChanged(ref this._feedId, value); 
    }

    public string IssuerPublicKey 
    { 
        get => this._issuerPublicKey; 
        set => this.RaiseAndSetIfChanged(ref this._issuerPublicKey, value); 
    }

    public string FeedMessageEncrypted 
    { 
        get => this._feedMessageEncrypted; 
        set => this.RaiseAndSetIfChanged(ref this._feedMessageEncrypted, value); 
    }

    public string FeedMessageDecrypted 
    { 
        get => this._feedMessageDecrypted; 
        set => this.RaiseAndSetIfChanged(ref this._feedMessageDecrypted, value); 
    }

    public bool IsFeedMessageConfirmed 
    { 
        get => this._isFeedMessageConfirmed; 
        set => this.RaiseAndSetIfChanged(ref this._isFeedMessageConfirmed, value); 
    }

    public bool IsOwnMessage 
    { 
        get => this._isOwnMessage; 
        set => this.RaiseAndSetIfChanged(ref this._isOwnMessage, value); 
    }

    public string MessageTime 
    { 
        get => this._messageTime; 
        set => this.RaiseAndSetIfChanged(ref this._messageTime, value); 
    }
}