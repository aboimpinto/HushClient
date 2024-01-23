using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HushClient.Views;

public partial class BalanceView : UserControl
{
    public BalanceView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}