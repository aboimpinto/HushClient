using System.Collections.Generic;
using System.Threading.Tasks;
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

    public ViewModelBase CurrentOperation 
    { 
        get => this._currentOperation; 
        set => this.RaiseAndSetIfChanged(ref this._currentOperation, value); 
    }

    public MainViewModel(
        INavigationManager navigationManager,
        IHushClientWorkflow hushClientWorkflow,
        IEventAggregator eventAggregator)
    {
        this._navigationManager = navigationManager;
        this._hushClientWorkflow = hushClientWorkflow;
        this._eventAggregator = eventAggregator;

        this._navigationManager.RegisterNavigatableView(this);
    }

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        this._hushClientWorkflow.Start();
        
        return Task.CompletedTask;
    }
}
