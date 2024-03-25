using System.Collections.Generic;
using System.Threading.Tasks;
using HushClient.Model;
using HushClient.Workflows;
using Olimpo;
using Olimpo.NavigationManager;
using ReactiveUI;

namespace HushClient.ViewModels;

public class MainViewModel :
    ViewModelBase,
    INavigatableView,
    ILoadableViewModel
{
    private readonly INavigationManager _navigationManager;
    private readonly IHushClientWorkflow _hushClientWorkflow;
    private readonly IEventAggregator _eventAggregator;
    private ViewModelBase _currentOperation;

    public BlockchainInformation BlockchainInformation { get; private set; }
    public LocalInformation LocalInformation { get; private set; }

    public ViewModelBase CurrentOperation 
    { 
        get => this._currentOperation; 
        set => this.RaiseAndSetIfChanged(ref this._currentOperation, value); 
    }

    public MainViewModel(
        BlockchainInformation blockchainInformation,
        LocalInformation localInformation,
        INavigationManager navigationManager,
        IHushClientWorkflow hushClientWorkflow,
        IEventAggregator eventAggregator)
    {
        this.BlockchainInformation = blockchainInformation;
        this.LocalInformation = localInformation;
        this._navigationManager = navigationManager;
        this._hushClientWorkflow = hushClientWorkflow;
        this._eventAggregator = eventAggregator;

        this._navigationManager.RegisterNavigatableView(this);
    }

    public async Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        await this._hushClientWorkflow.Start();
    }
}
