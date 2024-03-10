using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HushClient.Views;

public partial class SearchAccountsView : UserControl
{
    public SearchAccountsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}