using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HushClient.Views;

public partial class FeedView : UserControl
{
    public FeedView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}